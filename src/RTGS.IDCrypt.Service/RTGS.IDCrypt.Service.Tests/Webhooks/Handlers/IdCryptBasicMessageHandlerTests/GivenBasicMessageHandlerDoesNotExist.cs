using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptBasicMessageHandlerTests;

public class GivenBasicMessageHandlerDoesNotExist : IAsyncLifetime
{
	private FakeLogger<IdCryptBasicMessageHandler> _logger;
	private Mock<IBasicMessageHandler> _mockBasicMessageHandler;

	public async Task InitializeAsync()
	{
		_logger = new FakeLogger<IdCryptBasicMessageHandler>();

		var basicMessage = new IdCryptBasicMessage
		{
			ConnectionId = "connection-id",
			Content = JsonSerializer.Serialize(new BasicMessageContent<string>
			{
				MessageType = "invalid-message-type",
				MessageContent = "hello"
			})
		};

		_mockBasicMessageHandler = new Mock<IBasicMessageHandler>();
		_mockBasicMessageHandler.SetupGet(handler => handler.MessageType).Returns("message-type");

		var handler = new IdCryptBasicMessageHandler(_logger, new[] { _mockBasicMessageHandler.Object });

		var message = JsonSerializer.Serialize(basicMessage);

		await handler.HandleAsync(message, default);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenLogsExpected() =>
		_logger.Logs[LogLevel.Information].Should().BeEquivalentTo(new[]
		{
			"Received invalid-message-type BasicMessage from ConnectionId connection-id",
			"No BasicMessage handler found for message type invalid-message-type"
		}, options => options.WithStrictOrdering());

	[Fact]
	public void ThenDoesNotCallHandleAsync() => _mockBasicMessageHandler
		.Verify(handler => handler.HandleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
}
