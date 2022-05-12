namespace RTGS.IDCrypt.Service.Contracts.VerifyMessage;

/// <summary>
/// Represents the data required to verify a signature.
/// </summary>
/// <param name="RtgsGlobalId">The RTGS.global Id of the connection with which to sign the document.</param>
/// <param name="Message">The message to verify.</param>
/// <param name="PrivateSignature">The private signature of the signed message.</param>
/// <param name="Alias">The alias of the connection with which to sign the message.</param>
public record VerifyPrivateSignatureRequest(
	string RtgsGlobalId,
	string Message,
	string PrivateSignature,
	string Alias);
