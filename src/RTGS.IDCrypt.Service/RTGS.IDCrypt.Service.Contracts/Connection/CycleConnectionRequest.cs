namespace RTGS.IDCrypt.Service.Contracts.Connection;

public record CycleConnectionRequest
{
	public string RtgsGlobalId { get; init; }
}
