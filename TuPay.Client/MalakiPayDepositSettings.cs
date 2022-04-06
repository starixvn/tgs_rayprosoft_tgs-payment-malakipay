namespace Tgs.Payment.MalakiPay;

public record MalakiPayDepositSettings(
	string Merchant,
	string Secret,
	string PayType,
	Guid FormId);
