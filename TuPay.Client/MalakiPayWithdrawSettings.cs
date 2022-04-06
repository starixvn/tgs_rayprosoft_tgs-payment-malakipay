namespace Tgs.Payment.MalakiPay;

public record MalakiPayWithdrawSettings(
	string Merchant,
	string Secret,
	string BankName,
	string? SubBankName,
	string CardNo,
	string CardName,
	Guid FormId,
	string? Remark);
