using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.MalakiPay;

public class TuPayHasher
{
	private readonly ILogger<TuPayHasher> _logger;

	public TuPayHasher(ILogger<TuPayHasher> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public string ComputeHash(string secret, IEnumerable<KeyValuePair<string, string>> dict)
	{
		var content = string.Join(
			"&",
			dict.OrderBy(kvp => kvp.Key)
				.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value?.ToString() ?? string.Empty).Replace("%20", "+").ToLower()}"));

		var fullText = $"{content}&secret={secret}";

		_logger.LogTrace("FullText: {fullText}", fullText);

		var hash = new BigInteger(
			MD5.HashData(Encoding.UTF8.GetBytes(fullText)),
			isBigEndian: true).ToString("x32");

		_logger.LogTrace("Hash: {hash}", hash);

		return hash;
	}
}
