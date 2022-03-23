using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.TuPay.ApplicationServices;

internal class GetDepositFormQueryService : IGetService<GetDepositFormQuery, DepositForm?>
{
	private readonly IPaymentServiceClient _paymentServiceClient;
	private readonly TuPaySerializer _tuPaySerializer;

	public GetDepositFormQueryService(IPaymentServiceClient paymentServiceClient, TuPaySerializer tuPaySerializer)
	{
		_paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
		_tuPaySerializer = tuPaySerializer ?? throw new ArgumentNullException(nameof(tuPaySerializer));
	}

	public async ValueTask<DepositForm?> GetAsync(GetDepositFormQuery query, CancellationToken cancellationToken = default)
	{
		var data = await _paymentServiceClient
			.GetDepositFormAsync(query.FormId)
			.ConfigureAwait(false);

		return data is null
			? null
			: new DepositForm(
				data.Id,
				data.PlayerCode,
				data.BrandId,
				data.BrandPaymentId,
				data.Amount,
				data.Status,
				string.IsNullOrEmpty(data.ApplyInfo)
					? null
					: _tuPaySerializer.Deserialize<TuPayDepositApplyInfo>(data.ApplyInfo),
				data.PaymentReturn);
	}
}
