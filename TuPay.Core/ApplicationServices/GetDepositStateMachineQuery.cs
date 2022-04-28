using Tgs.Payment.Clients;

namespace Tgs.Payment.MalakiPay.ApplicationServices;
public record GetDepositStateMachineQuery(PaymentFlowState InitialState);
