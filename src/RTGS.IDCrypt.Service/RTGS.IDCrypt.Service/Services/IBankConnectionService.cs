using RTGS.IDCrypt.Service.Models.ConnectionInvitations;

namespace RTGS.IDCrypt.Service.Services;

public interface IBankConnectionService
{
	Task AcceptInvitationAsync(BankConnectionInvitation invitation, CancellationToken cancellationToken = default);
	Task<BankConnectionInvitation> CreateInvitationAsync(string toRtgsGlobalId, CancellationToken cancellationToken = default);
	Task DeleteAsync(string connectionId, bool notifyPartner, CancellationToken cancellationToken = default);
	Task CycleAsync(string rtgsGlobalId, CancellationToken cancellationToken = default);
	Task DeleteBankAsync(string itgsGlobalId, CancellationToken cancellationToken = default);
}
