using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.TuPay.ViewModels;

namespace Tgs.Payment.TuPay.Controllers;

[Route("[controller]")]
[ApiController]
public class DepositController : ControllerBase
{
	[Consumes("application/x-www-form-urlencoded")]
	[Produces(MediaTypeNames.Text.Plain)]
	[HttpPost("callback", Name = "DepositCallback")]
	public string DepositCallback(
		[FromServices] TuPayHasher hasher,
		[FromForm] IFormCollection formData)
	{
		var hash = hasher.ComputeHash(
			"28ac492de45f44ccb7b7d700e5a82928",
			formData
				.Where(kv => kv.Key != "sign")
				.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value)));

		if (hash != formData["sign"].ToString())
			return "fail";

		return "success";
	}
}
