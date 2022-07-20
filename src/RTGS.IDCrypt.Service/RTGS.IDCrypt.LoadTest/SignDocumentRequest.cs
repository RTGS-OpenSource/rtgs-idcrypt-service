using System.Text.Json;
using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.LoadTest;
public record SignDocumentRequest
{
	[JsonPropertyName("connection_id")]
	public string ConnectionId { get; init; }

	[JsonPropertyName("document")]
	public JsonElement Document { get; init; }
}
