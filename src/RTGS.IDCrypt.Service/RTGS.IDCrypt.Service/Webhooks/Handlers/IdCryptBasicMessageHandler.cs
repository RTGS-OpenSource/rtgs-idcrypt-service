using System.Collections.Generic;
using System.Text.Json;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class IdCryptBasicMessageHandler : IMessageHandler
{
	private readonly ILogger<IdCryptBasicMessageHandler> _logger;
	private readonly IRtgsConnectionRepository _rtgsConnectionRepository;
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly IDictionary<string, IBasicMessageHandler> _handlers;

	public IdCryptBasicMessageHandler(
		ILogger<IdCryptBasicMessageHandler> logger,
		IEnumerable<IBasicMessageHandler> handlers,
		IRtgsConnectionRepository rtgsConnectionRepository,
		IBankPartnerConnectionRepository bankPartnerConnectionRepository)
	{
		_logger = logger;
		_rtgsConnectionRepository = rtgsConnectionRepository;
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
		_handlers = handlers.ToDictionary(handler => handler.MessageType, handler => handler);
	}

	public string MessageType => "basicmessages";

	public async Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		var message = JsonSerializer.Deserialize<IdCryptBasicMessage>(jsonMessage);
		var messageContent = JsonSerializer.Deserialize<BasicMessageContent>(message!.Content);

		_logger.LogInformation("Received {MessageType} BasicMessage from ConnectionId {ConnectionId}", messageContent!.MessageType, message.ConnectionId);

		if (!_handlers.TryGetValue(messageContent.MessageType, out var handler))
		{
			_logger.LogInformation("No BasicMessage handler found for message type {MessageType}", messageContent.MessageType);

			return;
		}

		if (!handler.RequiresActiveConnection)
		{
			await HandleMessage(message, messageContent, handler, cancellationToken);

			return;
		}

		if (messageContent.Source.StartsWith("Bank"))
		{
			var rtgsGlobalId = messageContent.Source[5..];

			var bankPartnerConnection = await _bankPartnerConnectionRepository.GetAsync(rtgsGlobalId, message.ConnectionId, cancellationToken);

			if (bankPartnerConnection.Status == ConnectionStatuses.Active)
			{
				await HandleMessage(message, messageContent, handler, cancellationToken);

				return;
			}
		}

		if (messageContent.Source == "RTGS")
		{
			var rtgsConnection = await _rtgsConnectionRepository.GetAsync(message.ConnectionId, cancellationToken);

			if (rtgsConnection.Status == ConnectionStatuses.Active)
			{
				await HandleMessage(message, messageContent, handler, cancellationToken);

				return;
			}
		}

		_logger.LogInformation("Message not handled because connection {ConnectionId} is not active", message.ConnectionId);
	}

	private async Task HandleMessage(IdCryptBasicMessage message, BasicMessageContent messageContent, IBasicMessageHandler handler, CancellationToken cancellationToken)
	{
		await handler.HandleAsync(message.Content, message.ConnectionId, cancellationToken);

		_logger.LogInformation("Handled {MessageType} BasicMessage", messageContent.MessageType);
	}
}
