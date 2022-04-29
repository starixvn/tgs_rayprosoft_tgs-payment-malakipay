using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.MalakiPay.ApplicationServices;

internal class GetWithdrawFormQueryService : IGetService<GetWithdrawFormQuery, WithdrawForm?>
{
	private readonly IPaymentServiceClient _paymentServiceClient;
	private readonly MalakiPaySerializer _tuPaySerializer;

	public GetWithdrawFormQueryService(IPaymentServiceClient paymentServiceClient, MalakiPaySerializer tuPaySerializer)
	{
		_paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
		_tuPaySerializer = tuPaySerializer ?? throw new ArgumentNullException(nameof(tuPaySerializer));
	}

	public async ValueTask<WithdrawForm?> GetAsync(GetWithdrawFormQuery query, CancellationToken cancellationToken = default)
	{
		var data = await _paymentServiceClient
			.GetWithdrawFormAsync(query.FormId, cancellationToken)
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
					: _tuPaySerializer.Deserialize<MalakiPayWithdrawApplyInfo>(data.ApplyInfo),
				data.PaymentReturn);
	}
}
