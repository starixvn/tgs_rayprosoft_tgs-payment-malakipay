namespace Tgs.Payment.TuPay;

internal record TuPayClientDepositOrder(
	decimal Amount,
	string PayType,
	Guid OutTradeNo,
	Uri NotifyUrl);
