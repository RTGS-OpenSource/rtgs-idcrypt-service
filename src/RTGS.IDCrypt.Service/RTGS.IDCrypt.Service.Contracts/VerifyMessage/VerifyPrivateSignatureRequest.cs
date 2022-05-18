using System.Text.Json;
using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.VerifyMessage;

/// <summary>
/// Represents the data required to verify a signature.
/// </summary>
public record VerifyPrivateSignatureRequest
{
	/// <summary>
	/// The RTGS.global Id of the connection with which to verify the document.
	/// </summary>
	[JsonPropertyName("rtgsGlobalId")]
	public string RtgsGlobalId { get; init; }

	/// <summary>
	/// The message to verify.
	/// </summary>
	[JsonPropertyName("message")]
	public JsonElement Message { get; init; }

	/// <summary>
	/// The private signature of the signed message.
	/// </summary>
	[JsonPropertyName("privateSignature")]
	public string PrivateSignature { get; init; }

	/// <summary>
	/// The alias of the connection with which to verify the message.
	/// </summary>
	[JsonPropertyName("alias")]
	public string Alias { get; init; }
}
