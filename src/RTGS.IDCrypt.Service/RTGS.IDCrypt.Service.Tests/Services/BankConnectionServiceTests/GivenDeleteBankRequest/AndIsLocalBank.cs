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

namespace RTGS.IDCrypt.Service.Tests.Services.BankConnectionServiceTests.GivenDeleteBankRequest;

public class AndIsLocalBank : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly Mock<IBasicMessageClient> _basicMessageClientMock = new();
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock = new();

	private readonly BankConnectionService _bankConnectionService;

	private const string RtgsGlobalId = "rtgs-global-id-1";
	private const string ConnectionId = "connection-id-123";

	public AndIsLocalBank()
	{
		var logger = new FakeLogger<BankConnectionService>();

		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = RtgsGlobalId
		});

		_bankPartnerConnectionRepositoryMock
			.Setup(repo => repo.GetMatchingAsync(default, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new[]
			{
				new BankPartnerConnection
				{
					PartitionKey = RtgsGlobalId,
					ConnectionId = ConnectionId
				}
			});

		_bankPartnerConnectionRepositoryMock
			.Setup(repo => repo.DeleteAsync(ConnectionId, It.IsAny<CancellationToken>()))
			.Verifiable();

		_connectionsClientMock
			.Setup(conn => conn.DeleteConnectionAsync(ConnectionId, It.IsAny<CancellationToken>()))
			.Verifiable();

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
		await _bankConnectionService.DeleteBankAsync(RtgsGlobalId, default);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenDeleteIsCalled() => _bankPartnerConnectionRepositoryMock.Verify();

	[Fact]
	public void ThenCallsDeleteOnAgent() => _connectionsClientMock.Verify();
}
