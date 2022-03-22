namespace Tgs.Payment.TuPay;

public record TuPayDepositResponse(
	string TradeNo,
	decimal Amount,
	decimal DiscountAmount,
	string PayType,
	string CardName,
	string CardNo,
	string BankName,
	string SubBankName,
	string PayerName,
	Uri BankQrCode,
	DateTime CreatedTime,
	DateTime ExpireTime,
	Uri RedirectUrl);
