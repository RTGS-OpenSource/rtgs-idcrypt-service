using System.Collections.Generic;
using System.Text.Json;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class IdCryptBasicMessageHandler : IMessageHandler
{
	private readonly ILogger<IdCryptBasicMessageHandler> _logger;
	private readonly IDictionary<string, IBasicMessageHandler> _handlers;

	public IdCryptBasicMessageHandler(
		ILogger<IdCryptBasicMessageHandler> logger,
		IEnumerable<IBasicMessageHandler> handlers)
	{
		_logger = logger;
		_handlers = handlers.ToDictionary(handler => handler.MessageType, handler => handler);
	}

	public string MessageType => "basicmessage";

	public async Task HandleAsync(string jsonMessage)
	{
		var message = JsonSerializer.Deserialize<IdCryptBasicMessage>(jsonMessage);

		_logger.LogInformation("Received {MessageType} BasicMessage.", message!.MessageType);

		if (_handlers.TryGetValue(message.MessageType, out var handler))
		{
			await handler.HandleAsync(message.Content);

			_logger.LogInformation("Handled {MessageType} BasicMessage.", message.MessageType);
		}
		else
		{
			_logger.LogDebug("No BasicMessage handler found for message type {MessageType}.", message.MessageType);
		}
	}
}
