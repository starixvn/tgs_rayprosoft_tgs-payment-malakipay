using System.Text.Json.Serialization;

namespace Tgs.Payment.TuPay;

internal class TuPayJsonResponse<TResult>
{
	[JsonPropertyName("code")]
	public int Code { get; init; }

	[JsonPropertyName("results")]
	public TResult Results { get; init; } = default!;
}
