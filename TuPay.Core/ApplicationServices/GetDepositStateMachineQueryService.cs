using Stateless;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.MalakiPay.ApplicationServices;

internal class GetDepositStateMachineQueryService : IGetService<GetDepositStateMachineQuery, StateMachine<PaymentFlowState, PaymentTrigger>>
{
	private readonly IPaymentServiceClient _paymentServiceClient;

	public GetDepositStateMachineQueryService(IPaymentServiceClient paymentServiceClient)
	{
		_paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
	}

	public ValueTask<StateMachine<PaymentFlowState, PaymentTrigger>> GetAsync(
		GetDepositStateMachineQuery query,
		CancellationToken cancellationToken = default)
		=> ValueTask.FromResult(_paymentServiceClient.CreateDepositStateMachine(query.InitialState));
}
