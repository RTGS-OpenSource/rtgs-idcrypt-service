﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenDeleteRequest;

public class AndBothRepositoryAndAgentCallsFail
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly ConnectionService _connectionService;
	private readonly Mock<IConnectionRepository> _connectionRepositoryMock = new();
	private const string ConnectionId = "connection-id";
	private readonly FakeLogger<ConnectionService> _logger = new();

	public AndBothRepositoryAndAgentCallsFail()
	{
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		});

		_connectionsClientMock
			.Setup(client => client.DeleteConnectionAsync(ConnectionId, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Something went wrong"))
			.Verifiable();

		_connectionRepositoryMock
			.Setup(service => service.DeleteAsync(ConnectionId,
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Something else went wrong"))
			.Verifiable();

		_connectionService = new ConnectionService(
			_connectionsClientMock.Object,
			_logger,
			_connectionRepositoryMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>(),
			coreOptions);
	}

	[Fact]
	public async Task WhenInvoked_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionService.DeleteAsync(ConnectionId))
			.Should()
			.ThrowAsync<AggregateException>()
			.WithMessage("One or more errors occurred. (Something went wrong) (Something else went wrong)");

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error occurred when deleting connection.", "Error occurred when deleting connection.");
	}
}
