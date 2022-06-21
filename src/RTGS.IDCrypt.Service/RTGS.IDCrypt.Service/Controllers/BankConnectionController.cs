using Microsoft.AspNetCore.Mvc;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Extensions;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/bank-connection")]
[ApiController]
public class BankConnectionController : ControllerBase
{
	private readonly IBankConnectionService _bankConnectionService;
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;

	public BankConnectionController(
		IBankConnectionService bankConnectionService,
		IBankPartnerConnectionRepository bankPartnerConnectionRepository)
	{
		_bankConnectionService = bankConnectionService;
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
	}

	[HttpPost("cycle")]
	public async Task<IActionResult> Cycle(CycleConnectionRequest request)
	{
		await _bankConnectionService.CycleAsync(request.RtgsGlobalId);

		return Ok();
	}

	/// <summary>
	/// Endpoint to create an invitation for a bank.
	/// </summary>
	/// <param name="request">The data required to create an invitation.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="CreateConnectionInvitationResponse"/></returns>
	[HttpPost("create")]
	public async Task<IActionResult> Create(
		CreateConnectionInvitationForBankRequest request,
		CancellationToken cancellationToken = default)
	{
		var connectionInvitation = await _bankConnectionService.CreateInvitationAsync(request.RtgsGlobalId, cancellationToken);

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
		var invitation = new BankConnectionInvitation
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

		await _bankConnectionService.AcceptInvitationAsync(invitation, cancellationToken);

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
		await _bankConnectionService.DeleteAsync(connectionId, true, cancellationToken);

		return NoContent();
	}

	/// <summary>
	/// Endpoint to return distinct list of RtgsGlobalIds for Active partner backs that were Invited by us.
	/// </summary>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="OkObjectResult"/></returns>
	[HttpGet("InvitedPartnerIds")]
	public async Task<IActionResult> InvitedPartnerIds(CancellationToken cancellationToken = default)
	{
		var result = await _bankPartnerConnectionRepository.GetInvitedPartnerIdsAsync(cancellationToken);

		return Ok(result);
	}
}
