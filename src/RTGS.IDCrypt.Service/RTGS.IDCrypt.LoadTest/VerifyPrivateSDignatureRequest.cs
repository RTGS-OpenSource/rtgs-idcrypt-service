using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.LoadTest;
internal record VerifyPrivateSignatureRequest<TDocument>
{
	[JsonPropertyName("connection_id")]
	public string ConnectionId { get; init; }

	[JsonPropertyName("document")]
	public TDocument Document { get; init; }

	[JsonPropertyName("signature")]
	public string Signature { get; init; }
}
