using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.Payment.MalakiPay.ApplicationServices;
using Tgs.ServiceUtility;

using WithdrawStateMachine = Stateless.StateMachine<Tgs.Payment.Clients.PaymentFlowState, Tgs.Payment.Clients.PaymentTrigger>;

namespace Tgs.Payment.MalakiPay.Controllers;

[Route("[controller]")]
[ApiController]
public class WithdrawController : ControllerBase
{
	private readonly ILogger<WithdrawController> _logger;

	public WithdrawController(ILogger<WithdrawController> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[HttpPost("callback", Name = "WithdrawCallback")]
	public async Task<string> WithdrawCallbackAsync(
		[FromServices] TuPayHasher hasher,
		[FromServices] IGetService<GetWithdrawFormQuery, WithdrawForm?> getFormService,
		[FromServices] IGetService<GetWithdrawStateMachineQuery, WithdrawStateMachine> getStateMachineService,
		[FromServices] IPaymentServiceClient paymentServiceClient,
		[FromServices] MalakiPaySerializer serializer,
		[FromServices] IPaymentClient paymentClient,
		[FromForm] IFormCollection formData)
	{
		static DateTime? ConvertToSystemDate(string date)
		{
			if (string.IsNullOrWhiteSpace(date))
				return null;

			var unspecifyDate = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", null);
			var specifyDate = DateTime.SpecifyKind(
				new DateTimeOffset(unspecifyDate, TimeSpan.FromHours(7)).UtcDateTime,
				DateTimeKind.Utc);

			return specifyDate;
		}

		var paymentReturn = new MalakiPayWithdrawReturn(
			formData["transferno"],
			Guid.Parse(formData["outtransferno"]),
			decimal.Parse(formData["tradeamount"]),
			decimal.Parse(formData["transferamount"]),
			ConvertToSystemDate(formData["endtime"]),
			formData["remark"],
			formData["status"]);

		_logger.LogInformation("PaymentReturn: {paymentReturn}", paymentReturn);

		if (paymentReturn.Status is not ("1" or "4"))
			return "success";

		var withdrawForm = await getFormService
			.GetAsync(new GetWithdrawFormQuery(paymentReturn.FormId))
			.ConfigureAwait(false);

		if (withdrawForm == null)
			return "fail";

		var hash = hasher.ComputeHash(
			withdrawForm.ApplyInfo!.Settings.Secret,
			formData
				.Where(kv => kv.Key != "sign")
				.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value)));

		if (hash != formData["sign"].ToString())
			return "fail";

		var fsm = await getStateMachineService
			.GetAsync(new GetWithdrawStateMachineQuery(withdrawForm.Status))
			.ConfigureAwait(false);

		switch (paymentReturn.Status)
		{
			case "1":
				if (fsm.CanFire(PaymentTrigger.PaymentWithdrawCompleted))
				{
					await fsm.FireAsync(PaymentTrigger.PaymentWithdrawCompleted).ConfigureAwait(false);

					await paymentServiceClient.SubmitWithdrawFormStatusAsync(
						withdrawForm.Id,
						fsm.State,
						serializer.Serialize(paymentReturn)).ConfigureAwait(false);
				}
				break;

			case "4":
				if (fsm.CanFire(PaymentTrigger.Reject))
				{
					await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

					await paymentServiceClient.SubmitWithdrawFormStatusAsync(
						withdrawForm.Id,
						paymentReturn: serializer.Serialize(paymentReturn)).ConfigureAwait(false);

					await paymentClient
						.RejectWithdrawAsync(withdrawForm.Id, withdrawForm.BrandId)
						.ConfigureAwait(false);
				}
				break;
		}

		return "success";
	}
}
