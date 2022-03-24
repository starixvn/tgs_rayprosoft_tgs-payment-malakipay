namespace Tgs.Payment.TuPay.MessageQueue;

public static class ProcessorTypes
{
	public static readonly Type DepositProcessorType = typeof(DepositProcessor);
	public static readonly Type WithdrawProcessorType = typeof(WithdrawProcessor);
}
