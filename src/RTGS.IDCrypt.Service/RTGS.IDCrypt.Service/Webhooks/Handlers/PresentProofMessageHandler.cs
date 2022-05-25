using System.Text.Json;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class PresentProofMessageHandler : IMessageHandler
{
	private readonly IConnectionRepository _connectionRepository;

	public PresentProofMessageHandler(IConnectionRepository connectionRepository)
	{
		_connectionRepository = connectionRepository;
	}

	public string MessageType => throw new NotImplementedException();

	public Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		var proof = JsonSerializer.Deserialize<Proof>(jsonMessage);

		_connectionRepository.ActivateBankPartnerConnectionAsync(proof.ConnectionId);

		return Task.CompletedTask;
	}
}
