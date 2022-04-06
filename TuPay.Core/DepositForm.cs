using Tgs.Payment.Clients;

namespace Tgs.Payment.MalakiPay;

public record DepositForm(
	Guid Id,
	string PlayerCode,
	Guid BrandId,
	Guid BrandPaymentId,
	decimal Amount,
	PaymentFlowState Status,
	MalakiPayDepositApplyInfo? ApplyInfo,
	string? PaymentReturn = null);
