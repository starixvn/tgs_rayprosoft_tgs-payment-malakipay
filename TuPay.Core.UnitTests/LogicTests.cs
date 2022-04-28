using Microsoft.VisualStudio.TestTools.UnitTesting;

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
}
