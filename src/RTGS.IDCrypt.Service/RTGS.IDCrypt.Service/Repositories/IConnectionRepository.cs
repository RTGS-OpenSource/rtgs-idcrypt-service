using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Repositories;

public interface IConnectionRepository
{
	Task SaveBankPartnerConnectionAsync(BankPartnerConnection connection, CancellationToken cancellationToken = default);
	Task ActivateBankPartnerConnectionAsync(string connectionId);
}
