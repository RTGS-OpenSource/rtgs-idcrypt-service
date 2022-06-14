using System.Collections.Generic;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Repositories;

public interface IBankPartnerConnectionRepository
{
	Task CreateAsync(BankPartnerConnection connection, CancellationToken cancellationToken = default);
	Task DeleteAsync(string connectionId, CancellationToken cancellationToken = default);
	Task DeleteAsync(BankPartnerConnection connection, CancellationToken cancellationToken = default);
	Task ActivateAsync(string connectionId, CancellationToken cancellationToken = default);
	Task<IEnumerable<string>> GetInvitedPartnerIdsAsync(CancellationToken cancellationToken = default);
	Task<BankPartnerConnection> GetAsync(string connectionId, CancellationToken cancellationToken = default);
	Task<BankPartnerConnection> GetAsync(string rtgsGlobalId, string alias, CancellationToken cancellationToken = default);
	Task<BankPartnerConnection> GetEstablishedAsync(string rtgsGlobalId, CancellationToken cancellationToken = default);
}
