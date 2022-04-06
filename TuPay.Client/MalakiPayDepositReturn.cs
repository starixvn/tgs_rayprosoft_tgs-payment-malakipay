namespace Tgs.Payment.MalakiPay;

public record MalakiPayDepositReturn(
	string TradeNo,
	Guid OutTradeNo,
	decimal Amount,
	decimal ReceivedAmount,
	decimal? OriginAmount,
	DateTime PayTime,
	string Status,
	string? Remark);
