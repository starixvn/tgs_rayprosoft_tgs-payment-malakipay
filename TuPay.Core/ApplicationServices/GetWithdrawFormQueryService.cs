using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.TuPay.ApplicationServices;

internal class GetWithdrawFormQueryService : IGetService<GetWithdrawFormQuery, WithdrawForm?>
{
	private readonly IPaymentServiceClient _paymentServiceClient;
	private readonly TuPaySerializer _tuPaySerializer;

	public GetWithdrawFormQueryService(IPaymentServiceClient paymentServiceClient, TuPaySerializer tuPaySerializer)
	{
		_paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
		_tuPaySerializer = tuPaySerializer ?? throw new ArgumentNullException(nameof(tuPaySerializer));
	}

	public async ValueTask<WithdrawForm?> GetAsync(GetWithdrawFormQuery query, CancellationToken cancellationToken = default)
	{
		var data = await _paymentServiceClient
			.GetWithdrawFormAsync(query.FormId)
			.ConfigureAwait(false);

		return data == null
			? null
			: new WithdrawForm(
				data.Id,
				data.UserId,
				data.UserType,
				data.BrandId,
				data.BrandPaymentId,
				data.Amount,
				data.Status,
				data.BankInfoId,
				string.IsNullOrEmpty(data.ApplyInfo)
					? null
					: _tuPaySerializer.Deserialize<TuPayWithdrawApplyInfo>(data.ApplyInfo),
				data.PaymentReturn);
	}
}
