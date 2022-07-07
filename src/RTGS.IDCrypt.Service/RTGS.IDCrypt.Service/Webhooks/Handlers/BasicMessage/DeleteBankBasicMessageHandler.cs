using System.Text.Json;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class DeleteBankBasicMessageHandler : IBasicMessageHandler
{
	private readonly IBankConnectionService _bankConnectionService;
	private readonly IRtgsConnectionService _rtgsConnectionService;
	private readonly IRtgsConnectionRepository _rtgsConnectionRepository;

	public DeleteBankBasicMessageHandler(
		IBankConnectionService bankConnectionService,
		IRtgsConnectionService rtgsConnectionService,
		IRtgsConnectionRepository rtgsConnectionRepository)
	{
		_bankConnectionService = bankConnectionService;
		_rtgsConnectionService = rtgsConnectionService;
		_rtgsConnectionRepository = rtgsConnectionRepository;
	}

	public string MessageType => nameof(DeleteBankRequest);

	public bool RequiresActiveConnection => false;

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default)
	{
		var rtgsConnections = await _rtgsConnectionRepository.FindAsync(connection => connection.ConnectionId == connectionId, cancellationToken);
		if (!rtgsConnections.Any())
		{
			throw new InvalidMessageSourceException("Message did not originate from RTGS.");
		}

		var request = JsonSerializer.Deserialize<BasicMessageContent<DeleteBankRequest>>(message);

		await _bankConnectionService.DeleteBankAsync(request.MessageContent.RtgsGlobalId, cancellationToken);
		await _rtgsConnectionService.DeleteBankAsync(request.MessageContent.RtgsGlobalId, cancellationToken);
	}
}
