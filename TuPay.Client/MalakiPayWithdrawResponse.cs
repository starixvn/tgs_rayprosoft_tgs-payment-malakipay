namespace Tgs.Payment.MalakiPay;

public record MalakiPayWithdrawResponse(
	string TransferNo,
	decimal TradeAmount,
	decimal TransferAmount,
	DateTime? PayTime,
	string? Remark,
	int Status);
