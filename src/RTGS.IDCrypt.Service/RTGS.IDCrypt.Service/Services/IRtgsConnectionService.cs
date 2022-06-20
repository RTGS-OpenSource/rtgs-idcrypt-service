using RTGS.IDCrypt.Service.Models.ConnectionInvitations;

namespace RTGS.IDCrypt.Service.Services;

public interface IRtgsConnectionService
{
	Task AcceptInvitationAsync(RtgsConnectionInvitation invitation, CancellationToken cancellationToken = default);
	Task<RtgsConnectionInvitation> CreateInvitationAsync(CancellationToken cancellationToken = default);
	Task DeleteAsync(string connectionId, CancellationToken cancellationToken = default);
}
