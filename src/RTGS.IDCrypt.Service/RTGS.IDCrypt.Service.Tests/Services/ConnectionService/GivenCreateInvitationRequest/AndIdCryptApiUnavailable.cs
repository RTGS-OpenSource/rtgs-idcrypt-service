using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionService.GivenCreateInvitationRequest;

public class AndIdCryptApiUnavailable
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock;
	private readonly Service.Services.ConnectionService _connectionService;
	private readonly string _alias = "alias";
	private readonly FakeLogger<Service.Services.ConnectionService> _logger;

	public AndIdCryptApiUnavailable()
	{
		_connectionsClientMock = new Mock<IConnectionsClient>();

		_connectionsClientMock
			.Setup(client => client.CreateInvitationAsync(
				_alias,
				It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>()
			.Verifiable();

		_logger = new FakeLogger<Service.Services.ConnectionService>();

		_connectionService = new Service.Services.ConnectionService(
			_connectionsClientMock.Object,
			_logger);
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _connectionService.CreateInvitationAsync(_alias))
			.Should()
			.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionService.CreateInvitationAsync(_alias))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error occurred when sending CreateInvitation request with alias alias to ID Crypt Cloud Agent");
	}
}
