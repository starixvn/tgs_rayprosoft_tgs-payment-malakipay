using Tgs.Payment.Clients;

namespace Tgs.Payment.MalakiPay;

public record WithdrawForm(
	Guid Id,
	string? UserId,
	UserType UserType,
	Guid BrandId,
	Guid? BrandPaymentId,
	decimal Amount,
	PaymentFlowState Status,
	Guid BankInfoId,
	MalakiPayWithdrawApplyInfo? ApplyInfo,
	string? PaymentReturn = null);
