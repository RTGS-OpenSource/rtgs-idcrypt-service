using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class DeleteRtgsConnectionBasicMessageHandler : IBasicMessageHandler
{
	private readonly IRtgsConnectionService _rtgsConnectionService;

	public DeleteRtgsConnectionBasicMessageHandler(IRtgsConnectionService rtgsConnectionService)
	{
		_rtgsConnectionService = rtgsConnectionService;
	}

	public string MessageType => nameof(DeleteRtgsConnectionBasicMessage);

	public bool RequiresActiveConnection => throw new NotImplementedException();

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default) =>
		await _rtgsConnectionService.DeleteAsync(connectionId, cancellationToken);
}
