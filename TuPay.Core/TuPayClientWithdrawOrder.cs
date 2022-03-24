namespace Tgs.Payment.TuPay;

public record TuPayClientWithdrawOrder(
	decimal Amount,
	string BankName,
	string? SubBankName,
	string CardNo,
	string CardName,
	Uri NotifyUrl,
	Guid OutTradeNo,
	string? Remark);
