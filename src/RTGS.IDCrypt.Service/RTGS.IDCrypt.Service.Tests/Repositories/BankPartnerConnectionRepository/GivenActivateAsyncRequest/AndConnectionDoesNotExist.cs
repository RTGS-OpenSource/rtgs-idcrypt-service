﻿using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.BankPartnerConnectionRepository.GivenActivateAsyncRequest;

public class AndConnectionDoesNotExist : IAsyncLifetime
{
	private readonly Service.Repositories.BankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly Mock<IStorageTableResolver> _storageTableResolverMock;
	private readonly Mock<TableClient> _tableClientMock;
	private readonly FakeLogger<Service.Repositories.BankPartnerConnectionRepository> _logger;

	public AndConnectionDoesNotExist()
	{
		var retrievedConnection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "connection-id",
			Alias = "alias",
			PublicDid = "public-did",
			Status = "Pending"
		};

		var bankPartnerConnectionMock = new Mock<Pageable<BankPartnerConnection>>();

		bankPartnerConnectionMock.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator())
			.Returns(new List<BankPartnerConnection> { retrievedConnection }.GetEnumerator());

		_tableClientMock = new Mock<TableClient>();

		_tableClientMock.Setup(tableClient =>
				tableClient.Query<BankPartnerConnection>(
					It.IsAny<string>(),
					It.IsAny<int?>(),
					It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnectionMock.Object);

		_storageTableResolverMock = new Mock<IStorageTableResolver>();

		_storageTableResolverMock
			.Setup(resolver => resolver.GetTable("bankPartnerConnections"))
			.Returns(_tableClientMock.Object)
			.Verifiable();

		_logger = new FakeLogger<Service.Repositories.BankPartnerConnectionRepository>();

		var options = Options.Create(new ConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_bankPartnerConnectionRepository =
			new Service.Repositories.BankPartnerConnectionRepository(_storageTableResolverMock.Object, options, _logger);
	}

	public async Task InitializeAsync() => await _bankPartnerConnectionRepository.ActivateAsync("non-existent-connection-id");

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenExpectedTableIsResolved() => _storageTableResolverMock.Verify();

	[Fact]
	public void ThenQueryIsPerformed() => _tableClientMock.Verify();

	[Fact]
	public void ThenNoUpdateAttemptIsMade() => _tableClientMock
		.Verify(client => client.UpdateEntityAsync(
			It.IsAny<BankPartnerConnection>(),
			It.IsAny<ETag>(),
			It.IsAny<TableUpdateMode>(),
			It.IsAny<CancellationToken>()), Times.Never);

	[Fact]
	public void ThenLog() =>
		_logger.Logs[LogLevel.Warning]
			.Should().BeEquivalentTo("Unable to activate connection as the connection was not found");
}
