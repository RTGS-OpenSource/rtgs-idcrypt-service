using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Repositories;

public interface IBankPartnerConnectionRepository
{
	Task CreateAsync(BankPartnerConnection connection, CancellationToken cancellationToken = default);
	Task DeleteAsync(string connectionId, CancellationToken cancellationToken = default);
	Task ActivateAsync(string connectionId, CancellationToken cancellationToken = default);
	Task<BankPartnerConnection> GetAsync(string connectionId, CancellationToken cancellationToken = default);
}

