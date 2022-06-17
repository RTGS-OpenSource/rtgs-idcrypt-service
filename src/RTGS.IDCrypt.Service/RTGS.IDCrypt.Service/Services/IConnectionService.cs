using RTGS.IDCrypt.Service.Models.ConnectionInvitations;

namespace RTGS.IDCrypt.Service.Services;

public interface IConnectionService
{
	Task AcceptBankInvitationAsync(BankConnectionInvitation invitation, CancellationToken cancellationToken = default);
	Task AcceptRtgsInvitationAsync(RtgsConnectionInvitation invitation, CancellationToken cancellationToken = default);
	Task<BankConnectionInvitation> CreateConnectionInvitationForBankAsync(string toRtgsGlobalId, CancellationToken cancellationToken = default);
	Task<RtgsConnectionInvitation> CreateConnectionInvitationForRtgsAsync(CancellationToken cancellationToken = default);
	Task DeletePartnerAsync(string connectionId, bool notifyPartner, CancellationToken cancellationToken = default);
	Task CycleConnectionForBankAsync(string rtgsGlobalId, CancellationToken cancellationToken = default);
	Task DeleteRtgsAsync(string connectionId, CancellationToken cancellationToken = default);
}
