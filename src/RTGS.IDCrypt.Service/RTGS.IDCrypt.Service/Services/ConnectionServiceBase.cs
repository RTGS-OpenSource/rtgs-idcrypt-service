using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.Services;

public class ConnectionServiceBase
{
	protected IConnectionsClient ConnectionsClient { get; }

	protected ConnectionServiceBase(IConnectionsClient connectionsClient)
	{
		ConnectionsClient = connectionsClient;
	}

	protected async Task<CreateConnectionInvitationResponse> CreateAgentConnectionInvitationAsync(
		string alias,
		CancellationToken cancellationToken)
	{
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		var createConnectionInvitationResponse = await ConnectionsClient.CreateConnectionInvitationAsync(
			alias,
			autoAccept,
			multiUse,
			usePublicDid,
			cancellationToken);

		return createConnectionInvitationResponse;
	}
}
