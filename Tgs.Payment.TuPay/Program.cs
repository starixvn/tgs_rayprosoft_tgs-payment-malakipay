using Tgs.Payment.TuPay;
using Tgs.Payment.TuPay.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
