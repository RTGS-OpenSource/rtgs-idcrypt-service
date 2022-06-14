﻿using System.Text.Json;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class ConnectionInvitationBasicMessageHandler : IBasicMessageHandler
{
	private readonly IConnectionService _connectionService;

	public ConnectionInvitationBasicMessageHandler(IConnectionService connectionService)
	{
		_connectionService = connectionService;
	}

	public string MessageType => nameof(ConnectionInvitation);

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default)
	{
		var request = JsonSerializer.Deserialize<BasicMessageContent<ConnectionInvitation>>(message);

		await _connectionService.AcceptInvitationAsync(request.MessageContent, cancellationToken);
	}
}
