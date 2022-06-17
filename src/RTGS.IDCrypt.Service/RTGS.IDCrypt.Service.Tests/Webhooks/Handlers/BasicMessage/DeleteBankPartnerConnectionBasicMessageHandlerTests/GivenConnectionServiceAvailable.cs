using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.DeleteBankPartnerConnectionBasicMessageHandlerTests;

public class GivenConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionService> _connectionServiceMock = new();

	public async Task InitializeAsync()
	{
		const string connectionId = "connection-id";

		var basicMessage = new BasicMessageContent<DeleteBankPartnerConnectionBasicMessage>
		{
			MessageType = nameof(DeleteBankPartnerConnectionBasicMessageHandler),
			MessageContent = new DeleteBankPartnerConnectionBasicMessage()
		};

		_connectionServiceMock
			.Setup(service => service.DeletePartnerAsync(connectionId, false, It.IsAny<CancellationToken>()))
			.Verifiable();

		var handler = new DeleteBankPartnerConnectionBasicMessageHandler(_connectionServiceMock.Object);

		var message = JsonSerializer.Serialize(basicMessage);

		await handler.HandleAsync(message, connectionId);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenCallsDeleteAsync() => _connectionServiceMock.Verify();
}
