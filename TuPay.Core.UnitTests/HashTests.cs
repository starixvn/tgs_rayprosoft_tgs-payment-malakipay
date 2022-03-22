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

	[TestMethod]
	public void Callback_Data_HashTest()
	{
		var dict = new Dictionary<string, string>
		{
			["amount"] = "100",
			["endtime"] = "2022-03-22 10:03:09",
			["outtradeno"] = "ab359aa9-1542-451d-8db4-8261eb9b85c7",
			["ramount"] = "99.53",
			["remark"] = string.Empty,
			["status"] = "1",
			["tradeno"] = "8a89ddcaf6f542fd9ac15b9011c844dd"
		};

		var sut = new TuPayHasher(NullLogger<TuPayHasher>.Instance);

		var expected = "477cd77b07c18050322228e06693b649";

		var actual = sut.ComputeHash("28ac492de45f44ccb7b7d700e5a82928", dict);

		Assert.AreEqual(expected, actual);
	}
}
