using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Tgs.Payment.TuPay;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTuPay(
		this IServiceCollection services,
		Action<TuPayOptions, IServiceProvider> configure)
		=> services
			.AddOptions<TuPayOptions>()
			.Configure(configure)
			.Services
			.AddHttpClient<TuPayClient>()
			.ConfigureHttpClient((sp, httpClient) =>
			{
				var options = sp.GetRequiredService<IOptions<TuPayOptions>>().Value;

				httpClient.BaseAddress = options.PaymentApiUrl;
			})
			.Services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddPaymentClient()
			.AddPaymentServiceClient((options, sp) =>
			{
				var skyPayOptions = sp.GetRequiredService<IOptions<TuPayOptions>>().Value;

				options.PaymentServiceBaseUrl = skyPayOptions.PaymentServiceBaseUrl;

				options.JsonSerializerOptions = new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					Converters = {
						new JsonStringEnumConverter()
					}
				};
			})
			.AddTransient<TuPayHasher>()
			.AddSingleton<TuPaySerializer>();
}
