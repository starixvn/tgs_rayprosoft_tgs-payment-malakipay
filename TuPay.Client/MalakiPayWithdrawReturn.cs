namespace Tgs.Payment.MalakiPay;

public record MalakiPayWithdrawReturn(
	string TransferNo,
	Guid FormId,
	decimal TradeAmount,
	decimal TransferAmount,
	DateTime? PayTime,
	string? Remark,
	string Status);
