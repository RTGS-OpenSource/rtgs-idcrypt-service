using System.Text.Json;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class RtgsConnectionInvitationBasicMessageHandler : IBasicMessageHandler
{
	private readonly IRtgsConnectionService _rtgsConnectionService;

	public RtgsConnectionInvitationBasicMessageHandler(IRtgsConnectionService rtgsConnectionService)
	{
		_rtgsConnectionService = rtgsConnectionService;
	}

	public string MessageType => nameof(RtgsConnectionInvitation);

	public bool RequiresActiveConnection => throw new NotImplementedException();

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default)
	{
		var request = JsonSerializer.Deserialize<BasicMessageContent<RtgsConnectionInvitation>>(message);

		await _rtgsConnectionService.AcceptInvitationAsync(request!.MessageContent, cancellationToken);
	}
}
