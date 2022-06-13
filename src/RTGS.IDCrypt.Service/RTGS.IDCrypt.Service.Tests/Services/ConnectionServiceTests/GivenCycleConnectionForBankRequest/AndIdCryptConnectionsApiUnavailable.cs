﻿using Microsoft.Extensions.Logging;
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

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenCycleConnectionForBankRequest;

public class AndIdCryptConnectionsApiUnavailable
{
	private readonly Mock<IBasicMessageClient> _basicMessageClientMock = new();

	private readonly ConnectionService _connectionService;
	private readonly FakeLogger<ConnectionService> _logger;

	public AndIdCryptConnectionsApiUnavailable()
	{
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		});

		const string alias = "alias";

		var connectionsClientMock = new Mock<IConnectionsClient>();

		connectionsClientMock
			.Setup(client => client.CreateConnectionInvitationAsync(
				alias,
				It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>()
			.Verifiable();

		_logger = new FakeLogger<ConnectionService>();

		var aliasProviderMock = new Mock<IAliasProvider>();
		aliasProviderMock.Setup(provider => provider.Provide()).Returns(alias);

		_connectionService = new ConnectionService(
			connectionsClientMock.Object,
			_logger,
			Mock.Of<IBankPartnerConnectionRepository>(),
			Mock.Of<IRtgsConnectionRepository>(),
			aliasProviderMock.Object,
			Mock.Of<IWalletClient>(),
			coreOptions,
			_basicMessageClientMock.Object
		);
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _connectionService.CycleConnectionForBankAsync("rtgs-global-id"))
			.Should()
			.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionService.CycleConnectionForBankAsync("rtgs-global-id"))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error occurred when cycling connection for bank rtgs-global-id");
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
