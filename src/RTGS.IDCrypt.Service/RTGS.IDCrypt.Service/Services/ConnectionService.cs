using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.Services;

public class ConnectionService : IConnectionService
{
	private readonly IConnectionsClient _connectionsClient;
	private readonly ILogger<ConnectionService> _logger;

	public ConnectionService(IConnectionsClient connectionsClient, ILogger<ConnectionService> logger)
	{
		_connectionsClient = connectionsClient;
		_logger = logger;
	}

	public async Task<ConnectionResponse> AcceptInvitationAsync(ReceiveAndAcceptInvitationRequest request, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await _connectionsClient.ReceiveAndAcceptInvitationAsync(request, cancellationToken);

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when accepting invitation");

			throw;
		}
	}

	public async Task<CreateInvitationResponse> CreateInvitationAsync(string alias, CancellationToken cancellationToken = default)
	{
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		try
		{
			var createInvitationResponse = await _connectionsClient.CreateInvitationAsync(
				alias,
				autoAccept,
				multiUse,
				usePublicDid,
				cancellationToken);

			return createInvitationResponse;
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when sending CreateInvitation request with alias {Alias} to ID Crypt Cloud Agent",
				alias);

			throw;
		}
	}
}
