using System.Collections.Generic;
using System.Text.Json;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage.Models;

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

	public string MessageType => "basicmessages";

	public async Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		var message = JsonSerializer.Deserialize<IdCryptBasicMessage>(jsonMessage);
		var messageContent = JsonSerializer.Deserialize<BasicMessageContent>(message!.Content);

		_logger.LogInformation("Received {MessageType} BasicMessage from ConnectionId {ConnectionId}.", messageContent!.MessageType, message.ConnectionId);

		if (_handlers.TryGetValue(messageContent.MessageType, out var handler))
		{
			await handler.HandleAsync(message.Content, message.ConnectionId, cancellationToken);

			_logger.LogInformation("Handled {MessageType} BasicMessage.", messageContent.MessageType);
		}
		else
		{
			_logger.LogInformation("No BasicMessage handler found for message type {MessageType}.", messageContent.MessageType);
		}
	}
}
