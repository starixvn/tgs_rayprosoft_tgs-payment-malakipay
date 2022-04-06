namespace Tgs.Payment.MalakiPay;

public class ApiFailException : Exception
{
	public ApiFailException()
	{
	}

	public ApiFailException(string? message) : base(message)
	{
	}
}
