using Google.Protobuf;
using Microsoft.Extensions.Options;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;

namespace Tgs.Payment.MalakiPay.MessageQueue;

internal class DepositProcessor : IMessageProcessor
{
	private readonly MalakiPaySerializer _tuPaySerializer;
	private readonly TuPayClient _tuPayClient;
	private readonly TuPayOptions _options;

	public DepositProcessor(MalakiPaySerializer tuPaySerializer, IOptions<TuPayOptions> tuPayOptionsAccessor, TuPayClient tuPayClient)
	{
		_tuPaySerializer = tuPaySerializer ?? throw new ArgumentNullException(nameof(tuPaySerializer));
		_tuPayClient = tuPayClient ?? throw new ArgumentNullException(nameof(tuPayClient));
		_options = tuPayOptionsAccessor.Value;
	}

	public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = DepositRequest.Parser.ParseFrom(data.Span);

		var settings = _tuPaySerializer.Deserialize<MalakiPayDepositSettings>(request.ApplyInfo)!;

		var order = new TuPayClientDepositOrder(
			decimal.Parse(request.Amount),
			settings.PayType,
			settings.FormId,
			new Uri($"{_options.CallbackBaseUrl}/deposit/callback"));

		var depositResponse = await _tuPayClient.DepositAsync(
			settings.Merchant,
			settings.Secret,
			order).ConfigureAwait(false);

		var createdTime = DateTimeOffset.FromUnixTimeSeconds(depositResponse.CreatedTimestamp).LocalDateTime;

		var applyInfo = new MalakiPayDepositApplyInfo(
			settings,
			new MalakiPayDepositResponse(
				depositResponse.TradeNo,
				depositResponse.Amount,
				depositResponse.DiscountAmount,
				depositResponse.PayType,
				depositResponse.CardName,
				depositResponse.CardNo,
				depositResponse.BankName,
				depositResponse.SubBankName,
				depositResponse.PayerName,
				depositResponse.BankQrCode,
				createdTime,
				createdTime.AddSeconds(depositResponse.Timeout),
				depositResponse.Redirect));

		var msg = new DepositResponse
		{
			Result = _tuPaySerializer.Serialize(applyInfo)
		};

		return msg.ToByteArray();
	}
}
