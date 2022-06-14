using System.Text.Json;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class DeleteConnectionBasicMessageHandler : IBasicMessageHandler
{
	private readonly IConnectionService _connectionService;

	public DeleteConnectionBasicMessageHandler(IConnectionService connectionService)
	{
		_connectionService = connectionService;
	}

	public string MessageType => nameof(DeleteBankPartnerConnectionBasicMessage);

	public async Task HandleAsync(string message, CancellationToken cancellationToken = default)
	{
		var request = JsonSerializer.Deserialize<BasicMessageContent<DeleteBankPartnerConnectionBasicMessage>>(message);

		await _connectionService.DeleteAsync(request.MessageContent.FromRtgsGlobalId, request.MessageContent.Alias, cancellationToken);
	}
}
