namespace RTGS.Service.Dtos;

public record SignMessageRequest
{
	public string Message { get; init; }
	public string Alias { get; init; }
}
