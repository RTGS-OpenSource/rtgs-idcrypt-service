using System.Text.Json.Serialization;

namespace RTGS.IDCrypt.Service.Contracts.SignMessage;

/// <summary>
/// Model of the response from /signmessage.
/// </summary>
public record SignMessageResponse
{
	/// <summary>
	/// The public DID signature.
	/// </summary>
	[JsonPropertyName("publicDidSignature")]
	public string PublicDidSignature { get; init; }
	/// <summary>
	/// The connection DID signature.
	/// </summary>
	[JsonPropertyName("pairwiseDidSignature")]
	public string PairwiseDidSignature { get; init; }
	/// <summary>
	/// Connection alias.
	/// </summary>
	[JsonPropertyName("alias")]
	public string Alias { get; init; }
}
