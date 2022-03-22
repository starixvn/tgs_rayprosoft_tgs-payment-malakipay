using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tgs.Payment.TuPay;

namespace TuPay.Core.UnitTests;

[TestClass]
public class HashTests
{
	[TestMethod]
	public void HashTest()
	{
		var dict = new Dictionary<string, string>
		{
			["amount"] = "123.45",
			["merchant"] = "2e2e51902ca34026915b59be8623efec",
			["paytype"] = "promptpay",
			["outtradeno"] = "3efb21af-ebb0-4737-b9e8-5d3a0ea9950e",
		};

		var sut = new TuPayHasher(NullLogger<TuPayHasher>.Instance);

		var expected = "ed93880e8698b0af971a4d6e1b045e1f";

		var actual = sut.ComputeHash("28ac492de45f44ccb7b7d700e5a82928", dict);

		Assert.AreEqual(32, actual.Length);
		Assert.AreEqual(expected, actual);
	}
}
