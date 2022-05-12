using System.Text.Json;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class IdCryptConnectionMessageHandler : IIdCryptMessageHandler
{
	private readonly ILogger<IdCryptConnectionMessageHandler> _logger;

	public IdCryptConnectionMessageHandler(ILogger<IdCryptConnectionMessageHandler> logger)
	{
		_logger = logger;
	}

	public string MessageType => "connection";

	public void Handle(string jsonMessage)
	{
		var message = JsonSerializer.Deserialize<IdCryptConnection>(jsonMessage);

		if (message!.State is not "active")
		{
			_logger.LogDebug("Ignoring {RequestType} for {Alias} because state '{State}' is not 'active'",
				MessageType, message.Alias, message.State);

			return;
		}
	}
}
