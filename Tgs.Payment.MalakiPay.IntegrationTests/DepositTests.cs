using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Stateless;
using Tgs.Payment.Clients;
using Tgs.Payment.MalakiPay.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.Payment.MalakiPay.IntegrationTests;

[TestClass]
public class DepositTests
{
	[TestMethod]
	public async Task DepositCallback()
	{
		var fakeGetDepositFormService = Substitute.For<IGetService<GetDepositFormQuery, DepositForm?>>();
		var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		await using var web = new TestMalakiPayApplication(builder => builder
			.ConfigureServices(services => services
				.AddSingleton(fakeGetDepositFormService)
				.AddSingleton(fakePaymentServiceClient)
				.AddSingleton(fakePaymentClient)));

		var httpClient = web.CreateClient();

		var cryptor = web.Services.GetRequiredService<TuPayHasher>();

		var depositFormId = Guid.NewGuid();
		var formDict = new Dictionary<string, string>
		{
			["tradeno"] = "123",
			["outtradeno"] = depositFormId.ToString(),
			["amount"] = "1000",
			["ramount"] = "1001",
			["oamount"] = "1000",
			["endtime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
			["status"] = "1",
			["remark"] = "test"
		};

		var merchantId = "2e2e51902ca34026915b59be8623efec";
		var secret = "28ac492de45f44ccb7b7d700e5a82928";

		var sign = cryptor.ComputeHash(secret, formDict);

		formDict["sign"] = sign;

		var applyInfo = new MalakiPayDepositApplyInfo(
			new MalakiPayDepositSettings(
				merchantId,
				secret,
				"Test",
				depositFormId),
			new MalakiPayDepositResponse(
				"123",
				1000M,
				1000M,
				PayTypes.GCash,
				"test",
				"111111",
				"bank",
				"subBank",
				"testPlayer",
				new Uri("http://localhost"),
				DateTime.Now,
				DateTime.Now,
				new Uri("http://localhost")));

		_ = fakeGetDepositFormService.GetAsync(Arg.Any<GetDepositFormQuery>())
			.Returns(new DepositForm(
				depositFormId,
				"player1",
				Guid.NewGuid(),
				Guid.NewGuid(),
				1000M,
				PaymentFlowState.WaitPayment,
				applyInfo));

		var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(PaymentFlowState.WaitPayment);

		_ = fsm.Configure(PaymentFlowState.Init)
			.Permit(PaymentTrigger.DepositCreate, PaymentFlowState.WaitPayment)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitPayment)
			.Permit(PaymentTrigger.PaymentDepositCompleted, PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		fakePaymentServiceClient.CreateDepositStateMachine(Arg.Is(PaymentFlowState.WaitPayment))
			.Returns(fsm);

		var requestContent = new FormUrlEncodedContent(formDict);

		var response = await httpClient.PostAsync("/deposit/callback", requestContent);

		var actual = await response.Content.ReadAsStringAsync();

		Assert.AreEqual("success", actual);

		_ = fakePaymentServiceClient.Received(1)
			.SubmitDepositFormStatusAsync(
				Arg.Is(depositFormId),
				Arg.Is((PaymentFlowState?)null),
				Arg.Any<string>());
		_ = fakePaymentClient.Received(1)
			.ApproveDepositAsync(
				Arg.Is(depositFormId),
				Arg.Any<Guid>(),
				Arg.Is(1001M));
	}
}
