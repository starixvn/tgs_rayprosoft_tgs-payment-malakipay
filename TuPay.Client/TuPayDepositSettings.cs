namespace Tgs.Payment.TuPay;

public record TuPayDepositSettings(
	string Merchant,
	string Secret,
	string PayType,
	Guid FormId);
