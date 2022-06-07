using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Webhooks.Models;

public record Proof
{
	[JsonPropertyName("connection_id")]
	public string ConnectionId { get; init; }

	[JsonPropertyName("state")]
	public string State { get; init; }
}
