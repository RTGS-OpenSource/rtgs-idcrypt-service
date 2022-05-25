using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Tests.TestData;

namespace RTGS.IDCrypt.Service.Tests.Repositories.ConnectionRepository.GivenDeleteAsyncRequest;

public class AndConnectionDoesNotExist : IAsyncLifetime
{
	private readonly Service.Repositories.ConnectionRepository _connectionRepository;
	private readonly Mock<IStorageTableResolver> _storageTableResolverMock;
	private readonly Mock<TableClient> _tableClientMock;
	private const string ConnectionId = "connection-id-999";

	public AndConnectionDoesNotExist()
	{
		var bankPartnerConnectionMock = new Mock<Azure.Pageable<BankPartnerConnection>>();
		bankPartnerConnectionMock.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			TestBankPartnerConnections.Connections
				.GetEnumerator());

		_tableClientMock = new Mock<TableClient>();

		_tableClientMock.Setup(tableClient =>
				tableClient.Query<BankPartnerConnection>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnectionMock.Object);

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

	public async Task InitializeAsync() => await _connectionRepository.DeleteAsync(ConnectionId);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenExpectedTableIsResolved() => _storageTableResolverMock.Verify();

	[Fact]
	public void ThenQueryIsPerformed() => _tableClientMock.Verify();

	[Fact]
	public void ThenNoDeleteAttemptIsMade() => _tableClientMock
		.Verify(client => client.DeleteEntityAsync(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<Azure.ETag>(),
			It.IsAny<CancellationToken>()), Times.Never);
}
