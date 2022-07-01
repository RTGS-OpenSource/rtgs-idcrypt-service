using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptBasicMessageHandlerTests.GivenBasicMessageFromRtgs.AndHandlerExists.RequiringActiveConnection;

public class AndConnectionIsActive : IAsyncLifetime
{
	private FakeLogger<IdCryptBasicMessageHandler> _logger;
	private IdCryptBasicMessage _receivedBasicMessage;
	private Mock<IBasicMessageHandler> _mockBasicMessageHandler;

	public async Task InitializeAsync()
	{
		_logger = new FakeLogger<IdCryptBasicMessageHandler>();

		_receivedBasicMessage = new IdCryptBasicMessage
		{
			ConnectionId = "connection-id",
			Content = JsonSerializer.Serialize(new BasicMessageContent<string>
			{
				MessageType = "message-type",
				MessageContent = "hello",
				Source = "RTGS"
			})
		};

		_mockBasicMessageHandler = new Mock<IBasicMessageHandler>();
		_mockBasicMessageHandler.SetupGet(handler => handler.MessageType).Returns("message-type");
		_mockBasicMessageHandler.SetupGet(handler => handler.RequiresActiveConnection).Returns(true);

		var _mockRtgsConnectionRepository = new Mock<IRtgsConnectionRepository>();
		_mockRtgsConnectionRepository.Setup(repository =>
				repository.GetAsync("connection-id", It.IsAny<CancellationToken>()))
			.ReturnsAsync(new RtgsConnection
			{
				ConnectionId = "connection-id",
				Status = ConnectionStatuses.Active
			});

		var handler = new IdCryptBasicMessageHandler(
			_logger,
			new[] { _mockBasicMessageHandler.Object },
			_mockRtgsConnectionRepository.Object,
			Mock.Of<IBankPartnerConnectionRepository>());

		var message = JsonSerializer.Serialize(_receivedBasicMessage);

		await handler.HandleAsync(message, default);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenLogsExpected() =>
		_logger.Logs[LogLevel.Information].Should().BeEquivalentTo(new[]
		{
			"Received message-type BasicMessage from ConnectionId connection-id",
			"Handled message-type BasicMessage"
		}, options => options.WithStrictOrdering());

	[Fact]
	public void ThenHandlerIsInvoked() =>
		_mockBasicMessageHandler.Verify(handler => handler.HandleAsync(
				_receivedBasicMessage.Content,
				_receivedBasicMessage.ConnectionId,
				It.IsAny<CancellationToken>()),
			Times.Once);
}
