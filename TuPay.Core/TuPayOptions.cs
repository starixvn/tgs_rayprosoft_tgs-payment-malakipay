namespace Tgs.Payment.TuPay;

public class TuPayOptions
{
	public string CallbackBaseUrl { get; set; } = default!;

	public Uri PaymentApiUrl { get; set; } = default!;

	public Uri PaymentServiceBaseUrl { get; set; } = default!;
}
