namespace Tgs.Payment.TuPay;

public class ApiFailException : Exception
{
	public ApiFailException()
	{
	}

	public ApiFailException(string? message) : base(message)
	{
	}
}
