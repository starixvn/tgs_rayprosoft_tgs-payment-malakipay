namespace Tgs.Payment.TuPay;

public record TuPayWithdrawReturn(
	string TransferNo,
	Guid FormId,
	decimal TradeAmount,
	decimal TransferAmount,
	DateTime? PayTime,
	string? Remark,
	string Status);
