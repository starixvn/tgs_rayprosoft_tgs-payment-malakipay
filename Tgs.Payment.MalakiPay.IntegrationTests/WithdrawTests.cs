using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Stateless;
using Tgs.Payment.Clients;
using Tgs.Payment.MalakiPay.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.Payment.MalakiPay.IntegrationTests;

[TestClass]
public class WithdrawTests
{
	[TestMethod]
	public async Task WithdrawCallback()
	{
		var fakeGetWithdrawFormQueryService = Substitute.For<IGetService<GetWithdrawFormQuery, WithdrawForm?>>();
		var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		await using var web = new TestMalakiPayApplication(builder => builder
			.ConfigureServices(services => services
				.AddSingleton(fakeGetWithdrawFormQueryService)
				.AddSingleton(fakePaymentServiceClient)
				.AddSingleton(fakePaymentClient)));

		var httpClient = web.CreateClient();

		var cryptor = web.Services.GetRequiredService<TuPayHasher>();

		var merchantId = "2e2e51902ca34026915b59be8623efec";
		var secret = "28ac492de45f44ccb7b7d700e5a82928";

		var withdrawFormId = Guid.NewGuid();
		var formDict = new Dictionary<string, string>
		{
			["transferno"] = "123",
			["outtransferno"] = withdrawFormId.ToString(),
			["tradeamount"] = "1000",
			["transferamount"] = "1001",
			["endtime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
			["status"] = "1",
			["remark"] = "test"
		};

		var sign = cryptor.ComputeHash(secret, formDict);

		formDict["sign"] = sign;

		var applyInfo = new MalakiPayWithdrawApplyInfo(
			new MalakiPayWithdrawSettings(
				merchantId,
				secret,
				"bank",
				"subBank",
				"111111111",
				"test",
				withdrawFormId,
				"test"),
			new MalakiPayWithdrawResponse(
				"123",
				1000M,
				1001M,
				DateTime.Now,
				null,
				0));

		_ = fakeGetWithdrawFormQueryService.GetAsync(Arg.Any<GetWithdrawFormQuery>())
			.Returns(new WithdrawForm(
				withdrawFormId,
				"player1",
				UserType.Player,
				Guid.NewGuid(),
				Guid.NewGuid(),
				1000M,
				PaymentFlowState.Approved,
				Guid.NewGuid(),
				applyInfo));

		var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(PaymentFlowState.Approved);

		_ = fsm.Configure(PaymentFlowState.Init)
			.Permit(PaymentTrigger.WithdrawCreate, PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.Approved)
			.Permit(PaymentTrigger.PaymentWithdrawCompleted, PaymentFlowState.Paid)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);


		_ = fakePaymentServiceClient.CreateWithdrawStateMachine(Arg.Is(PaymentFlowState.Approved))
			.Returns(fsm);

		var requestContent = new FormUrlEncodedContent(formDict);

		var response = await httpClient.PostAsync("/withdraw/callback", requestContent);

		var actual = await response.Content.ReadAsStringAsync();

		Assert.AreEqual("success", actual);

		_ = fakePaymentServiceClient.Received(1)
			.SubmitWithdrawFormStatusAsync(
				Arg.Is(withdrawFormId),
				Arg.Is(default(PaymentFlowState?)),
				Arg.Any<string?>());
		_ = fakePaymentClient.Received(1)
			.PaidWithdrawAsync(
				Arg.Is(withdrawFormId),
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>());
	}
}
