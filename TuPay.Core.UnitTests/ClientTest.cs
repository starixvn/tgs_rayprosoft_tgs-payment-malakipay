using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tgs.Payment.TuPay;

namespace TuPay.Core.UnitTests;

[TestClass]
public class ClientTest
{
	[TestMethod]
	public async Task DepositTest()
	{
		var httpClient = new HttpClient
		{
			BaseAddress = new Uri("https://www.tupay168.com")
		};

		var sut = new TuPayClient(
			httpClient,
			new TuPayHasher(NullLogger<TuPayHasher>.Instance),
			NullLogger<TuPayClient>.Instance);

		var merchant = "2e2e51902ca34026915b59be8623efec";
		var secret = "28ac492de45f44ccb7b7d700e5a82928";

		var request = new TuPayClientDepositOrder(
			123.45M,
			"truewallet",
			Guid.NewGuid(),
			new Uri("https://tgs.com/tupay/deposit/callback"));

		var actual = await sut.DepositAsync(merchant, secret, request);
	}
}
