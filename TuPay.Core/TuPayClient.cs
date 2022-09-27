using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.MalakiPay;

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

		var responseData = JsonSerializer.Deserialize<TuPayJsonResponse<JsonDocument>>(bodyJson, _jsonSerializerOptions)!;

		return responseData.Code != 0
			? throw new ApiFailException(bodyJson)
			: responseData.Results.Deserialize<TuPayClientDepositResponse>(_jsonSerializerOptions)!;
	}

	public async ValueTask<TuPayClientDepositInfo?> QueryDepositFormAsync(string merchant, string secret, Guid outTradeNo)
	{
		var queryFormData = new Dictionary<string, string>
		{
			["merchant"] = merchant,
			["outtradeno"] = outTradeNo.ToString()
		};

		var querySign = _tuPayHasher.ComputeHash(secret, queryFormData);
		queryFormData["sign"] = querySign;

		var queryReqContent = new FormUrlEncodedContent(queryFormData);

		var queryResponse = await _httpClient
			.PostAsync("/trade/query", queryReqContent)
			.ConfigureAwait(false);

		_logger.LogInformation("Withdraw Query StatusCode: {queryResponse.StatusCode}", queryResponse.StatusCode);

		var queryBodyJson = await queryResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

		_logger.LogInformation("Withdraw Query Body: {queryBodyJson}", queryBodyJson);

		_ = queryResponse.EnsureSuccessStatusCode();

		var queryResponseData = JsonSerializer.Deserialize<TuPayJsonResponse<TuPayClientDepositInfo?>>(queryBodyJson, _jsonSerializerOptions);

		return queryResponseData!.Code != 0 ? throw new ApiFailException(queryBodyJson) : queryResponseData.Results;
	}

	public async Task<TuPayClientWithdrawInfo> WithdrawAsync(string merchant, string secret, TuPayClientWithdrawOrder order)
	{
		var requestFormData = new Dictionary<string, string>
		{
			["amount"] = order.Amount.ToString(),
			["merchant"] = merchant,
			["bankname"] = order.BankName,
			["cardno"] = order.CardNo,
			["cardname"] = order.CardName,
			["notifyurl"] = order.NotifyUrl.ToString(),
			["outtransferno"] = order.OutTradeNo.ToString(),
			["remark"] = order.Remark ?? string.Empty
		};

		var sign = _tuPayHasher.ComputeHash(secret, requestFormData);
		requestFormData["sign"] = sign;

		var requestContent = new FormUrlEncodedContent(requestFormData);

		var response = await _httpClient
			.PostAsync("/transfer/apply", requestContent)
			.ConfigureAwait(false);

		_logger.LogInformation("Withdraw StatusCode: {response.StatusCode}", response.StatusCode);

		var bodyJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		_logger.LogInformation("Withdraw Body: {bodyJson}", bodyJson);

		_ = response.EnsureSuccessStatusCode();

		var responseData = JsonSerializer.Deserialize<TuPayJsonResponse<object?>>(bodyJson, _jsonSerializerOptions);

		if (responseData!.Code != 0)
			throw new ApiFailException();

		return new TuPayClientWithdrawInfo
		{
			OutTradeNo = order.OutTradeNo,
			TransferNo = order.OutTradeNo.ToString(),
			TradeAmount = order.Amount,
			TransferAmount = order.Amount,
			Status = 2
		};
	}
}
