namespace Tgs.Payment.TuPay;

public record TuPayWithdrawSettings(
	string Merchant,
	string Secret,
	string BankName,
	string? SubBankName,
	string CardNo,
	string CardName,
	Guid FormId,
	string? Remark);
