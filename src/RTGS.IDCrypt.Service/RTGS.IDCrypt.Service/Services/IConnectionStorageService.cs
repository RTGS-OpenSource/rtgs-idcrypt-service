using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Services;

public interface IConnectionStorageService
{
	Task SavePendingBankPartnerConnectionAsync(PendingBankPartnerConnection pendingConnection, CancellationToken cancellationToken = default);
}
