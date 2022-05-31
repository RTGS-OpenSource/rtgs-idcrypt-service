using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.Proof;
using RTGS.IDCryptSDK.Proof.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptConnectionMessageHandlerTests.GivenStatusIsActive;

public class AndAgentNotAvailable
{
	private readonly string _message;
	private readonly IdCryptConnectionMessageHandler _handler;
	private readonly FakeLogger<IdCryptConnectionMessageHandler> _logger;

	public AndAgentNotAvailable()
	{
		var proofClientMock = new Mock<IProofClient>();

		proofClientMock
			.Setup(client => client.SendProofRequestAsync(
				It.IsAny<SendProofRequestRequest>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		_logger = new FakeLogger<IdCryptConnectionMessageHandler>();

		var activeConnection = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "active",
			TheirLabel = "RTGS_Bank_Agent"
		};

		_message = JsonSerializer.Serialize(activeConnection);

		_handler = new IdCryptConnectionMessageHandler(
			_logger,
			proofClientMock.Object,
			Mock.Of<IRtgsConnectionRepository>());
	}

	[Fact]
	public async Task WhenPosting_ThenThrow() =>
		await FluentActions
			.Awaiting(() => _handler.HandleAsync(_message, default))
			.Should()
			.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _handler.HandleAsync(_message, default))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(
			"Error occurred requesting proof");
	}
}
