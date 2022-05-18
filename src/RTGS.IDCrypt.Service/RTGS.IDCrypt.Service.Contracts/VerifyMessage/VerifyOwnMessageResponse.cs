using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.VerifyMessage;

/// <summary>
/// Represents the response to a <see cref="VerifyOwnMessageRequest"/>.
/// </summary>
public record VerifyOwnMessageResponse
{
	/// <summary>
	/// True if the signature is verified, otherwise false.
	/// </summary>
	[JsonPropertyName("verified")]
	public bool Verified { get; init; }
}
