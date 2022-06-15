using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class DeleteRtgsConnectionBasicMessageHandler : IBasicMessageHandler
{
	private readonly IConnectionService _connectionService;

	public DeleteRtgsConnectionBasicMessageHandler(IConnectionService connectionService)
	{
		_connectionService = connectionService;
	}

	public string MessageType => nameof(DeleteRtgsConnectionBasicMessage);

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default) =>
		await _connectionService.DeleteRtgsAsync(connectionId, cancellationToken);
}
