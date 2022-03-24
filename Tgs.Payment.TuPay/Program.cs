using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Payment.TuPay;
using Tgs.Payment.TuPay.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
			fluentOptions.UseMessagePack();
		});
	});

builder.Services
	.AddControllers();

builder.Services
	.AddTuPay((options, _) => builder.Configuration
		.GetSection("TuPay")
		.Bind(options))
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigQueueOptions((options, _) => options.Url = builder.Configuration.GetConnectionString("Nats"))
		.AddProcessorByType(
			ProcessorTypes.DepositProcessorType,
			"tgs.payment.tupay.deposit",
			"tgs.payment.tupay.deposit")
		.AddProcessorByType(
			ProcessorTypes.WithdrawProcessorType,
			"tgs.payment.tupay.withdraw",
			"tgs.payment.tupay.withdraw"))
	.AddNatsGlobPatternExchange("*");

builder.Services
	.AddHttpLogging(logging =>
	{
		logging.LoggingFields = HttpLoggingFields.All;
		logging.MediaTypeOptions.AddText("application/x-www-form-urlencoded");
		logging.MediaTypeOptions.AddText("text/html");
	});

builder.Services
	.AddOptions<OtlpExporterOptions>()
	.Configure<IConfiguration>((options, conifg) =>
	{
		var nodeIp = conifg.GetValue<string>("NODE_IP");

		if (!string.IsNullOrWhiteSpace(nodeIp))
			options.Endpoint = new Uri($"http://{nodeIp}:4317");

		conifg.GetSection("Opentelemetry:Tracing").Bind(options);
	})
	.Services
	.AddOpenTelemetryTracing(builder => builder
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
		.AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddProfileViewExporter()
		.AddOtlpExporter())
	.Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
		httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
			&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
			&& !new[] { "html", "ico", "png", "jpg" }.Any(
				ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
	.AddProfileViewer();

var app = builder.Build();

app.UseHttpLogging();

app.MapControllers();
app.MapProfileViewer();
app.MapGet("/", () => "Hello World!");

app.Use(async (context, next) =>
{
	await next().ConfigureAwait(false);

	using var stream = new StreamReader(context.Request.Body);

	_ = await stream.ReadToEndAsync().ConfigureAwait(false);
});

app.Run();
