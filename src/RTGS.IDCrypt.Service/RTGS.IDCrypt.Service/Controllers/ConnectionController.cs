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

		await _connectionsClient.CreateInvitationAsync(
			alias,
			autoAccept,
			multiUse,
			usePublicDid,
			cancellationToken);

		await _walletClient.GetPublicDidAsync(cancellationToken);

		var response = new CreateConnectionInvitationResponse();

		return Ok(response);
	}
}
