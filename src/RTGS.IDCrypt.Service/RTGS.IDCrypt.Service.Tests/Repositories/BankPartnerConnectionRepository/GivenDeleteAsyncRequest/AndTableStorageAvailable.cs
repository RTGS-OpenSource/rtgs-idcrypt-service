using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Tests.TestData;

namespace RTGS.IDCrypt.Service.Tests.Repositories.BankPartnerConnectionRepository.GivenDeleteAsyncRequest;

public class AndTableStorageAvailable : IAsyncLifetime
{
	private readonly Service.Repositories.BankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly Mock<IStorageTableResolver> _storageTableResolverMock;
	private readonly Mock<TableClient> _tableClientMock;
	private const string ConnectionId = "connection-id-1";

	public AndTableStorageAvailable()
	{
		var bankPartnerConnectionMock = new Mock<Azure.Pageable<BankPartnerConnection>>();
		bankPartnerConnectionMock.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			TestBankPartnerConnections.Connections
				.GetEnumerator());

		_tableClientMock = new Mock<TableClient>();

		var expectedConnection = TestBankPartnerConnections.Connections.Single(x => x.ConnectionId == ConnectionId);

		_tableClientMock.Setup(tableClient =>
				tableClient.Query<BankPartnerConnection>(
					It.IsAny<string>(),
					It.IsAny<int?>(),
					It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnectionMock.Object);

		_tableClientMock
			.Setup(tableClient => tableClient.DeleteEntityAsync(
				expectedConnection.PartitionKey,
				expectedConnection.RowKey,
				It.IsAny<Azure.ETag>(),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		_storageTableResolverMock = new Mock<IStorageTableResolver>();
		_storageTableResolverMock
			.Setup(resolver => resolver.GetTable("bankPartnerConnections"))
			.Returns(_tableClientMock.Object)
			.Verifiable();

		var logger = new FakeLogger<Service.Repositories.BankPartnerConnectionRepository>();

		var options = Options.Create(new ConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_bankPartnerConnectionRepository = new Service.Repositories.BankPartnerConnectionRepository(
			_storageTableResolverMock.Object,
			options,
			logger,
			Mock.Of<IDateTimeProvider>());
	}

	public async Task InitializeAsync() => await _bankPartnerConnectionRepository.DeleteAsync(ConnectionId);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenExpectedTableIsResolved() => _storageTableResolverMock.Verify();

	[Fact]
	public void ThenConnectionDeleted() => _tableClientMock.Verify();
}
