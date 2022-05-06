using Microsoft.AspNetCore.Mvc;
using RTGS.IDCryptSDK.Connections;

namespace RTGS.IDCrypt.Service.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ConnectionController : ControllerBase
	{
		private readonly IConnectionsClient _connectionsClient;

		public ConnectionController(IConnectionsClient connectionsClient)
		{
			_connectionsClient = connectionsClient;
		}

		[HttpPost]
		public IActionResult Post()
		{
			_connectionsClient.CreateInvitationAsync(
				createConnectionInvitationRequest.Alias,
				createConnectionInvitationRequest.AutoAccept,
				createConnectionInvitationRequest.MultiUse,
				createConnectionInvitationRequest.UsePublicDid);

			return Ok();
		}
	}
}
