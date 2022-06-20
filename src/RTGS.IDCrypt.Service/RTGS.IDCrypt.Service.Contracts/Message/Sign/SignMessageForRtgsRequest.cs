using System.Text.Json;
using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.Message.Sign;

/// <summary>
///  Model of the sign message request.
/// </summary>
public record SignMessageForRtgsRequest
{
	/// <summary>
	/// The JSON document to be signed.
	/// </summary>
	[JsonPropertyName("message")]
	public JsonElement Message { get; init; }
}
