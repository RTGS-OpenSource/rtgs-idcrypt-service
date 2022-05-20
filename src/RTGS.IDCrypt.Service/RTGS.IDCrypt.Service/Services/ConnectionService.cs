using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;
using ConnectionInvitation = RTGS.IDCrypt.Service.Models.ConnectionInvitation;

namespace RTGS.IDCrypt.Service.Services;

public class ConnectionService : IConnectionService
{
	private readonly IConnectionsClient _connectionsClient;
	private readonly ILogger<ConnectionService> _logger;
	private readonly IConnectionRepository _connectionRepository;
	private readonly IAliasProvider _aliasProvider;
	private readonly IWalletClient _walletClient;

	public ConnectionService(
		IConnectionsClient connectionsClient,
		ILogger<ConnectionService> logger,
		IConnectionRepository connectionRepository,
		IAliasProvider aliasProvider,
		IWalletClient walletClient)
	{
		_connectionsClient = connectionsClient;
		_logger = logger;
		_connectionRepository = connectionRepository;
		_aliasProvider = aliasProvider;
		_walletClient = walletClient;
	}

	public async Task AcceptInvitationAsync(ConnectionInvitation invitation, CancellationToken cancellationToken = default)
	{
		try
		{
			var receiveAndAcceptInvitationRequest = new ReceiveAndAcceptInvitationRequest
			{
				Alias = invitation.Alias,
				Id = invitation.Id,
				Label = invitation.Label,
				RecipientKeys = invitation.RecipientKeys,
				ServiceEndpoint = invitation.ServiceEndpoint,
				Type = invitation.Type
			};

			var response = await _connectionsClient.ReceiveAndAcceptInvitationAsync(receiveAndAcceptInvitationRequest, cancellationToken);

			var connection = new BankPartnerConnection
			{
				PartitionKey = invitation.FromRtgsGlobalId,
				RowKey = response.Alias,
				ConnectionId = response.ConnectionId,
				Alias = response.Alias,
				PublicDid = invitation.PublicDid
			};

			await _connectionRepository.SaveBankPartnerConnectionAsync(connection, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when accepting invitation");

			throw;
		}
	}

	public async Task<ConnectionInvitation> CreateConnectionInvitationAsync(
		string rtgsGlobalId,
		CancellationToken cancellationToken = default)
	{
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		var alias = _aliasProvider.Provide();

		try
		{
			var createConnectionInvitationResponse = await _connectionsClient.CreateConnectionInvitationAsync(
				alias,
				autoAccept,
				multiUse,
				usePublicDid,
				cancellationToken);

			var connection = new BankPartnerConnection
			{
				PartitionKey = rtgsGlobalId,
				RowKey = alias,
				ConnectionId = createConnectionInvitationResponse.ConnectionId,
				Alias = alias
			};

			await _connectionRepository.SaveBankPartnerConnectionAsync(connection, cancellationToken);

			var publicDid = await _walletClient.GetPublicDidAsync(cancellationToken);

			return new ConnectionInvitation
			{
				Type = createConnectionInvitationResponse.Invitation.Type,
				Alias = createConnectionInvitationResponse.Alias,
				Label = createConnectionInvitationResponse.Invitation.Label,
				RecipientKeys = createConnectionInvitationResponse.Invitation.RecipientKeys,
				ServiceEndpoint = createConnectionInvitationResponse.Invitation.ServiceEndpoint,
				Id = createConnectionInvitationResponse.Invitation.Id,
				PublicDid = publicDid,
				Did = createConnectionInvitationResponse.Invitation.Did,
				ImageUrl = createConnectionInvitationResponse.Invitation.ImageUrl,
				InvitationUrl = createConnectionInvitationResponse.InvitationUrl,
				ToRtgsGlobalId = rtgsGlobalId
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when sending CreateConnectionInvitation request with alias {Alias} to ID Crypt Cloud Agent",
				alias);

			throw;
		}
	}
}
