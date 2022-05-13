namespace RTGS.IDCrypt.Service.Contracts.SignMessage;

/// <summary>
///  Model of the sign message request.
/// </summary>
public record SignMessageRequest
{
	/// <summary>
	/// The RTGS Global identifier.
	/// </summary>
	public string RtgsGlobalId { get; init; }

	/// <summary>
	/// The JSON document to be signed.
	/// </summary>
	public string Message { get; init; }
}
