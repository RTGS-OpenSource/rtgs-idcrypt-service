using Microsoft.AspNetCore.Mvc;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Extensions;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConnectionController : ControllerBase
{
	private readonly IConnectionService _connectionService;
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;

	public ConnectionController(
		IConnectionService connectionService,
		IBankPartnerConnectionRepository bankPartnerConnectionRepository)
	{
		_connectionService = connectionService;
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
	}

	/// <summary>
	/// Endpoint to create an invitation for RTGS.global.
	/// </summary>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="CreateConnectionInvitationResponse"/></returns>
	[HttpPost("for-rtgs")]
	public async Task<IActionResult> ForRtgs(
		CancellationToken cancellationToken = default)
	{
		var connectionInvitation = await _connectionService.CreateConnectionInvitationForRtgsAsync(cancellationToken);

		var response = connectionInvitation.MapToContract();

		return Ok(response);
	}

	/// <summary>
	/// Endpoint to create an invitation for a bank.
	/// </summary>
	/// <param name="request">The data required to create an invitation.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="CreateConnectionInvitationResponse"/></returns>
	[HttpPost("for-bank")]
	public async Task<IActionResult> ForBank(
		CreateConnectionInvitationForBankRequest request,
		CancellationToken cancellationToken = default)
	{
		var connectionInvitation = await _connectionService.CreateConnectionInvitationForBankAsync(request.RtgsGlobalId, cancellationToken);

		var response = connectionInvitation.MapToContract();

		return Ok(response);
	}

	/// <summary>
	/// Endpoint to accept an invitation.
	/// </summary>
	/// <param name="request">The data required to accept an invitation.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="AcceptedResult"/></returns>
	[HttpPost("accept")]
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
			PublicDid = request.AgentPublicDid,
			FromRtgsGlobalId = request.RtgsGlobalId
		};

		await _connectionService.AcceptInvitationAsync(invitation, cancellationToken);

		return Accepted();
	}

	/// <summary>
	/// Endpoint to delete a connection.
	/// </summary>
	/// <param name="connectionId">The identifier of the connection to delete.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="NoContentResult"/></returns>
	[HttpDelete("{connectionId}")]
	public async Task<IActionResult> Delete(
		string connectionId,
		CancellationToken cancellationToken = default)
	{
		await _connectionService.DeleteAsync(connectionId, cancellationToken);

		return NoContent();
	}

	/// <summary>
	/// Endpoint to return distinct list of RtgsGlobalIds for Active partner backs that were Invited by us.
	/// </summary>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="OkObjectResult"/></returns>
	[HttpGet("InvitedPartnerIds")]
	public IActionResult InvitedPartnerIds(CancellationToken cancellationToken = default)
	{
		var result = _bankPartnerConnectionRepository.GetInvitedPartnerIds(cancellationToken);

		return Ok(result);
	}
}
