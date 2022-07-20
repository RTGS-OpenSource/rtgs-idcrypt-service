using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.LoadTest;
public record SignDocumentResponse
{
	/// <summary>
	/// The connection DID signature.
	/// </summary>
	[JsonPropertyName("pairwise_did_signature")]
	public string PairwiseDidSignature { get; init; }

	/// <summary>
	/// The public DID signature.
	/// </summary>
	[JsonPropertyName("public_did_signature")]
	public string PublicDidSignature { get; init; }
}
