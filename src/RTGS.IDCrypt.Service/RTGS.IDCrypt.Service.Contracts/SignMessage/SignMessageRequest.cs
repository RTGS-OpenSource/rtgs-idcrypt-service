namespace RTGS.IDCrypt.Service.Contracts.SignMessage;

public record SignMessageRequest
{
	public string RtgsGlobalId { get; init; }
	public string Message { get; init; }
}
