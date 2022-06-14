using Microsoft.Extensions.Logging;
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

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenDeleteRequest.WithRtgsGlobalIdAndAlias;

public class AndRepositoryGetCallFails
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly ConnectionService _connectionService;
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock = new();
	private readonly FakeLogger<ConnectionService> _logger = new();
	private const string RtgsGlobalId = "rtgs-global-id";
	private const string Alias = "alias";

	public AndRepositoryGetCallFails()
	{
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		});

		_bankPartnerConnectionRepositoryMock
			.Setup(service
				=> service.GetAsync(RtgsGlobalId, Alias, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Something went wrong"))
			.Verifiable();

		_connectionService = new ConnectionService(
			_connectionsClientMock.Object,
			_logger,
			_bankPartnerConnectionRepositoryMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>(),
			coreOptions,
			Mock.Of<IBasicMessageClient>());
	}

	[Fact]
	public async Task WhenInvoked_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionService.DeleteAsync(RtgsGlobalId, Alias))
			.Should()
			.ThrowAsync<Exception>()
			.WithMessage("Something went wrong");

		_logger.Logs[LogLevel.Error].Should()
			.BeEquivalentTo("Error occurred when getting connection with RtgsGlobalId rtgs-global-id and Alias alias.");
	}
}
