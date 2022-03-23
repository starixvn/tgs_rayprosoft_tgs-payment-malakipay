namespace Tgs.Payment.TuPay;

public record TuPayDepositReturn(
	string TradeNo,
	Guid OutTradeNo,
	decimal Amount,
	decimal ReceivedAmount,
	decimal? OriginAmount,
	DateTime PayTime,
	string Status,
	string? Remark);
