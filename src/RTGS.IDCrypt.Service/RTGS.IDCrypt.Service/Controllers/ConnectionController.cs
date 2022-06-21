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
		Redirect("/api/rtgs-connection/create", cancellationToken);

	[HttpPost("cycle")]
	public IActionResult Cycle(CycleConnectionRequest request) =>
		Redirect("/api/bank-connection/cycle", request);

	[HttpPost("for-bank")]
	public IActionResult ForBank(CreateConnectionInvitationForBankRequest request, CancellationToken cancellationToken = default) =>
		Redirect("/api/bank-connection/create", request, cancellationToken);

	[HttpPost("accept")]
	public IActionResult Accept(AcceptConnectionInvitationRequest request, CancellationToken cancellationToken = default) =>
		Redirect("/api/bank-connection/accept", request, cancellationToken);

	[HttpDelete("{connectionId}")]
	public IActionResult Delete(string connectionId, CancellationToken cancellationToken = default) =>
		Redirect($"/api/bank-connection/{connectionId}", connectionId, cancellationToken);

	[HttpGet("InvitedPartnerIds")]
	public IActionResult InvitedPartnerIds(CancellationToken cancellationToken = default) =>
		Redirect("/api/bank-connection/InvitedPartnerIds", cancellationToken);

	private IActionResult Redirect(string url, params object[] args)
	{
		Console.WriteLine(args.Length);
		return RedirectPermanentPreserveMethod(url);
	}
}
