using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.RtgsConnectionServiceTests.GivenDeleteBankRequest;

public class AndIsNotLocalBank : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly Mock<IRtgsConnectionRepository> _mockRtgsConnectionRepository = new();
	
	private readonly RtgsConnectionService _rtgsConnectionService;

	private const string RtgsGlobalId = "rtgs-global-id-1";

	public AndIsNotLocalBank()
	{
		var logger = new FakeLogger<RtgsConnectionService>();
		
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = RtgsGlobalId
		});

		_mockRtgsConnectionRepository
			.Setup(repo => repo.GetMatchingAsync(default, It.IsAny<CancellationToken>()))
			.ReturnsAsync(Array.Empty<RtgsConnection>());

		_rtgsConnectionService = new RtgsConnectionService(
			_connectionsClientMock.Object,
			logger,
			_mockRtgsConnectionRepository.Object,
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>(),
			coreOptions);
	}
	
	public async Task InitializeAsync() =>
		await _rtgsConnectionService.DeleteBankAsync(RtgsGlobalId, default);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenDeleteIsNotCalled() => _mockRtgsConnectionRepository.Verify(repo => repo.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

	[Fact]
	public void ThenCallsDeleteOnAgent() => _connectionsClientMock.Verify(conn=>conn.DeleteConnectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
}
