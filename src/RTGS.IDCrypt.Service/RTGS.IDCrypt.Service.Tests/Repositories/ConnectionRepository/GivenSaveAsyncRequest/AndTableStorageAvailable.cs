using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.ConnectionRepository.GivenSaveAsyncRequest;

public class AndTableStorageAvailable : IAsyncLifetime
{
	private readonly Service.Repositories.ConnectionRepository _connectionRepository;
	private readonly BankPartnerConnection _connection;
	private readonly Mock<IStorageTableResolver> _storageTableResolverMock;
	private readonly Mock<TableClient> _tableClientMock;

	public AndTableStorageAvailable()
	{
		_connection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "connection-id",
			PublicDid = "public-did",
			Alias = "alias"
		};

		Func<BankPartnerConnection, bool> connectionMatches = request =>
		{
			request.Should().BeEquivalentTo(_connection, options =>
			{
				options.Excluding(connection => connection.ETag);
				options.Excluding(connection => connection.Timestamp);

				return options;
			});

			return true;
		};

		_tableClientMock = new Mock<TableClient>();
		_tableClientMock
			.Setup(tableClient => tableClient.AddEntityAsync(
				It.Is<BankPartnerConnection>(connection => connectionMatches(connection)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		_storageTableResolverMock = new Mock<IStorageTableResolver>();
		_storageTableResolverMock
			.Setup(resolver => resolver.GetTable("bankPartnerConnections"))
			.Returns(_tableClientMock.Object)
			.Verifiable();

		var logger = new FakeLogger<Service.Repositories.ConnectionRepository>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_connectionRepository =
			new Service.Repositories.ConnectionRepository(_storageTableResolverMock.Object, options, logger);
	}

	public async Task InitializeAsync() => await _connectionRepository.SaveAsync(_connection);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenExpectedTableIsResolved() => _storageTableResolverMock.Verify();

	[Fact]
	public void ThenConnectionIsWritten() => _tableClientMock.Verify();
}
