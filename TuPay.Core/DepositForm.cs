using Tgs.Payment.Clients;

namespace Tgs.Payment.TuPay;

public record DepositForm(
	Guid Id,
	string PlayerCode,
	Guid BrandId,
	Guid BrandPaymentId,
	decimal Amount,
	PaymentFlowState Status,
	TuPayDepositApplyInfo? ApplyInfo,
	string? PaymentReturn = null);
