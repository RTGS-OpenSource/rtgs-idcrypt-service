namespace RTGS.IDCrypt.Service.Config;

public record BankPartnerConnectionsConfig
{
	public string BankPartnerConnectionsTableName { get; init; }
	public string PendingBankPartnerConnectionsTableName { get; init; }
}
