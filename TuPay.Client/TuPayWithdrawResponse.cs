namespace Tgs.Payment.TuPay;

public record TuPayWithdrawResponse(
	string TransferNo,
	decimal TradeAmount,
	decimal TransferAmount,
	DateTime? PayTime,
	string? Remark,
	int Status);
