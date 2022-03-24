using System.Text.Json.Serialization;

namespace Tgs.Payment.TuPay;

internal class TuPayClientWithdrawInfo
{
	[JsonPropertyName("transferno")]
	public string TransferNo { get; init; } = default!;

	[JsonPropertyName("outtransferno")]
	public Guid OutTradeNo { get; init; }

	[JsonPropertyName("tradeamount")]
	public decimal TradeAmount { get; init; }

	[JsonPropertyName("transferamount")]
	public decimal TransferAmount { get; init; }

	[JsonPropertyName("endtime")]
	public string? PayTime { get; init; }

	[JsonPropertyName("remark")]
	public string? Remark { get; init; }

	[JsonPropertyName("status")]
	public int Status { get; init; } = default!;

	[JsonPropertyName("sign")]
	public string Sign { get; init; } = default!;
}
