using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tgs.Payment.MalakiPay;

namespace MalakiPay.Core.UnitTests;

[TestClass]
public class LogicTests
{
	[TestMethod]
	public void Status_Logic_Test()
	{
		var status = "2";

		var actual = status is not ("1" or "2" or "4");

		Assert.IsFalse(actual);
	}

	[TestMethod]
	public void TupayClientDepositResponse_Deserialize()
	{
		var json = "{\"code\":6,\"results\":\"no online device..\"}";

		var serializeOptions = new JsonSerializerOptions()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
		};

		_ = JsonSerializer.Deserialize<TuPayJsonResponse<JsonDocument>>(json, serializeOptions);
	}
}
