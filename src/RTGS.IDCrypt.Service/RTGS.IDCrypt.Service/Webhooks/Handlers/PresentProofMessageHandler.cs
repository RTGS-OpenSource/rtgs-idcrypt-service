using System.Text.Json;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class PresentProofMessageHandler : IMessageHandler
{
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;

	private readonly ILogger<PresentProofMessageHandler> _logger;

	public PresentProofMessageHandler(
		IBankPartnerConnectionRepository bankPartnerConnectionRepository,
		ILogger<PresentProofMessageHandler> logger = null)
	{
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
		_logger = logger;
	}

	public string MessageType => "present_proof";

	public async Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		_logger?.LogInformation("Present proof webhook called with payload: {Payload}", jsonMessage);

		var proof = JsonSerializer.Deserialize<Proof>(jsonMessage);

		if (proof!.State != ProofStates.Received)
		{
			return;
		}

		await _bankPartnerConnectionRepository.ActivateAsync(proof!.ConnectionId, cancellationToken);
	}
}
