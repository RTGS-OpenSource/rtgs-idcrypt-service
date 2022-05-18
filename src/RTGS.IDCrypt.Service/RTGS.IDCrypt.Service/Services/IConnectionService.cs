using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Services;

public interface IConnectionService
{
	Task AcceptInvitationAsync(ConnectionInvitation invitation, CancellationToken cancellationToken = default);

	Task<ConnectionInvitation> CreateConnectionInvitationAsync(CancellationToken cancellationToken = default);
}
