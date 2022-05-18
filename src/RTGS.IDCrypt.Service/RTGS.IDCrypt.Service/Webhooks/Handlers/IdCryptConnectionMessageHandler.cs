using System.Text.Json;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.Proof;
using RTGS.IDCryptSDK.Proof.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class IdCryptConnectionMessageHandler : IMessageHandler
{
	private readonly ILogger<IdCryptConnectionMessageHandler> _logger;
	private readonly IProofClient _proofClient;

	public IdCryptConnectionMessageHandler(
		ILogger<IdCryptConnectionMessageHandler> logger,
		IProofClient proofClient)
	{
		_logger = logger;
		_proofClient = proofClient;
	}

	public string MessageType => "connection";

	public async Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		var connection = JsonSerializer.Deserialize<IdCryptConnection>(jsonMessage);

		if (connection!.State is not "active")
		{
			_logger.LogDebug("Ignoring {RequestType} with alias {Alias} because state is {State}",
				MessageType, connection.Alias, connection.State);

			return;
		}

		var request = new SendProofRequestRequest
		{
			ConnectionId = connection.ConnectionId
		};

		await _proofClient.SendProofRequestAsync(request, cancellationToken);
	}
}
