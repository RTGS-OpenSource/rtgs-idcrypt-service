using System.Text.Json;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.BasicMessage.Models;
using RTGSIDCryptWorker.Contracts;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class DeleteBankBasicMessageHandler : IBasicMessageHandler
{
	private readonly IBankConnectionService _bankConnectionService;

	public DeleteBankBasicMessageHandler(IBankConnectionService bankConnectionService)
	{
		_bankConnectionService = bankConnectionService;
	}

	public string MessageType => nameof(DeleteBankRequest);

	public bool RequiresActiveConnection => false;

	public async Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default)
	{
		var request = JsonSerializer.Deserialize<BasicMessageContent<DeleteBankRequest>>(message);

		// Is this the bank being deleted?

		// yes - retrieve All connections from rtgs AND bank partners
		// call delete on the agent for each connectionid
		// delete both tables

		// no - retrieve bank partner connections for the specified bank partner
		// call delete on the agent for each connectionid
		// delete the selected bank partner rows
	}
}
