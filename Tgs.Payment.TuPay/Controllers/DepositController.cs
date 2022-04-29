using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.Payment.MalakiPay.ApplicationServices;
using Tgs.ServiceUtility;

using DepositStateMachine = Stateless.StateMachine<Tgs.Payment.Clients.PaymentFlowState, Tgs.Payment.Clients.PaymentTrigger>;

namespace Tgs.Payment.MalakiPay.Controllers;

[Route("[controller]")]
[ApiController]
public class DepositController : ControllerBase
{
	private readonly ILogger<DepositController> _logger;

	public DepositController(ILogger<DepositController> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[Consumes("application/x-www-form-urlencoded")]
	[Produces(MediaTypeNames.Text.Plain)]
	[HttpPost("callback", Name = "DepositCallback")]
	public async Task<string> DepositCallbackAsync(
		[FromServices] TuPayHasher hasher,
		[FromServices] IGetService<GetDepositFormQuery, DepositForm?> getFormService,
		[FromServices] IGetService<GetDepositStateMachineQuery, DepositStateMachine> getStateMachineService,
		[FromServices] IPaymentServiceClient paymentServiceClient,
		[FromServices] MalakiPaySerializer tuPaySerializer,
		[FromServices] IPaymentClient paymentClient,
		[FromForm] IFormCollection formData)
	{
		var unspecifyPayDate = DateTime.ParseExact(formData["endtime"], "yyyy-MM-dd HH:mm:ss", null);
		var specifyPayDate = DateTime.SpecifyKind(
			new DateTimeOffset(unspecifyPayDate, TimeSpan.FromHours(7)).UtcDateTime,
			DateTimeKind.Utc);

		var paymentReturn = new MalakiPayDepositReturn(
			formData["tradeno"],
			Guid.Parse(formData["outtradeno"]),
			decimal.Parse(formData["amount"]),
			decimal.Parse(formData["ramount"]),
			string.IsNullOrEmpty(formData["oamount"]) ? null : decimal.Parse(formData["oamount"]),
			specifyPayDate,
			formData["status"],
			formData["remark"]);

		_logger.LogInformation("PaymentReturn: {paymentReturn}", paymentReturn);

		if (paymentReturn.Status is not ("1" or "2" or "4"))
			return "success";

		var depositForm = await getFormService
			.GetAsync(new GetDepositFormQuery(paymentReturn.OutTradeNo))
			.ConfigureAwait(false);

		if (depositForm is null)
			return "fail";

		var hash = hasher.ComputeHash(
			depositForm.ApplyInfo!.Settings.Secret,
			formData
				.Where(kv => kv.Key != "sign")
				.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value)));

		if (hash != formData["sign"].ToString())
			return "fail";

		var fsm = await getStateMachineService
			.GetAsync(new GetDepositStateMachineQuery(depositForm.Status))
			.ConfigureAwait(false);

		switch (paymentReturn.Status)
		{
			case "1":
				if (fsm.CanFire(PaymentTrigger.Approve))
				{
					await fsm.FireAsync(PaymentTrigger.Approve).ConfigureAwait(false);

					await paymentServiceClient.SubmitDepositFormStatusAsync(
						depositForm.Id,
						paymentReturn: tuPaySerializer.Serialize(paymentReturn)).ConfigureAwait(false);

					await paymentClient.ApproveDepositAsync(
						depositForm.Id,
						depositForm.BrandId,
						paymentReturn.ReceivedAmount).ConfigureAwait(false);
				}

				break;

			case "2":
			case "4":
				if (fsm.CanFire(PaymentTrigger.Reject))
				{
					await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

					await paymentServiceClient.SubmitDepositFormStatusAsync(
						depositForm.Id,
						paymentReturn: tuPaySerializer.Serialize(paymentReturn)).ConfigureAwait(false);

					await paymentClient.RejectDepositAsync(
						depositForm.Id,
						depositForm.BrandId).ConfigureAwait(false);
				}

				break;
		}

		return "success";
	}
}
