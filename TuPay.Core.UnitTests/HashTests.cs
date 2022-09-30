using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tgs.Payment.MalakiPay;

namespace MalakiPay.Core.UnitTests;

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

	[TestMethod]
	public void Withdraw_Data_HashTest()
	{
		var dict = new Dictionary<string, string>
		{
			["amount"] = "123.45",
			["merchant"] = "2e2e51902ca34026915b59be8623efec",
			["bankname"] = "BANGKOK BANK PUBLIC COMPANY LTD.",
			["cardno"] = "11111111111",
			["cardname"] = "Tester",
			["notifyurl"] = "https://tgs.com/tupay/withdraw/callback",
			["outtransferno"] = "48db066a-e73c-4940-8bde-eec01e075d7e",
			["remark"] = string.Empty,
		};

		var sut = new TuPayHasher(NullLogger<TuPayHasher>.Instance);

		var expected = "ef94ee40f4733c1aa46016895c8b181a";

		var actual = sut.ComputeHash("28ac492de45f44ccb7b7d700e5a82928", dict);

		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void Withdraw_Data_Hash2Test()
	{
		var dict = new Dictionary<string, string>
		{
			["endtime"] = "2022-09-30 09:13:30",
			["outtransferno"] = "27c5ca41-c5b7-4214-b03b-6d2bfb8ed01e",
			["remark"] = "error:transfer failError: 訂單已經提交，頁面未跳轉到成功也有，請到 GCASH APP 上檢查是否已經出款(The order has been submitted, but the page does not jump to success, please go to the GCASH APP to check whether the payment has been successed)操作人:Adam",
			["status"] = "1",
			["tradeamount"] = "303",
			["transferamount"] = "300",
			["transferno"] = "087476514f90408aa2efee51d3ab86bd",
		};

		var sut = new TuPayHasher(NullLogger<TuPayHasher>.Instance);

		var expected = "19f7429c674546d6ec9a01190dd7f442";

		var actual = sut.ComputeHash("28ac492de45f44ccb7b7d700e5a82928", dict);

		Assert.AreEqual(expected, actual);
	}
}
