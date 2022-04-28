using Tgs.Payment.Clients;

namespace Tgs.Payment.MalakiPay.ApplicationServices;

public record GetWithdrawStateMachineQuery(PaymentFlowState InitialState);
