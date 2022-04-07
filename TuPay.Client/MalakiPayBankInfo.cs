using System.Text.Json.Serialization;

namespace Tgs.Payment.MalakiPay;

public class MalakiPayBankInfo
{
	[JsonPropertyName("bankName")]
	public string BankName { get; init; } = default!;

	[JsonPropertyName("bankBranch")]
	public string? SubBankName { get; init; }

	[JsonPropertyName("phone")]
	public string CardNo { get; init; } = default!;

	[JsonPropertyName("username")]
	public string CardName { get; init; } = default!;
}
