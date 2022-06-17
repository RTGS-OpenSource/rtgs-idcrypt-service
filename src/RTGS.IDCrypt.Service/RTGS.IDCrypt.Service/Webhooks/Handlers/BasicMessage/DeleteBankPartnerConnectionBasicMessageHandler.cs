using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class DeleteBankPartnerConnectionBasicMessageHandler : IBasicMessageHandler
{
	private readonly IConnectionService _connectionService;

	public DeleteBankPartnerConnectionBasicMessageHandler(IConnectionService connectionService)
	{
		_connectionService = connectionService;
	}

	public string MessageType => nameof(DeleteBankPartnerConnectionBasicMessage);

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default) =>
		await _connectionService.DeletePartnerAsync(connectionId, false, cancellationToken);
}
