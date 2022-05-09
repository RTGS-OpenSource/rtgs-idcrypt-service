namespace RTGS.IDCrypt.Service.Contracts.VerifyMessage;

/// <summary>
/// Represents the data required to verity a signature
/// </summary>
public record VerifyPrivateSignatureRequest
{
	/// <summary>
	/// The RTGS.global id of the connection with which to sign the document 
	/// </summary>
	public string RtgsGlobalId { get; init; }
	/// <summary>
	/// The private message to verify.
	/// </summary>
	public string Message { get; init; }
	/// <summary>
	/// The private signature of the signed document.
	/// </summary>
	public string PrivateSignature { get; init; }
	/// <summary>
	/// The alias of the connection with which to sign the document
	/// </summary>
	public string Alias { get; init; }
}
