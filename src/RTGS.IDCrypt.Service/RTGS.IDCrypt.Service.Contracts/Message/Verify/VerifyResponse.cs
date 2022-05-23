using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.Message.Verify;

/// <summary>
/// Model of the response from /message/verify.
/// </summary>
public record VerifyResponse
{
	/// <summary>
	/// True if the signature is verified, otherwise false.
	/// </summary>
	[JsonPropertyName("verified")]
	public bool Verified { get; init; }
}
