namespace RTGS.IDCrypt.Service.Models;

public record SetBankPartnershipOnlineRequest
{
	public string RequestingBankDid { get; init; }
	public string ApprovingBankDid { get; init; }
}
