using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndIdCryptCreateInvitationApiUnavailable
{
	private readonly FakeLogger<ConnectionController> _logger;
	private readonly Mock<IWalletClient> _walletClientMock;
	private readonly ConnectionController _connectionController;
	private const string Alias = "alias";

	public AndIdCryptCreateInvitationApiUnavailable()
	{
		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(connectionsClient => connectionsClient.CreateInvitationAsync(
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_walletClientMock = new Mock<IWalletClient>();

		var mockAliasProvider = new Mock<IAliasProvider>();

		mockAliasProvider
			.Setup(provider => provider.Provide())
			.Returns(Alias);

		_logger = new FakeLogger<ConnectionController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_connectionController = new ConnectionController(
			_logger,
			_walletClientMock.Object,
			mockAliasProvider.Object,
			connectionServiceMock.Object,
			Mock.Of<IConnectionStorageService>());
	}

	[Fact]
	public async Task WhenPosting_ThenThrows()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionController.Post(default))
			.Should()
			.ThrowAsync<Exception>();
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
