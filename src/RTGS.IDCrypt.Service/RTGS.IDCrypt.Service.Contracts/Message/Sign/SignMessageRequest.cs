using System.Text.Json;
using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.Message.Sign;

/// <summary>
///  Model of the sign message request.
/// </summary>
public record SignMessageRequest
{
	/// <summary>
	/// The RTGS Global identifier.
	/// </summary>
	[JsonPropertyName("rtgsGlobalId")]
	public string RtgsGlobalId { get; init; }

	/// <summary>
	/// The JSON document to be signed.
	/// </summary>
	[JsonPropertyName("message")]
	public JsonElement Message { get; init; }
}
