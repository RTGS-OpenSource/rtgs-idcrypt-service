using System.Text.Json.Serialization;

public record IdCryptConnection
{
	[JsonPropertyName("alias")]
	public string Alias { get; init; }

	[JsonPropertyName("connection_id")]
	public string ConnectionId { get; init; }

	[JsonPropertyName("state")]
	public string State { get; init; }
}
