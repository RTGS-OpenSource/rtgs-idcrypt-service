using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Helpers;

public interface IBankPartnerConnectionResolver
{
	BankPartnerConnection Resolve(List<BankPartnerConnection> bankPartnerConnections);
}
