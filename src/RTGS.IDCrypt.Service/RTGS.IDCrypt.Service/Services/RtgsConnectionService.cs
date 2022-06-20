using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Extensions;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Services;

public class RtgsConnectionService : ConnectionServiceBase, IRtgsConnectionService
{
	private readonly ILogger<RtgsConnectionService> _logger;
	private readonly IRtgsConnectionRepository _rtgsConnectionRepository;
	private readonly IAliasProvider _aliasProvider;
	private readonly IWalletClient _walletClient;
	private readonly string _rtgsGlobalId;

	public RtgsConnectionService(
		IConnectionsClient connectionsClient,
		ILogger<RtgsConnectionService> logger,
		IRtgsConnectionRepository rtgsConnectionRepository,
		IAliasProvider aliasProvider,
		IWalletClient walletClient,
		IOptions<CoreConfig> coreOptions)
		: base(connectionsClient)
	{
		_logger = logger;
		_rtgsConnectionRepository = rtgsConnectionRepository;
		_aliasProvider = aliasProvider;
		_walletClient = walletClient;

		if (string.IsNullOrWhiteSpace(coreOptions.Value.RtgsGlobalId))
		{
			throw new ArgumentException("RtgsGlobalId configuration option is not set.");
		}

		_rtgsGlobalId = coreOptions.Value.RtgsGlobalId;
	}

	public async Task AcceptInvitationAsync(RtgsConnectionInvitation invitation, CancellationToken cancellationToken = default)
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

			var response = await ConnectionsClient.ReceiveAndAcceptInvitationAsync(receiveAndAcceptInvitationRequest, cancellationToken);

			var connection = new RtgsConnection
			{
				PartitionKey = invitation.Alias,
				RowKey = response.ConnectionId,
				ConnectionId = response.ConnectionId,
				Alias = response.Alias,
				Status = ConnectionStatuses.Pending,
			};

			await _rtgsConnectionRepository.CreateAsync(connection, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when accepting RTGS invitation");

			throw;
		}
	}

	public async Task<RtgsConnectionInvitation> CreateInvitationAsync(
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

			var connectionInvitation = createConnectionInvitationResponse.MapToConnectionInvitation<RtgsConnectionInvitation>(publicDid, _rtgsGlobalId);

			return connectionInvitation;
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when creating connection invitation for RTGS");

			throw;
		}
	}

	public async Task DeleteAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		Task aggregateTask = null;

		try
		{
			aggregateTask = Task.WhenAll(
				ConnectionsClient.DeleteConnectionAsync(connectionId, cancellationToken),
				_rtgsConnectionRepository.DeleteAsync(connectionId, cancellationToken));

			await aggregateTask;
		}
		catch (Exception e)
		{
			aggregateTask?.Exception?.InnerExceptions.ToList()
				.ForEach(ex => _logger.LogError(ex, "Error occurred when deleting RTGS connection"));

			throw aggregateTask?.Exception ?? e;
		}
	}
}
