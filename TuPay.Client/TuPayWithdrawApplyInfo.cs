namespace Tgs.Payment.TuPay;

public record TuPayWithdrawApplyInfo(
	TuPayWithdrawSettings Settings,
	TuPayWithdrawResponse Response);
