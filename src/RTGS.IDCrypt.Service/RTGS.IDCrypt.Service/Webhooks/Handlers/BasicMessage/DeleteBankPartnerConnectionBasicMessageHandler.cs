using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class DeleteBankPartnerConnectionBasicMessageHandler : IBasicMessageHandler
{
	private readonly IBankConnectionService _bankConnectionService;

	public DeleteBankPartnerConnectionBasicMessageHandler(IBankConnectionService bankConnectionService)
	{
		_bankConnectionService = bankConnectionService;
	}

	public string MessageType => nameof(DeleteBankPartnerConnectionBasicMessage);

	public bool RequiresActiveConnection => false;

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default) =>
		await _bankConnectionService.DeleteAsync(connectionId, false, cancellationToken);
}
