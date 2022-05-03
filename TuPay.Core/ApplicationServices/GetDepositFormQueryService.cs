using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.MalakiPay.ApplicationServices;

internal class GetDepositFormQueryService : IGetService<GetDepositFormQuery, DepositForm?>
{
	private readonly IPaymentServiceClient _paymentServiceClient;
	private readonly MalakiPaySerializer _tuPaySerializer;

	public GetDepositFormQueryService(IPaymentServiceClient paymentServiceClient, MalakiPaySerializer tuPaySerializer)
	{
		_paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
		_tuPaySerializer = tuPaySerializer ?? throw new ArgumentNullException(nameof(tuPaySerializer));
	}

	public async ValueTask<DepositForm?> GetAsync(GetDepositFormQuery query, CancellationToken cancellationToken = default)
	{
		var data = await _paymentServiceClient
			.GetDepositFormAsync(query.FormId, cancellationToken)
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
					: _tuPaySerializer.Deserialize<MalakiPayDepositApplyInfo>(data.ApplyInfo),
				data.PaymentReturn);
	}
}
