using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.DeleteConnectionBasicMessageHandlerTests;

public class GivenConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionService> _connectionServiceMock = new();

	public async Task InitializeAsync()
	{
		var basicMessage = new BasicMessageContent<DeleteBankPartnerConnectionBasicMessage>
		{
			MessageType = nameof(DeleteConnectionBasicMessageHandler),
			MessageContent = new DeleteBankPartnerConnectionBasicMessage
			{
				FromRtgsGlobalId = "from-rtgs-global-id",
				Alias = "alias"
			}
		};

		_connectionServiceMock
			.Setup(service => service.DeleteAsync(
				It.Is<string>(rtgsGlobalId => rtgsGlobalId == basicMessage.MessageContent.FromRtgsGlobalId),
				It.Is<string>(alias => alias == basicMessage.MessageContent.Alias), It.IsAny<CancellationToken>()))
			.Verifiable();

		var handler = new DeleteConnectionBasicMessageHandler(_connectionServiceMock.Object);

		var message = JsonSerializer.Serialize(basicMessage);

		await handler.HandleAsync(message);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenCallsAcceptInvitationAsync() => _connectionServiceMock.Verify();
}
