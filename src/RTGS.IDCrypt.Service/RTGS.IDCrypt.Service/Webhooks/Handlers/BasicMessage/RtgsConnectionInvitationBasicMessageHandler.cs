using System.Text.Json;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class RtgsConnectionInvitationBasicMessageHandler : IBasicMessageHandler
{
	private readonly IConnectionService _connectionService;

	public RtgsConnectionInvitationBasicMessageHandler(IConnectionService connectionService)
	{
		_connectionService = connectionService;
	}

	public string MessageType => nameof(RtgsConnectionInvitation);

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default)
	{
		var request = JsonSerializer.Deserialize<BasicMessageContent<RtgsConnectionInvitation>>(message);

		await _connectionService.AcceptRtgsInvitationAsync(request.MessageContent, cancellationToken);
	}
}
