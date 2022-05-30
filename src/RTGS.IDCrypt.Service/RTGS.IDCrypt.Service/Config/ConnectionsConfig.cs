namespace RTGS.IDCrypt.Service.Config;

public record ConnectionsConfig
{
	public string BankPartnerConnectionsTableName { get; init; }
	public string RtgsConnectionsTableName { get; init; }
	public TimeSpan MinimumConnectionAge { get; init; }
}
