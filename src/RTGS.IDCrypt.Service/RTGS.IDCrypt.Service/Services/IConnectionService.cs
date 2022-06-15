using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Services;

public interface IConnectionService
{
	Task AcceptInvitationAsync(ConnectionInvitation invitation, CancellationToken cancellationToken = default);
	Task<ConnectionInvitation> CreateConnectionInvitationForBankAsync(string toRtgsGlobalId, CancellationToken cancellationToken = default);
	Task<ConnectionInvitation> CreateConnectionInvitationForRtgsAsync(CancellationToken cancellationToken = default);
	Task DeleteAsync(string connectionId, bool notifyPartner, CancellationToken cancellationToken = default);
	Task CycleConnectionForBankAsync(string rtgsGlobalId, CancellationToken cancellationToken = default);
}
