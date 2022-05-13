using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;
using ConnectionInvitation = RTGS.IDCrypt.Service.Contracts.Connection.ConnectionInvitation;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConnectionController : ControllerBase
{
	private readonly ILogger<ConnectionController> _logger;
	private readonly IConnectionsClient _connectionsClient;
	private readonly IWalletClient _walletClient;
	private readonly IAliasProvider _aliasProvider;
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly BankPartnerConnectionsConfig _bankPartnerConnectionsConfig;

	public ConnectionController(
		ILogger<ConnectionController> logger,
		IConnectionsClient connectionsClient,
		IWalletClient walletClient,
		IAliasProvider aliasProvider,
		IStorageTableResolver storageTableResolver,
		IOptions<BankPartnerConnectionsConfig> bankPartnerConnectionsOptions)
	{
		_logger = logger;
		_connectionsClient = connectionsClient;
		_walletClient = walletClient;
		_aliasProvider = aliasProvider;
		_storageTableResolver = storageTableResolver;
		_bankPartnerConnectionsConfig = bankPartnerConnectionsOptions.Value;
	}

	[HttpPost]
	public async Task<IActionResult> Post(CancellationToken cancellationToken)
	{
		var alias = _aliasProvider.Provide();

		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		CreateInvitationResponse createInvitationResponse;

		try
		{
			createInvitationResponse = await _connectionsClient.CreateInvitationAsync(
			alias,
			autoAccept,
			multiUse,
			usePublicDid,
			cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when sending CreateInvitation request with alias {Alias} to ID Crypt Cloud Agent",
				alias);

			throw;
		}

		string publicDid;

		try
		{
			publicDid = await _walletClient.GetPublicDidAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent");

			throw;
		}

		var response = new CreateConnectionInvitationResponse
		{
			ConnectionId = createInvitationResponse.ConnectionId,
			Alias = alias,
			AgentPublicDid = publicDid,
			InvitationUrl = createInvitationResponse.InvitationUrl,
			Invitation = new ConnectionInvitation
			{
				Did = createInvitationResponse.Invitation.Did,
				Id = createInvitationResponse.Invitation.Id,
				ImageUrl = createInvitationResponse.Invitation.ImageUrl,
				Label = createInvitationResponse.Invitation.Label,
				RecipientKeys = createInvitationResponse.Invitation.RecipientKeys,
				ServiceEndpoint = createInvitationResponse.Invitation.ServiceEndpoint,
				Type = createInvitationResponse.Invitation.Type
			}
		};

		return Ok(response);
	}

	[HttpPost("Accept")]
	public async Task<IActionResult> Accept(
		AcceptConnectionInvitationRequest request,
		CancellationToken cancellationToken)
	{
		var receiveAndAcceptInvitationRequest = new ReceiveAndAcceptInvitationRequest
		{
			Alias = request.Alias,
			Id = request.Id,
			Label = request.Label,
			RecipientKeys = request.RecipientKeys,
			ServiceEndpoint = request.ServiceEndpoint,
			Type = request.Type
		};

		try
		{
			var response = await _connectionsClient.ReceiveAndAcceptInvitationAsync(receiveAndAcceptInvitationRequest, cancellationToken);

			var pendingConnection = new PendingBankPartnerConnection
			{
				PartitionKey = response.ConnectionId,
				RowKey = response.Alias,
				ConnectionId = response.ConnectionId,
				Alias = response.Alias
			};

			var tableClient = _storageTableResolver.GetTable(_bankPartnerConnectionsConfig.PendingBankPartnerConnectionsTableName);

			await tableClient.AddEntityAsync(pendingConnection, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when accepting invitation");

			throw;
		}

		return Accepted();
	}
}
