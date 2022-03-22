using System.Text.Json.Serialization;

namespace Tgs.Payment.TuPay;

internal class TuPayClientDepositResponse
{
	[JsonPropertyName("tradeno")]
	public string TradeNo { get; init; } = default!;

	[JsonPropertyName("amount")]
	public decimal Amount { get; init; }

	[JsonPropertyName("discountamount")]
	public decimal DiscountAmount { get; init; }

	[JsonPropertyName("paytype")]
	public string PayType { get; init; } = default!;

	[JsonPropertyName("cardname")]
	public string CardName { get; init; } = default!;

	[JsonPropertyName("cardno")]
	public string CardNo { get; init; } = default!;

	[JsonPropertyName("bankname")]
	public string BankName { get; init; } = default!;

	[JsonPropertyName("subbankname")]
	public string SubBankName { get; init; } = default!;

	[JsonPropertyName("payername")]
	public string PayerName { get; init; } = default!;

	[JsonPropertyName("bankqrcode")]
	public Uri BankQrCode { get; init; } = default!;

	[JsonPropertyName("createtime")]
	public int CreatedTimestamp { get; init; } = default!;

	[JsonPropertyName("timeout")]
	public int Timeout { get; init; } = default!;

	[JsonPropertyName("redirect")]
	public Uri Redirect { get; init; } = default!;
}
