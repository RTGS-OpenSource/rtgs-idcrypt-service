namespace RTGS.IDCrypt.Service.Contracts.BasicMessage;

public record SetBankPartnershipOnlineRequest
{
	public string RequestingBankDid { get; init; }
	public string ApprovingBankDid { get; init; }
}
