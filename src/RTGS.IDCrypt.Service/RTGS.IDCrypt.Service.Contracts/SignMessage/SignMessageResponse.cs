namespace RTGS.IDCrypt.Service.Contracts.SignMessage;

/// <summary>
/// Model of the response from /signmessage.
/// </summary>
public record SignMessageResponse
{
	/// <summary>
	/// The public DID signature.
	/// </summary>
	public string PublicDidSignature { get; init; }
	/// <summary>
	/// The connection DID signature.
	/// </summary>
	public string PairwiseDidSignature { get; init; }
	/// <summary>
	/// Connection alias.
	/// </summary>
	public string Alias { get; init; }
}
