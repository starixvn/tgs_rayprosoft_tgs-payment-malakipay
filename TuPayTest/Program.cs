using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Payment.TuPay;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
	_ = services
		.AddMessageQueue()
		.AddNatsMessageQueue(natsConfig =>
		{
		})
		.AddNatsGlobPatternExchange("*");

	_ = services
		.AddPaymentClient()
		.AddSingleton<TuPaySerializer>();
});

var app = builder.Build();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();
var tupaySerializer = app.Services.GetRequiredService<TuPaySerializer>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("start");

var merchant = "2e2e51902ca34026915b59be8623efec";
var secret = "28ac492de45f44ccb7b7d700e5a82928";

var depositSettings = new TuPayDepositSettings(
	merchant,
	secret,
	PayTypes.TrueWallet,
	Guid.NewGuid());

var paymentReturn = await paymentClient.DepositAsync(
	"tgs.payment.tupay.deposit",
	100M,
	tupaySerializer.Serialize(depositSettings)).ConfigureAwait(false);

var paymentReturnJson = tupaySerializer.Serialize(paymentReturn);
logger.LogInformation("{paymentReturnJson}", paymentReturnJson);
