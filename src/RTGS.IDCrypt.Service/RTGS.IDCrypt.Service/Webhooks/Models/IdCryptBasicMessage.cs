using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Webhooks.Models;

public record IdCryptBasicMessage
{
	[JsonPropertyName("connection_id")]
	public string ConnectionId { get; set; }

	[JsonPropertyName("message_id")]
	public string MessageId { get; set; }

	[JsonPropertyName("content")]
	public string Content { get; set; }
}
