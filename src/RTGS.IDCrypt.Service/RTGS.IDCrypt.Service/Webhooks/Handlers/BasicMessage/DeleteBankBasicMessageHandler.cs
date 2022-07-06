using System.Text.Json;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.BasicMessage.Models;
using RTGSIDCryptWorker.Contracts;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class DeleteBankBasicMessageHandler : IBasicMessageHandler
{
	private readonly IBankConnectionService _bankConnectionService;
	private readonly IRtgsConnectionService _rtgsConnectionService;

	public DeleteBankBasicMessageHandler(
		IBankConnectionService bankConnectionService,
		IRtgsConnectionService rtgsConnectionService)
	{
		_bankConnectionService = bankConnectionService;
		_rtgsConnectionService = rtgsConnectionService;
	}

	public string MessageType => nameof(DeleteBankRequest);

	public bool RequiresActiveConnection => false;

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default)
	{
		var request = JsonSerializer.Deserialize<BasicMessageContent<DeleteBankRequest>>(message);
		if (request.Source != "RTGS")
		{
			throw new InvalidMessageSourceException($"Source {request.Source} is not valid.");
		}

		await _bankConnectionService.DeleteBankAsync(request.MessageContent.RtgsGlobalId, cancellationToken);
		await _rtgsConnectionService.DeleteBankAsync(request.MessageContent.RtgsGlobalId, cancellationToken);
	}
}
