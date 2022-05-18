using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenCreateInvitationRequest;

public class AndIdCryptApiUnavailable
{
	private readonly ConnectionService _connectionService;
	private readonly string _alias = "alias";
	private readonly FakeLogger<ConnectionService> _logger;

	public AndIdCryptApiUnavailable()
	{
		var connectionsClientMock = new Mock<IConnectionsClient>();

		connectionsClientMock
			.Setup(client => client.CreateConnectionInvitationAsync(
				_alias,
				It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>()
			.Verifiable();

		_logger = new FakeLogger<ConnectionService>();

		var aliasProviderMock = new Mock<IAliasProvider>();
		aliasProviderMock.Setup(provider => provider.Provide()).Returns(_alias);

		_connectionService = new ConnectionService(
			connectionsClientMock.Object,
			_logger,
			Mock.Of<IConnectionRepository>(),
			aliasProviderMock.Object,
			Mock.Of<IWalletClient>()
		);
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _connectionService.CreateConnectionInvitationAsync())
			.Should()
			.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionService.CreateConnectionInvitationAsync())
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error occurred when sending CreateConnectionInvitation request with alias alias to ID Crypt Cloud Agent");
	}
}
