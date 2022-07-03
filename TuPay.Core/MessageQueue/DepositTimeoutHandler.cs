using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;

namespace Tgs.Payment.MalakiPay.MessageQueue;

internal class DepositTimeoutHandler : IMessageHandler
{
	private readonly IPaymentServiceClient _paymentServiceClient;
	private readonly IPaymentClient _paymentClient;
	private readonly TuPayClient _tuPayClient;
	private readonly MalakiPaySerializer _malakiPaySerializer;
	private readonly ILogger<DepositTimeoutHandler> _logger;

	public DepositTimeoutHandler(
		IPaymentServiceClient paymentServiceClient,
		IPaymentClient paymentClient,
		TuPayClient tuPayClient,
		MalakiPaySerializer malakiPaySerializer,
		ILogger<DepositTimeoutHandler> logger)
	{
		_paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		_tuPayClient = tuPayClient ?? throw new ArgumentNullException(nameof(tuPayClient));
		_malakiPaySerializer = malakiPaySerializer ?? throw new ArgumentNullException(nameof(malakiPaySerializer));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = ScheduleRequest.Parser.ParseFrom(data.Span);
		_logger.LogInformation(
			"Deposit Schedule Handler Receive request Form Id:{request.FormId.ToGuid()}, "
				+ "Schedule Time:{request.ScheduleTime.ToDateTime():yyyy/MM/dd HH:mm:ss}",
			request.FormId.ToGuid(),
			request.ScheduleTime.ToDateTime());

		var formId = request.FormId.ToGuid();

		var depositForm = await _paymentServiceClient
			.GetDepositFormAsync(formId, cancellationToken)
			.ConfigureAwait(false);

		var brandId = depositForm!.BrandId;

		_logger.LogInformation(
			"GetDepositFormAsync Form Id:{depositForm.Id}, Brand Id:{brandId}, Status:{depositForm.Status}",
			depositForm.Id,
			brandId,
			depositForm.Status);

		if (depositForm.Status is PaymentFlowState.Approved
			or PaymentFlowState.Reject)
			return;

		var applyInfo = _malakiPaySerializer.Deserialize<MalakiPayDepositApplyInfo>(depositForm.ApplyInfo!)!;

		var result = await _tuPayClient.QueryDepositFormAsync(
			applyInfo.Settings.Merchant,
			applyInfo.Settings.Secret,
			formId).ConfigureAwait(false);

		if (result?.Status is "2" or "4")
			await _paymentClient
				.RejectDepositAsync(formId, brandId, cancellationToken)
				.ConfigureAwait(false);
		else if (result?.Status is not "1")
			await _paymentClient.ScheduleAsync(
				"tgs.payment.malakipay.deposit.schedule.reject",
				formId,
				applyInfo.Response.ExpireTime.AddMinutes(10D).ToUniversalTime(),
				cancellationToken).ConfigureAwait(false);
	}
}
