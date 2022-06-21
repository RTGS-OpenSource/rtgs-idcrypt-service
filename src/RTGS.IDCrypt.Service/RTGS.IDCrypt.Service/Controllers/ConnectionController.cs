using Microsoft.AspNetCore.Mvc;
using RTGS.IDCrypt.Service.Contracts.Connection;

namespace RTGS.IDCrypt.Service.Controllers;

// TODO: remove this once sdk/e2e/simulators updated with latest endpoints
[Route("api/connection")]
[ApiController]
[Obsolete]
public class ConnectionController : ControllerBase
{
	[HttpPost("for-rtgs")]
	public IActionResult ForRtgs(CancellationToken cancellationToken = default) =>
		RedirectPermanentPreserveMethod("/api/rtgs-connection/create");

	[HttpPost("cycle")]
	public IActionResult Cycle(CycleConnectionRequest request) =>
		RedirectPermanentPreserveMethod("/api/bank-connection/cycle");

	[HttpPost("for-bank")]
	public IActionResult ForBank(CreateConnectionInvitationForBankRequest request, CancellationToken cancellationToken = default) =>
		RedirectPermanentPreserveMethod("/api/bank-connection/create");

	[HttpPost("accept")]
	public ActionResult Accept(
		AcceptConnectionInvitationRequest request, CancellationToken cancellationToken = default) =>
		RedirectPermanentPreserveMethod("/api/bank-connection/accept");

	[HttpDelete("{connectionId}")]
	public IActionResult Delete(string connectionId, CancellationToken cancellationToken = default) =>
		RedirectPermanentPreserveMethod($"/api/bank-connection/{connectionId}");

	[HttpGet("InvitedPartnerIds")]
	public ActionResult InvitedPartnerIds(CancellationToken cancellationToken = default) =>
		RedirectPermanentPreserveMethod("/api/bank-connection/InvitedPartnerIds");
}
