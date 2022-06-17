using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.BasicMessage;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenDeleteRequest.WithoutNotifyingPartner;

public class AndConnectionExists : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly BankConnectionService _bankConnectionService;
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock = new();
	private const string ConnectionId = "connection-id";
	private readonly Mock<IBasicMessageClient> _basicMessageClientMock = new();

	public AndConnectionExists()
	{
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		});

		_connectionsClientMock
			.Setup(client => client.DeleteConnectionAsync(ConnectionId, It.IsAny<CancellationToken>()))
			.Verifiable();

		_bankPartnerConnectionRepositoryMock
			.Setup(service => service.DeleteAsync(ConnectionId,
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var logger = new FakeLogger<BankConnectionService>();

		_bankConnectionService = new BankConnectionService(
			_connectionsClientMock.Object,
			logger,
			_bankPartnerConnectionRepositoryMock.Object,
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>(),
			coreOptions,
			_basicMessageClientMock.Object);
	}

	public async Task InitializeAsync() =>
		await _bankConnectionService.DeleteAsync(ConnectionId, false);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenCallDeleteOnAgent() => _connectionsClientMock.Verify();

	[Fact]
	public void WhenInvoked_ThenCallDeleteOnRepository() => _bankPartnerConnectionRepositoryMock.Verify();

	[Fact]
	public void WhenInvoked_ThenDoesNotNotifyPartner() =>
		_basicMessageClientMock.Verify(client
			=> client.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(),
				It.IsAny<CancellationToken>()), Times.Never);
}
