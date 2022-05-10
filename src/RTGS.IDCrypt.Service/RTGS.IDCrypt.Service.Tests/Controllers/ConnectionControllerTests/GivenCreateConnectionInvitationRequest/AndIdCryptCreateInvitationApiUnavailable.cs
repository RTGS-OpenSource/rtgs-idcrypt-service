using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndIdCryptCreateInvitationApiUnavailable
{
	private readonly FakeLogger<ConnectionController> _logger;
	private readonly Mock<IConnectionsClient> _connectionsClientMock;
	private readonly Mock<IWalletClient> _walletClientMock;
	private readonly Mock<IAliasProvider> _mockAliasProvider;
	private readonly ConnectionController _connectionController;
	private const string Alias = "alias";

	public AndIdCryptCreateInvitationApiUnavailable()
	{
		_connectionsClientMock = new Mock<IConnectionsClient>();

		_connectionsClientMock
			.Setup(connectionsClient => connectionsClient.CreateInvitationAsync(
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<bool>(),
				It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_walletClientMock = new Mock<IWalletClient>();

		_mockAliasProvider = new Mock<IAliasProvider>();

		_mockAliasProvider
			.Setup(provider => provider.Provide())
			.Returns(Alias);

		_logger = new FakeLogger<ConnectionController>();

		_connectionController = new ConnectionController(
			_logger,
			_connectionsClientMock.Object,
			_walletClientMock.Object,
			_mockAliasProvider.Object);
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionController.Post(default))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
			{
				$"Error occurred when sending CreateInvitation request with alias {Alias} to ID Crypt Cloud Agent"
			});
	}

	[Fact]
	public async Task WhenPosting_ThenGetPublicDidIsNotCalled()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionController.Post(default))
			.Should()
			.ThrowAsync<Exception>();

		_walletClientMock.Verify(walletClient =>
			walletClient.GetPublicDidAsync(default), Times.Never());
	}
}
