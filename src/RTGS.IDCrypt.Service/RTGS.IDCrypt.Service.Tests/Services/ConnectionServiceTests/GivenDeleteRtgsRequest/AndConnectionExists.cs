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

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenDeleteRtgsRequest;

public class AndConnectionExists : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly ConnectionService _connectionService;
	private readonly Mock<IRtgsConnectionRepository> _rtgsConnectionRepositoryMock = new();
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

		_rtgsConnectionRepositoryMock
			.Setup(service => service.DeleteAsync(ConnectionId,
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var logger = new FakeLogger<ConnectionService>();

		_connectionService = new ConnectionService(
			_connectionsClientMock.Object,
			logger,
			Mock.Of<IBankPartnerConnectionRepository>(),
			_rtgsConnectionRepositoryMock.Object,
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>(),
			coreOptions,
			_basicMessageClientMock.Object);
	}

	public async Task InitializeAsync() =>
		await _connectionService.DeleteRtgsAsync(ConnectionId);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenCallDeleteOnAgent() => _connectionsClientMock.Verify();

	[Fact]
	public void WhenInvoked_ThenCallDeleteOnRepository() => _rtgsConnectionRepositoryMock.Verify();
}
