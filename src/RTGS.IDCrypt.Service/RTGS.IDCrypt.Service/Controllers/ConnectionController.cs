using Microsoft.AspNetCore.Mvc;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConnectionController : ControllerBase
{
	private readonly IConnectionsClient _connectionsClient;
	private readonly IWalletClient _walletClient;
	private readonly IAliasProvider _aliasProvider;

	public ConnectionController(IConnectionsClient connectionsClient, IWalletClient walletClient, IAliasProvider aliasProvider)
	{
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

		var createInvitationResponse = await _connectionsClient.CreateInvitationAsync(
			alias,
			autoAccept,
			multiUse,
			usePublicDid,
			cancellationToken);

		var publicDid = await _walletClient.GetPublicDidAsync(cancellationToken);

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
