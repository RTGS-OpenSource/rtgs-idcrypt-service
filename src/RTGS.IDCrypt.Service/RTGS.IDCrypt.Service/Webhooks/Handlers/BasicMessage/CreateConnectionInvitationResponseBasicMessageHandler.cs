using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class CreateConnectionInvitationResponseBasicMessageHandler : IBasicMessageHandler
{
	private readonly IConnectionService _connectionService;
	private readonly IConnectionStorageService _connectionStorageService;

	public CreateConnectionInvitationResponseBasicMessageHandler(IConnectionService connectionService, IConnectionStorageService connectionStorageService)
	{
		_connectionService = connectionService;
		_connectionStorageService = connectionStorageService;
	}

	public string ForType => nameof(CreateConnectionInvitationResponse);
	public async Task HandleAsync(string message, CancellationToken cancellationToken = default)
	{
		var request = JsonSerializer.Deserialize<CreateConnectionInvitationResponse>(message);

		var receiveAndAcceptInvitationRequest = new ReceiveAndAcceptInvitationRequest
		{
			Alias = request.Alias,
			Id = request.Invitation.Id,
			Label = request.Invitation.Label,
			RecipientKeys = request.Invitation.RecipientKeys,
			ServiceEndpoint = request.Invitation.ServiceEndpoint,
			Type = request.Invitation.Type
		};

		var response = await _connectionService.AcceptInvitationAsync(receiveAndAcceptInvitationRequest, cancellationToken);

		var pendingConnection = new PendingBankPartnerConnection
		{
			PartitionKey = response.ConnectionId,
			RowKey = response.Alias,
			ConnectionId = response.ConnectionId,
			Alias = response.Alias
		};

		await _connectionStorageService.SavePendingBankPartnerConnectionAsync(pendingConnection, cancellationToken);
	}
}
