using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class DeleteBankPartnerConnectionBasicMessageHandler : IBasicMessageHandler
{
	private readonly IBankConnectionService _connectionService;

	public DeleteBankPartnerConnectionBasicMessageHandler(IBankConnectionService connectionService)
	{
		_connectionService = connectionService;
	}

	public string MessageType => nameof(DeleteBankPartnerConnectionBasicMessage);

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default) =>
		await _connectionService.DeleteAsync(connectionId, false, cancellationToken);
}
