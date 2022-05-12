namespace RTGS.IDCrypt.Service.Contracts.VerifyMessage;

/// <summary>
/// Represents the data required to verify a signature.
/// </summary>
public record VerifyPrivateSignatureRequest
{
	/// <summary>
	/// The RTGS.global Id of the connection with which to sign the document.
	/// </summary>
	public string RtgsGlobalId { get; init; }

	/// <summary>
	/// The message to verify.
	/// </summary>
	public string Message { get; init; }

	/// <summary>
	/// The private signature of the signed message.
	/// </summary>
	public string PrivateSignature { get; init; }

	/// <summary>
	/// The alias of the connection with which to sign the message.
	/// </summary>
	public string Alias { get; init; }
}
