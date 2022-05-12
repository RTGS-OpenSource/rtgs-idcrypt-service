﻿using System.Text.Json;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class IdCryptConnectionMessageHandler : IIdCryptMessageHandler
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
			_logger.LogDebug("Ignoring {RequestType} for {Alias} because state is {State} and not active",
				MessageType, connection.Alias, connection.State);

			return;
		}

		// request proof here
	}
}
