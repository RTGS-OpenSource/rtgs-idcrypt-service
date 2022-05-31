using System.Text.Json;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class PresentProofMessageHandler : IMessageHandler
{
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;

	public PresentProofMessageHandler(IBankPartnerConnectionRepository bankPartnerConnectionRepository)
	{
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
	}

	public string MessageType => "present_proof";

	public async Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		var proof = JsonSerializer.Deserialize<Proof>(jsonMessage);

		await _bankPartnerConnectionRepository.ActivateAsync(proof.ConnectionId, cancellationToken);
	}
}
