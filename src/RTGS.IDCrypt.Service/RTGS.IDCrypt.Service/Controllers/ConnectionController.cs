using Microsoft.AspNetCore.Mvc;
using RTGS.IDCrypt.Service.Contracts.Connection;

namespace RTGS.IDCrypt.Service.Controllers;

// TODO: remove this once sdk/e2e/simulators updated with latest endpoints
[Route("api/connection")]
[ApiController]
[Obsolete]
public class ConnectionController : ControllerBase
{
	[HttpPost("cycle")]
	public IActionResult Cycle(CycleConnectionRequest request) =>
		RedirectPreserveMethod("/api/bank-connection/cycle");

	[HttpPost("for-bank")]
	public IActionResult ForBank(CreateConnectionInvitationForBankRequest request, CancellationToken cancellationToken = default) =>
		RedirectPreserveMethod("/api/bank-connection/create");

	[HttpPost("for-rtgs")]
	public IActionResult ForRtgs(CancellationToken cancellationToken = default) =>
		RedirectPreserveMethod("/api/rtgs-connection/create");

	[HttpPost("accept")]
	public ActionResult Accept(
		AcceptConnectionInvitationRequest request, CancellationToken cancellationToken = default) =>
		RedirectPreserveMethod("a/pi/bank-connection/accept");

	[HttpDelete("{connectionId}")]
	public IActionResult Delete(string connectionId, CancellationToken cancellationToken = default) =>
		RedirectPreserveMethod($"/api/bank-connection/{connectionId}");

	[HttpGet("InvitedPartnerIds")]
	public ActionResult InvitedPartnerIds(CancellationToken cancellationToken = default) =>
		RedirectPreserveMethod("/api/bank-connection/InvitedPartnerIds");
}
