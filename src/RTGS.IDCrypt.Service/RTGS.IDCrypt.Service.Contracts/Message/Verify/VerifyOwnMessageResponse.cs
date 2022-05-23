using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.Message.Verify;

/// <summary>
/// Model of the response from /message/verify/own.
/// </summary>
public record VerifyOwnMessageResponse
{
	/// <summary>
	/// True if the signature is verified, otherwise false.
	/// </summary>
	[JsonPropertyName("verified")]
	public bool Verified { get; init; }
}
