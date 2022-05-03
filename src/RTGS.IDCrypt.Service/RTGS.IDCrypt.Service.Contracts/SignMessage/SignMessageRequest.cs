namespace RTGS.IDCrypt.Service.Contracts.SignMessage;

public record SignMessageRequest
{
	public string Alias { get; init; }
	public string RtgsGlobalId { get; init; }
	public string Message { get; init; }
}
