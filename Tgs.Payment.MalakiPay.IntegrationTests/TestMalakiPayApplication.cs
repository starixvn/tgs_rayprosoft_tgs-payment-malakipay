using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;

namespace Tgs.Payment.MalakiPay.IntegrationTests;

internal class TestMalakiPayApplication : WebApplicationFactory<Program>
{
	private readonly Action<IWebHostBuilder>? _webHostConfigure;

	public TestMalakiPayApplication(Action<IWebHostBuilder>? webHostConfigure = null)
	{
		_webHostConfigure = webHostConfigure;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		_ = builder.ConfigureAppConfiguration(builder => builder.Sources.Clear());

		_ = builder.ConfigureServices(services => services
			.AddFakeNatsMessageQueue()
			.AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
				sp => new ConfigureNamedOptions<OtlpExporterOptions>(
					Options.DefaultName,
					exporterOptions =>
					{
						exporterOptions.TimeoutMilliseconds = 0;
					})));


		_webHostConfigure?.Invoke(builder);
	}
}
