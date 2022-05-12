namespace RTGS.IDCrypt.Service.Contracts.SignMessage;

/// <summary>
///  Model of the sign message request.
/// </summary>
/// <param name="RtgsGlobalId">The RTGS Global identifier.</param>
/// <param name="Message">The JSON document to be signed.</param>
public record SignMessageRequest(string RtgsGlobalId, string Message);
