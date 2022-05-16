using System.Text.Json;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class IdCryptConnectionMessageHandler : IMessageHandler
{
	private readonly ILogger<IdCryptConnectionMessageHandler> _logger;

	public IdCryptConnectionMessageHandler(
		ILogger<IdCryptConnectionMessageHandler> logger)
	{
		_logger = logger;
	}

	public string MessageType => "connection";

	public async Task HandleAsync(string jsonMessage)
	{
		var connection = JsonSerializer.Deserialize<IdCryptConnection>(jsonMessage);

		if (connection!.State is not "active")
		{
			_logger.LogDebug("Ignoring {RequestType} with alias {Alias} because state is {State}",
				MessageType, connection.Alias, connection.State);

			return;
		}

		//TODO: request proof here
	}
}
