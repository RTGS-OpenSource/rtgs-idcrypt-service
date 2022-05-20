using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Repositories;

public interface IConnectionRepository
{
	Task SavePendingBankPartnerConnectionAsync(PendingBankPartnerConnection pendingConnection, CancellationToken cancellationToken = default);
}
