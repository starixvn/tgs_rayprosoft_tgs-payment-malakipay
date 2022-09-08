using System.Reflection;
using Microsoft.Extensions.Options;
using Tgs.Payment.MalakiPay;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTuPay(
		this IServiceCollection services,
		Action<TuPayOptions, IServiceProvider> configure,
		Func<IServiceProvider, DelegatingHandler> httpClientMessageHandlerFactory)
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
			.AddHttpMessageHandler(httpClientMessageHandlerFactory)
			.Services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddPaymentClient()
			.AddPaymentServiceClient(httpClientBuilder => httpClientBuilder
				.ConfigureHttpClient((sp, http) =>
				{
					var skyPayOptions = sp.GetRequiredService<IOptions<TuPayOptions>>().Value;

					http.BaseAddress = skyPayOptions.PaymentServiceBaseUrl;
				})
				.AddHttpMessageHandler(httpClientMessageHandlerFactory))
			.AddTransient<TuPayHasher>()
			.AddSingleton<MalakiPaySerializer>();
}
