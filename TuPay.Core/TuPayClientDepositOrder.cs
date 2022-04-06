namespace Tgs.Payment.MalakiPay;

internal record TuPayClientDepositOrder(
	decimal Amount,
	string PayType,
	Guid OutTradeNo,
	Uri NotifyUrl);
