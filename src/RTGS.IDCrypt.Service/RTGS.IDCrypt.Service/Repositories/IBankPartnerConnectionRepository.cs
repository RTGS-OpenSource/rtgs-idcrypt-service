using System.Collections.Generic;
using System.Linq.Expressions;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Repositories;

public interface IBankPartnerConnectionRepository
{
	Task CreateAsync(BankPartnerConnection connection, CancellationToken cancellationToken = default);
	Task DeleteAsync(string connectionId, CancellationToken cancellationToken = default);
	Task ActivateAsync(string connectionId, CancellationToken cancellationToken = default);
	Task<IEnumerable<string>> GetInvitedPartnerIdsAsync(CancellationToken cancellationToken = default);
	Task<BankPartnerConnection> GetActiveAsync(string rtgsGlobalId, string alias, CancellationToken cancellationToken = default);
	Task<BankPartnerConnection> GetEstablishedAsync(string rtgsGlobalId, CancellationToken cancellationToken = default);
	Task<IEnumerable<string>> GetStaleConnectionIdsAsync(CancellationToken cancellationToken = default);
	Task<IEnumerable<string>> GetExpiredInvitationConnectionIdsAsync(CancellationToken cancellationToken = default);
	Task<bool> ActiveConnectionForBankExists(string alias, CancellationToken cancellationToken = default);
	Task<BankPartnerConnection> GetAsync(string rtgsGlobalId, string connectionId, CancellationToken cancellationToken = default);
	Task<IEnumerable<BankPartnerConnection>> FindAsync(Expression<Func<BankPartnerConnection, bool>> filter, CancellationToken cancellationToken = default);
}
