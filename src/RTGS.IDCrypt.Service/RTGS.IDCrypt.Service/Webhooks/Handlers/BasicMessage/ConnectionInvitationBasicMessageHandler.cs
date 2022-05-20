using System.Text.Json;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class ConnectionInvitationBasicMessageHandler : IBasicMessageHandler
{
	private readonly IConnectionService _connectionService;

	public ConnectionInvitationBasicMessageHandler(IConnectionService connectionService)
	{
		_connectionService = connectionService;
	}

	public string MessageType => nameof(ConnectionInvitation);

	public async Task HandleAsync(string message, CancellationToken cancellationToken = default)
	{
		var request = JsonSerializer.Deserialize<ConnectionInvitation>(message);

		await _connectionService.AcceptInvitationAsync(request, cancellationToken);
	}
}
