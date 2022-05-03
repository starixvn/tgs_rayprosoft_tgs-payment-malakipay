using System.Text.Json.Serialization;

namespace Tgs.Payment.MalakiPay;

internal class TuPayClientDepositInfo
{
	[JsonPropertyName("tradeno")]
	public string TradeNo { get; set; } = default!;

	[JsonPropertyName("outtradeno")]
	public string OutTradeNo { get; set; } = default!;

	[JsonPropertyName("amount")]
	public string Amount { get; set; } = default!;

	[JsonPropertyName("ramount")]
	public string ReceivedAmount { get; set; } = default!;

	[JsonPropertyName("endtime")]
	public string PaidTime { get; set; } = default!;

	[JsonPropertyName("status")]
	public string Status { get; set; } = default!;

	[JsonPropertyName("remark")]
	public string? Remark { get; set; }

	[JsonPropertyName("sign")]
	public string Sign { get; init; } = default!;
}
