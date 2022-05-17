using Microsoft.AspNetCore.Mvc;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;
using ConnectionInvitation = RTGS.IDCrypt.Service.Contracts.Connection.ConnectionInvitation;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConnectionController : ControllerBase
{
	private readonly ILogger<ConnectionController> _logger;
	private readonly IWalletClient _walletClient;
	private readonly IAliasProvider _aliasProvider;
	private readonly IConnectionService _connectionService;
	private readonly IConnectionStorageService _connectionStorageService;

	public ConnectionController(
		ILogger<ConnectionController> logger,
		IWalletClient walletClient,
		IAliasProvider aliasProvider,
		IConnectionService connectionService,
		IConnectionStorageService connectionStorageService)
	{
		_logger = logger;
		_walletClient = walletClient;
		_aliasProvider = aliasProvider;
		_connectionService = connectionService;
		_connectionStorageService = connectionStorageService;
	}

	/// <summary>
	/// Endpoint to create an invitation.
	/// </summary>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="CreateConnectionInvitationResponse"/></returns>
	[HttpPost]
	public async Task<IActionResult> Post(CancellationToken cancellationToken)
	{
		var alias = _aliasProvider.Provide();

		var createInvitationResponse = await _connectionService.CreateInvitationAsync(alias, cancellationToken);

		var publicDid = await GetPublicDid(cancellationToken);

		var pendingConnection = new PendingBankPartnerConnection
		{
			PartitionKey = createInvitationResponse.ConnectionId,
			RowKey = alias,
			ConnectionId = createInvitationResponse.ConnectionId,
			Alias = alias
		};

		await _connectionStorageService.SavePendingBankPartnerConnectionAsync(pendingConnection, cancellationToken);

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

	/// <summary>
	/// Endpoint to accept an invitation
	/// </summary>
	/// <param name="request">The data required to accept an invitations</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="AcceptedResult"/></returns>
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

		var response = await _connectionService.AcceptInvitationAsync(receiveAndAcceptInvitationRequest, cancellationToken);

		var pendingConnection = new PendingBankPartnerConnection
		{
			PartitionKey = response.ConnectionId,
			RowKey = response.Alias,
			ConnectionId = response.ConnectionId,
			Alias = response.Alias
		};

		await _connectionStorageService.SavePendingBankPartnerConnectionAsync(pendingConnection, cancellationToken);

		return Accepted();
	}



	private async Task<string> GetPublicDid(CancellationToken cancellationToken)
	{
		try
		{
			var publicDid = await _walletClient.GetPublicDidAsync(cancellationToken);

			return publicDid;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent");

			throw;
		}
	}
}
