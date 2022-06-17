using System.Text.Json;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class BankConnectionInvitationBasicMessageHandler : IBasicMessageHandler
{
	private readonly IConnectionService _connectionService;

	public BankConnectionInvitationBasicMessageHandler(IConnectionService connectionService)
	{
		_connectionService = connectionService;
	}

	public string MessageType => nameof(BankConnectionInvitation);

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default)
	{
		var request = JsonSerializer.Deserialize<BasicMessageContent<BankConnectionInvitation>>(message);

		await _connectionService.AcceptBankInvitationAsync(request!.MessageContent, cancellationToken);
	}
}
