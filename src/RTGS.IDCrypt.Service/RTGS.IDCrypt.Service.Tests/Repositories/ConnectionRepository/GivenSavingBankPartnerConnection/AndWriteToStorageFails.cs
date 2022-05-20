using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.ConnectionRepository.GivenSavingBankPartnerConnection;

public class AndWriteToStorageFails
{
	private readonly Service.Repositories.ConnectionRepository _connectionRepository;
	private readonly BankPartnerConnection _connection;
	private readonly FakeLogger<Service.Repositories.ConnectionRepository> _logger = new();

	public AndWriteToStorageFails()
	{
		_connection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "connection-id",
			Alias = "alias"
		};

		var tableClientMock = new Mock<TableClient>();
		tableClientMock
			.Setup(tableClient => tableClient.AddEntityAsync(
				It.IsAny<BankPartnerConnection>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		storageTableResolverMock
			.Setup(resolver => resolver.GetTable("bankPartnerConnections"))
			.Returns(tableClientMock.Object)
			.Verifiable();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_connectionRepository =
			new Service.Repositories.ConnectionRepository(storageTableResolverMock.Object, options, _logger);
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() => await FluentActions
		.Awaiting(() => _connectionRepository.SaveBankPartnerConnectionAsync(_connection))
		.Should()
		.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionRepository.SaveBankPartnerConnectionAsync(_connection))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error occurred when saving bank partner connection"
		});
	}
}
