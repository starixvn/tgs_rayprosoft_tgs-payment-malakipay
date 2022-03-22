using Microsoft.AspNetCore.HttpLogging;
using Tgs.Payment.TuPay;
using Tgs.Payment.TuPay.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

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
			"tgs.payment.tupay.deposit"));

builder.Services
	.AddHttpLogging(logging =>
	{
		logging.LoggingFields = HttpLoggingFields.All;
		logging.MediaTypeOptions.AddText("application/x-www-form-urlencoded");
		logging.MediaTypeOptions.AddText("text/html");
	});

var app = builder.Build();

app.UseHttpLogging();

app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Use(async (context, next) =>
{
	await next().ConfigureAwait(false);

	using var stream = new StreamReader(context.Request.Body);

	_ = await stream.ReadToEndAsync().ConfigureAwait(false);
});

app.Run();
