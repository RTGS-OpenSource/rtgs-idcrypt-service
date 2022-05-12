namespace RTGS.IDCrypt.Service.Contracts.VerifyMessage;

/// <summary>
/// Represents the data required to verify a message signed by the same party.
/// </summary>
public record VerifyOwnMessageRequest
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
