using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RTGS.IDCrypt.Service.Webhooks.Handlers;

namespace RTGS.IDCrypt.Service.Webhooks;

public class MessageHandlerResolver
{
	private readonly ILogger<MessageHandlerResolver> _logger;
	private readonly IEnumerable<IMessageHandler> _messageHandlers;

	public MessageHandlerResolver(
		ILogger<MessageHandlerResolver> logger,
		IEnumerable<IMessageHandler> idCryptMessageHandlers)
	{
		_logger = logger;
		_messageHandlers = idCryptMessageHandlers;
	}

	public async Task ResolveAsync(HttpContext context)
	{
		_logger.LogDebug("Handling request...");

		try
		{
			var messageType = context.GetRouteValue("route") as string;

			var handler = _messageHandlers.SingleOrDefault(messageHandler =>
				messageHandler.MessageType.Equals(messageType, StringComparison.OrdinalIgnoreCase));

			if (handler is null)
			{
				_logger.LogDebug("No message handler found for message type {MessageType}", messageType);

				return;
			}

			using var reader = new StreamReader(context.Request.Body);
			var message = await reader.ReadToEndAsync();

			await handler.HandleAsync(message);

			_logger.LogDebug("Finished handling request");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to handle request");
		}

		await context.Response.CompleteAsync();
	}
}
