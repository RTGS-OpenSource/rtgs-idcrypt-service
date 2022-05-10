namespace RTGS.IDCrypt.Service.Contracts.VerifyMessage;

/// <summary>
/// Represents the response to a <see cref="VerifyPrivateSignatureRequest"/>.
/// </summary>
public record VerifyPrivateSignatureResponse
{
	/// <summary>
	/// True if the signature is verified, otherwise false.
	/// </summary>
	public bool Verified { get; init; }
}
