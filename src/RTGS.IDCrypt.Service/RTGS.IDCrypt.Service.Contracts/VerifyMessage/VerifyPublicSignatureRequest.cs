namespace RTGS.IDCrypt.Service.Contracts.VerifyMessage;

/// <summary>
/// Represents the data required to verify a public signature.
/// </summary>
public record VerifyPublicSignatureRequest
{
	/// <summary>
	/// The message to verify.
	/// </summary>
	public string Message { get; init; }
	/// <summary>
	/// The public signature of the signed message.
	/// </summary>
	public string PublicSignature { get; init; }
}
