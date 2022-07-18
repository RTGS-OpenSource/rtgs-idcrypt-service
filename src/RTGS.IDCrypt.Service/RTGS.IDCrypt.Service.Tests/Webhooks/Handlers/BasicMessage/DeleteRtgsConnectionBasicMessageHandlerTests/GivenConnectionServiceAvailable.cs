using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Contracts.BasicMessage;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.DeleteRtgsConnectionBasicMessageHandlerTests;

public class GivenConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IRtgsConnectionService> _rtgsConnectionServiceMock = new();

	public async Task InitializeAsync()
	{
		const string connectionId = "connection-id";

		var basicMessage = new BasicMessageContent<DeleteRtgsConnectionBasicMessage>
		{
			MessageType = nameof(DeleteRtgsConnectionBasicMessage),
			MessageContent = new DeleteRtgsConnectionBasicMessage()
		};

		_rtgsConnectionServiceMock
			.Setup(service => service.DeleteAsync(connectionId, It.IsAny<CancellationToken>()))
			.Verifiable();

		var handler = new DeleteRtgsConnectionBasicMessageHandler(_rtgsConnectionServiceMock.Object);

		var message = JsonSerializer.Serialize(basicMessage);

		await handler.HandleAsync(message, connectionId);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenCallsDelete() => _rtgsConnectionServiceMock.Verify();
}
