﻿using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Extensions;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Services;

public class BankConnectionService : ConnectionServiceBase, IBankConnectionService
{
	private readonly ILogger<BankConnectionService> _logger;
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly IAliasProvider _aliasProvider;
	private readonly IWalletClient _walletClient;
	private readonly IBasicMessageClient _basicMessageClient;
	private readonly string _rtgsGlobalId;

	public BankConnectionService(
		IConnectionsClient connectionsClient,
		ILogger<BankConnectionService> logger,
		IBankPartnerConnectionRepository bankPartnerConnectionRepository,
		IAliasProvider aliasProvider,
		IWalletClient walletClient,
		IOptions<CoreConfig> coreOptions,
		IBasicMessageClient basicMessageClient)
		: base(connectionsClient)
	{
		_logger = logger;
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
		_aliasProvider = aliasProvider;
		_walletClient = walletClient;
		_basicMessageClient = basicMessageClient;

		if (string.IsNullOrWhiteSpace(coreOptions.Value.RtgsGlobalId))
		{
			throw new ArgumentException("RtgsGlobalId configuration option is not set.");
		}

		_rtgsGlobalId = coreOptions.Value.RtgsGlobalId;
	}

	public async Task AcceptInvitationAsync(BankConnectionInvitation invitation, CancellationToken cancellationToken = default)
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
			_logger.LogError(ex, "Error occurred when accepting bank invitation");

			throw;
		}
	}

	public async Task<BankConnectionInvitation> CreateInvitationAsync(
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

	public async Task CycleAsync(string rtgsGlobalId, CancellationToken cancellationToken = default)
	{
		try
		{
			var establishedConnection = await _bankPartnerConnectionRepository.GetEstablishedAsync(rtgsGlobalId, cancellationToken);

			var invitation = await DoCreateConnectionInvitationForBankAsync(rtgsGlobalId, cancellationToken);

			await _basicMessageClient.SendAsync(establishedConnection.ConnectionId, nameof(BankConnectionInvitation), invitation, cancellationToken);
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
				ConnectionsClient.DeleteConnectionAsync(connectionId, cancellationToken),
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

	/// <summary>
	/// Deletes a banks
	/// </summary>
	/// <remarks>
	/// If we are deleting ourselves then delete all connections, if another delete all connections to it.</remarks>
	/// <param name="rtgsGlobalId">RTGS Global Id that belongs to bank being deleted</param>
	/// <param name="cancellationToken">Cancellation Token</param>
	public async Task DeleteBankAsync(string rtgsGlobalId, CancellationToken cancellationToken = default)
	{
		if (rtgsGlobalId == _rtgsGlobalId)
		{
			var allConnections = await _bankPartnerConnectionRepository.GetMatchingAsync(null, cancellationToken);
			foreach (BankPartnerConnection bankPartnerConnection in allConnections)
			{
				await _bankPartnerConnectionRepository.DeleteAsync(bankPartnerConnection.ConnectionId,
					cancellationToken);
				
				await ConnectionsClient.DeleteConnectionAsync(bankPartnerConnection.ConnectionId, cancellationToken);
			}
		}
		else
		{
			var partnerConnections = await _bankPartnerConnectionRepository.GetMatchingAsync(conn=>conn.PartitionKey == rtgsGlobalId, cancellationToken);
			foreach (BankPartnerConnection bankPartnerConnection in partnerConnections)
			{
				await _bankPartnerConnectionRepository.DeleteAsync(bankPartnerConnection.ConnectionId,
					cancellationToken);
				
				await ConnectionsClient.DeleteConnectionAsync(bankPartnerConnection.ConnectionId, cancellationToken);
			}
		}
	}
	
	private async Task<BankConnectionInvitation> DoCreateConnectionInvitationForBankAsync(string toRtgsGlobalId, CancellationToken cancellationToken)
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

		var connectionInvitation = createConnectionInvitationResponse.MapToConnectionInvitation<BankConnectionInvitation>(publicDid, _rtgsGlobalId);

		return connectionInvitation;
	}
}
