using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.BasicMessage;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenDeleteRequest.WithRtgsGlobalIdAndAlias;

public class AndConnectionExists : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly ConnectionService _connectionService;
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock = new();
	private const string ConnectionId = "connection-id";
	private const string RtgsGlobalId = "rtgs-global-id";
	private const string Alias = "alias";

	public AndConnectionExists()
	{
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		});

		var bankPartnerConnection = new BankPartnerConnection { ConnectionId = ConnectionId };

		_bankPartnerConnectionRepositoryMock
			.Setup(service
				=> service.GetAsync(RtgsGlobalId, Alias, It.IsAny<CancellationToken>()))
			.ReturnsAsync(bankPartnerConnection)
			.Verifiable();

		_connectionsClientMock
			.Setup(client => client.DeleteConnectionAsync(ConnectionId, It.IsAny<CancellationToken>()))
			.Verifiable();

		_bankPartnerConnectionRepositoryMock
			.Setup(service => service.DeleteAsync(bankPartnerConnection,
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var logger = new FakeLogger<ConnectionService>();

		_connectionService = new ConnectionService(
			_connectionsClientMock.Object,
			logger,
			_bankPartnerConnectionRepositoryMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>(),
			coreOptions,
			Mock.Of<IBasicMessageClient>());
	}

	public async Task InitializeAsync() =>
		await _connectionService.DeleteAsync(RtgsGlobalId, Alias);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenCallDeleteOnAgent() => _connectionsClientMock.Verify();

	[Fact]
	public void WhenInvoked_ThenCallGetAndDeleteOnRepository() => _bankPartnerConnectionRepositoryMock.Verify();
}
