using Microsoft.Extensions.Logging;
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

namespace RTGS.IDCrypt.Service.Tests.Services.BankConnectionServiceTests.GivenCycleRequest;

public class AndTableStorageCreateAsyncThrows
{
	private readonly Mock<IBasicMessageClient> _basicMessageClientMock = new();
	private readonly BankConnectionService _bankConnectionService;

	private readonly FakeLogger<BankConnectionService> _logger = new();

	public AndTableStorageCreateAsyncThrows()
	{
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		});

		const string partnerRtgsGlobalId = "partner-rtgs-global-id";

		var establishedBankConnection = new BankPartnerConnection
		{
			PartitionKey = partnerRtgsGlobalId,
			RowKey = "established-alias",
			Alias = "established-alias",
			ConnectionId = "established-connection-id",
		};

		var bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();
		bankPartnerConnectionRepositoryMock.Setup(repo => repo.GetEstablishedAsync(
				partnerRtgsGlobalId,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(establishedBankConnection);

		bankPartnerConnectionRepositoryMock
			.Setup(repo => repo.CreateAsync(
				It.IsAny<BankPartnerConnection>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		var aliasProviderMock = new Mock<IAliasProvider>();
		aliasProviderMock.Setup(provider => provider.Provide()).Returns("alias");

		_bankConnectionService = new BankConnectionService(
			Mock.Of<IConnectionsClient>(),
			_logger,
			bankPartnerConnectionRepositoryMock.Object,
			aliasProviderMock.Object,
			Mock.Of<IWalletClient>(),
			coreOptions,
			_basicMessageClientMock.Object
		);
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _bankConnectionService.CycleAsync("partner-rtgs-global-id"))
			.Should()
			.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _bankConnectionService.CycleAsync("partner-rtgs-global-id"))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error occurred when cycling connection for bank partner-rtgs-global-id");
	}

	[Fact]
	public void WhenInvoked_ThenDoNotSendBasicMessage() =>
		_basicMessageClientMock.Verify(client =>
			client.SendAsync(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<CancellationToken>()),
			Times.Never);
}
