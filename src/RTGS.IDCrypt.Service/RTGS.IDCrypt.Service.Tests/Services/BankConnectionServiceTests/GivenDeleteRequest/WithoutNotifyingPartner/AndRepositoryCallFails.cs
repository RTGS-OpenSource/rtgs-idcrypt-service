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

namespace RTGS.IDCrypt.Service.Tests.Services.BankConnectionServiceTests.GivenDeleteRequest.WithoutNotifyingPartner;

public class AndRepositoryCallFails
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly BankConnectionService _bankConnectionService;
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock = new();
	private const string ConnectionId = "connection-id";
	private readonly FakeLogger<BankConnectionService> _logger = new();

	public AndRepositoryCallFails()
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
			.ThrowsAsync(new Exception("Something went wrong"))
			.Verifiable();

		_bankConnectionService = new BankConnectionService(
			_connectionsClientMock.Object,
			_logger,
			_bankPartnerConnectionRepositoryMock.Object,
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
			.Awaiting(() => _bankConnectionService.DeleteAsync(ConnectionId, false))
			.Should()
			.ThrowAsync<AggregateException>()
			.WithMessage("One or more errors occurred. (Something went wrong)");

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error occurred when deleting connection");
	}
}
