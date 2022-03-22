namespace Tgs.Payment.TuPay;

public record TuPayDepositApplyInfo(
	TuPayDepositSettings Settings,
	TuPayDepositResponse Response);
