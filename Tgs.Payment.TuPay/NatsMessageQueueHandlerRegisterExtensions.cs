using Tgs.MessageQueue.Nats.Configuration;

namespace Tgs.Payment.MalakiPay;

internal static class NatsMessageQueueHandlerRegisterExtensions
{
	public static NatsMessageQueueConfiguration AddProcessorByType(
		this NatsMessageQueueConfiguration configuration,
		Type processorType,
		string subject,
		string group)
	{
		var registerMethod = typeof(NatsMessageQueueConfiguration)
			.GetMethod(
				nameof(NatsMessageQueueConfiguration.AddProcessor),
				new[] { typeof(string), typeof(string) })!
			.MakeGenericMethod(processorType);

		_ = registerMethod.Invoke(configuration, new object[] { subject, group });

		return configuration;
	}
}
