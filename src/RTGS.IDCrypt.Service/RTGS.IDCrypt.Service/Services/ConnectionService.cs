using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Extensions;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;
using ConnectionInvitation = RTGS.IDCrypt.Service.Models.ConnectionInvitation;
using CreateConnectionInvitationResponse = RTGS.IDCryptSDK.Connections.Models.CreateConnectionInvitationResponse;

namespace RTGS.IDCrypt.Service.Services;

public class ConnectionService : IConnectionService
{
	private readonly IConnectionsClient _connectionsClient;
	private readonly ILogger<ConnectionService> _logger;
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly IRtgsConnectionRepository _rtgsConnectionRepository;
	private readonly IAliasProvider _aliasProvider;
	private readonly IWalletClient _walletClient;
	private readonly IBasicMessageClient _basicMessageClient;
	private readonly string _rtgsGlobalId;

	public ConnectionService(
		IConnectionsClient connectionsClient,
		ILogger<ConnectionService> logger,
		IBankPartnerConnectionRepository bankPartnerConnectionRepository,
		IRtgsConnectionRepository rtgsConnectionRepository,
		IAliasProvider aliasProvider,
		IWalletClient walletClient,
		IOptions<CoreConfig> coreOptions,
		IBasicMessageClient basicMessageClient)
	{
		_connectionsClient = connectionsClient;
		_logger = logger;
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
		_rtgsConnectionRepository = rtgsConnectionRepository;
		_aliasProvider = aliasProvider;
		_walletClient = walletClient;
		_basicMessageClient = basicMessageClient;

		if (string.IsNullOrWhiteSpace(coreOptions.Value.RtgsGlobalId))
		{
			throw new ArgumentException("RtgsGlobalId configuration option is not set.");
		}

		_rtgsGlobalId = coreOptions.Value.RtgsGlobalId;
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
				PublicDid = invitation.PublicDid,
				Status = ConnectionStatuses.Pending,
				Role = ConnectionRoles.Invitee
			};

			await _bankPartnerConnectionRepository.CreateAsync(connection, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when accepting invitation");

			throw;
		}
	}

	public async Task<ConnectionInvitation> CreateConnectionInvitationForBankAsync(
		string toRtgsGlobalId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			return await DoCreateConnectionInvitationForBankAsync(toRtgsGlobalId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when creating connection invitation for bank {RtgsGlobalId}",
				toRtgsGlobalId);

			throw;
		}
	}

	public async Task CycleConnectionForBankAsync(string rtgsGlobalId, CancellationToken cancellationToken = default)
	{
		try
		{
			var establishedConnection = await _bankPartnerConnectionRepository.GetEstablishedAsync(rtgsGlobalId, cancellationToken);

			var invitation = await DoCreateConnectionInvitationForBankAsync(rtgsGlobalId, cancellationToken);

			await _basicMessageClient.SendAsync(establishedConnection.ConnectionId, nameof(ConnectionInvitation), invitation, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when cycling connection for bank {RtgsGlobalId}",
				rtgsGlobalId);

			throw;
		}
	}

	public async Task<ConnectionInvitation> CreateConnectionInvitationForRtgsAsync(
		CancellationToken cancellationToken = default)
	{
		var alias = _aliasProvider.Provide();

		try
		{
			var createConnectionInvitationResponse = await CreateAgentConnectionInvitationAsync(alias, cancellationToken);

			var publicDid = await _walletClient.GetPublicDidAsync(cancellationToken);

			var connection = new RtgsConnection
			{
				PartitionKey = alias,
				RowKey = createConnectionInvitationResponse.ConnectionId,
				Alias = alias,
				ConnectionId = createConnectionInvitationResponse.ConnectionId,
				Status = ConnectionStatuses.Pending
			};

			await _rtgsConnectionRepository.CreateAsync(connection, cancellationToken);

			var connectionInvitation = createConnectionInvitationResponse.MapToConnectionInvitation(publicDid, _rtgsGlobalId);

			return connectionInvitation;
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when creating connection invitation for RTGS.global");

			throw;
		}
	}

	public async Task DeleteAsync(string connectionId, bool notifyPartner, CancellationToken cancellationToken = default)
	{
		if (notifyPartner)
		{
			try
			{
				await _basicMessageClient.SendAsync(
					connectionId,
					nameof(DeleteBankPartnerConnectionBasicMessage),
					new DeleteBankPartnerConnectionBasicMessage(),
					cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred when notifying partner of deleting connection");

				throw;
			}
		}

		Task aggregateTask = null;

		try
		{
			aggregateTask = Task.WhenAll(
				_connectionsClient.DeleteConnectionAsync(connectionId, cancellationToken),
				_bankPartnerConnectionRepository.DeleteAsync(connectionId, cancellationToken));

			await aggregateTask;
		}
		catch (Exception e)
		{
			aggregateTask?.Exception?.InnerExceptions.ToList()
				.ForEach(ex => _logger.LogError(ex, "Error occurred when deleting connection"));

			throw aggregateTask?.Exception ?? e;
		}
	}

	private async Task<ConnectionInvitation> DoCreateConnectionInvitationForBankAsync(string toRtgsGlobalId, CancellationToken cancellationToken)
	{
		var alias = _aliasProvider.Provide();
		var createConnectionInvitationResponse = await CreateAgentConnectionInvitationAsync(alias, cancellationToken);

		var publicDid = await _walletClient.GetPublicDidAsync(cancellationToken);

		var connection = new BankPartnerConnection
		{
			PartitionKey = toRtgsGlobalId,
			RowKey = alias,
			Alias = alias,
			ConnectionId = createConnectionInvitationResponse.ConnectionId,
			Status = ConnectionStatuses.Pending,
			PublicDid = publicDid,
			Role = ConnectionRoles.Inviter
		};

		await _bankPartnerConnectionRepository.CreateAsync(connection, cancellationToken);

		var connectionInvitation = createConnectionInvitationResponse.MapToConnectionInvitation(publicDid, _rtgsGlobalId);

		return connectionInvitation;
	}

	private async Task<CreateConnectionInvitationResponse> CreateAgentConnectionInvitationAsync(
		string alias,
		CancellationToken cancellationToken)
	{
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		var createConnectionInvitationResponse = await _connectionsClient.CreateConnectionInvitationAsync(
				alias,
				autoAccept,
				multiUse,
				usePublicDid,
				cancellationToken);

		return createConnectionInvitationResponse;
	}
}
