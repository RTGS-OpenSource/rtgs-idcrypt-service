using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionStorageService.GivenSavingPendingBankPartnerConnection;

public class AndWriteToStorageFails
{
	private readonly Service.Services.ConnectionStorageService _connectionStorageService;
	private readonly PendingBankPartnerConnection _pendingConnection;
	private readonly FakeLogger<Service.Services.ConnectionStorageService> _logger = new();

	public AndWriteToStorageFails()
	{
		_pendingConnection = new PendingBankPartnerConnection
		{
			PartitionKey = "connection-id",
			RowKey = "alias",
			ConnectionId = "connection-id",
			Alias = "alias"
		};

		var tableClientMock = new Mock<TableClient>();
		tableClientMock
			.Setup(tableClient => tableClient.AddEntityAsync(
				It.IsAny<PendingBankPartnerConnection>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		storageTableResolverMock
			.Setup(resolver => resolver.GetTable("pendingBankPartnerConnections"))
			.Returns(tableClientMock.Object)
			.Verifiable();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			PendingBankPartnerConnectionsTableName = "pendingBankPartnerConnections"
		});

		_connectionStorageService =
			new Service.Services.ConnectionStorageService(storageTableResolverMock.Object, options, _logger);
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() => await FluentActions
		.Awaiting(() => _connectionStorageService.SavePendingBankPartnerConnectionAsync(_pendingConnection))
		.Should()
		.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionStorageService.SavePendingBankPartnerConnectionAsync(_pendingConnection))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error occurred when saving pending bank partner connection"
		});
	}
}
