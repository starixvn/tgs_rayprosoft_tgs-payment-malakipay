using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Payment.MalakiPay;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
	_ = services
		.AddMessageQueue()
		.AddNatsMessageQueue(natsConfig => natsConfig
			.ConfigQueueOptions((natsOpt, _) => natsOpt.Url = "nats://localhost:44222"))
		.AddNatsGlobPatternExchange("*");

	_ = services
		.AddPaymentClient()
		.AddSingleton<MalakiPaySerializer>();
});

var app = builder.Build();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();
var tupaySerializer = app.Services.GetRequiredService<MalakiPaySerializer>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("start");

var merchant = "71c9ce0c286c4196ac7b592add8676c9";
var secret = "76b299b8c330481bbf4dd6cc8ffdd0f8";

var depositSettings = new MalakiPayDepositSettings(
	merchant,
	secret,
	PayTypes.GCash,
	Guid.NewGuid());

var paymentReturn = await paymentClient.DepositAsync(
	"tgs.payment.malakipay.deposit",
	100M,
	tupaySerializer.Serialize(depositSettings)).ConfigureAwait(false);

var paymentReturnJson = tupaySerializer.Serialize(paymentReturn);
logger.LogInformation("{paymentReturnJson}", paymentReturnJson);

var withdrawSettings = new MalakiPayWithdrawSettings(
	merchant,
	secret,
	"BANGKOK BANK PUBLIC COMPANY LTD.",
	null,
	"2222222222",
	"Tester",
	Guid.NewGuid(),
	null);

var withdrawReturn = await paymentClient.WithdrawAsync(
	"tgs.payment.malakipay.withdraw",
	250M,
	tupaySerializer.Serialize(withdrawSettings)).ConfigureAwait(false);

var withdrawReturnJson = tupaySerializer.Serialize(withdrawReturn);

logger.LogInformation("{withdrawReturnJson}", withdrawReturnJson);
