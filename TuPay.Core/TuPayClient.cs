using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.TuPay;

internal class TuPayClient
{
	private readonly HttpClient _httpClient;
	private readonly TuPayHasher _tuPayHasher;
	private readonly ILogger<TuPayClient> _logger;

	private readonly JsonSerializerOptions _jsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
	};

	public TuPayClient(HttpClient httpClient, TuPayHasher tuPayHasher, ILogger<TuPayClient> logger)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		_tuPayHasher = tuPayHasher ?? throw new ArgumentNullException(nameof(tuPayHasher));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask<TuPayClientDepositResponse> DepositAsync(string merchant, string secret, TuPayClientDepositOrder order)
	{
		var requestFormData = new Dictionary<string, string>
		{
			["amount"] = order.Amount.ToString(),
			["merchant"] = merchant,
			["paytype"] = order.PayType,
			["outtradeno"] = order.OutTradeNo.ToString(),
			["notifyurl"] = order.NotifyUrl.ToString(),
			["returndataformat"] = "serverjson"
		};

		var sign = _tuPayHasher.ComputeHash(secret, requestFormData);
		requestFormData["sign"] = sign;

		var requestContent = new FormUrlEncodedContent(requestFormData);

		var response = await _httpClient
			.PostAsync("/pay", requestContent)
			.ConfigureAwait(false);

		_logger.LogInformation("Deposit StatusCode: {response.StatusCode}", response.StatusCode);

		var bodyJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		_logger.LogInformation("Deposit Body: {bodyJson}", bodyJson);

		_ = response.EnsureSuccessStatusCode();

		var responseData = JsonSerializer.Deserialize<TuPayJsonResponse<TuPayClientDepositResponse>>(bodyJson, _jsonSerializerOptions);

		return responseData!.Code != 0
			? throw new ApiFailException()
			: responseData.Results;
	}
}
