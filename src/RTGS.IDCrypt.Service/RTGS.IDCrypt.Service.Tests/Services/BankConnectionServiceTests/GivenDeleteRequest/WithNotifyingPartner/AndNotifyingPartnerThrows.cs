using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.BankConnectionServiceTests.GivenDeleteRequest.WithNotifyingPartner;

public class AndNotifyingPartnerThrows
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly Mock<IBasicMessageClient> _basicMessageClientMock = new();
	private readonly BankConnectionService _bankConnectionService;
	private readonly FakeLogger<BankConnectionService> _logger = new();
	private const string ConnectionId = "connection-id";

	public AndNotifyingPartnerThrows()
	{
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		});

		_basicMessageClientMock.Setup(client => client.SendAsync(
				ConnectionId,
				nameof(DeleteBankPartnerConnectionBasicMessage),
				It.IsAny<DeleteBankPartnerConnectionBasicMessage>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Something went wrong"))
			.Verifiable();

		_bankConnectionService = new BankConnectionService(
			_connectionsClientMock.Object,
			_logger,
			Mock.Of<IBankPartnerConnectionRepository>(),
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>(),
			coreOptions,
			_basicMessageClientMock.Object);
	}

	[Fact]
	public async Task WhenInvoked_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _bankConnectionService.DeleteAsync(ConnectionId, true))
			.Should()
			.ThrowAsync<Exception>().WithMessage("Something went wrong");

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error occurred when notifying partner of deleting connection");
	}
}
