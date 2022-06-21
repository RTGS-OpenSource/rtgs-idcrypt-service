using Microsoft.AspNetCore.Mvc;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Extensions;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/rtgs-connection")]
[ApiController]
public class RtgsConnectionController : ControllerBase
{
	private readonly IRtgsConnectionService _rtgsConnectionService;

	public RtgsConnectionController(IRtgsConnectionService rtgsConnectionService)
	{
		_rtgsConnectionService = rtgsConnectionService;
	}

	/// <summary>
	/// Endpoint to create an invitation for RTGS.global.
	/// </summary>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="CreateConnectionInvitationResponse"/></returns>
	[HttpPost("create")]
	public async Task<IActionResult> Create(
		CancellationToken cancellationToken = default)
	{
		var connectionInvitation = await _rtgsConnectionService.CreateInvitationAsync(cancellationToken);

		var response = connectionInvitation.MapToContract();

		return Ok(response);
	}
}
