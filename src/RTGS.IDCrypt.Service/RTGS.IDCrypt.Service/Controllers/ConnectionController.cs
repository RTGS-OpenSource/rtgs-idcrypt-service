using Microsoft.AspNetCore.Mvc;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Extensions;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConnectionController : ControllerBase
{
	private readonly IConnectionService _connectionService;

	public ConnectionController(
		IConnectionService connectionService)
	{
		_connectionService = connectionService;
	}

	/// <summary>
	/// Endpoint to create an invitation.
	/// </summary>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="CreateConnectionInvitationResponse"/></returns>
	[HttpPost]
	public async Task<IActionResult> Post(
		CreateConnectionInvitationRequest createConnectionInvitationRequest,
		CancellationToken cancellationToken = default)
	{
		var createConnectionInvitationResponse = await _connectionService.CreateConnectionInvitationAsync(cancellationToken);

		var response = createConnectionInvitationResponse.MapToContract();

		return Ok(response);
	}

	/// <summary>
	/// Endpoint to accept an invitation.
	/// </summary>
	/// <param name="request">The data required to accept an invitation.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="AcceptedResult"/></returns>
	[HttpPost("Accept")]
	public async Task<IActionResult> Accept(
		AcceptConnectionInvitationRequest request,
		CancellationToken cancellationToken = default)
	{
		var invitation = new Models.ConnectionInvitation
		{
			Alias = request.Alias,
			Id = request.Id,
			Label = request.Label,
			RecipientKeys = request.RecipientKeys,
			ServiceEndpoint = request.ServiceEndpoint,
			Type = request.Type,
			PublicDid = request.AgentPublicDid
		};

		await _connectionService.AcceptInvitationAsync(invitation, cancellationToken);

		return Accepted();
	}
}
