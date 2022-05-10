using Microsoft.AspNetCore.Mvc;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Helpers;
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

	public ConnectionController(
		ILogger<ConnectionController> logger,
		IConnectionsClient connectionsClient,
		IWalletClient walletClient,
		IAliasProvider aliasProvider)
	{
		_logger = logger;
		_connectionsClient = connectionsClient;
		_walletClient = walletClient;
		_aliasProvider = aliasProvider;
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
			_logger.LogError(
				ex,
				"Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent");

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
}
