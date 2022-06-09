namespace RTGS.IDCrypt.Service.Contracts.BasicMessage;

public record ApproveBankPartnerRequest
{
	public string Iban { get; init; }
	public string RequestingBankDid { get; init; }
	public string ApprovingBankDid { get; init; }
}
