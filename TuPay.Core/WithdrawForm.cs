using Tgs.Payment.Clients;

namespace Tgs.Payment.TuPay;

public record WithdrawForm(
	Guid Id,
	string? UserId,
	UserType UserType,
	Guid BrandId,
	Guid? BrandPaymentId,
	decimal Amount,
	PaymentFlowState Status,
	Guid BankInfoId,
	TuPayWithdrawApplyInfo? ApplyInfo,
	string? PaymentReturn = null);
