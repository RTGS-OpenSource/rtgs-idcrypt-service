using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.Services;

public interface IConnectionService
{
	Task<ConnectionResponse> AcceptInvitationAsync(ReceiveAndAcceptInvitationRequest receiveAndAcceptInvitationRequest, CancellationToken cancellationToken = default);
	Task<CreateInvitationResponse> CreateInvitationAsync(string alias, CancellationToken cancellationToken = default);
}
