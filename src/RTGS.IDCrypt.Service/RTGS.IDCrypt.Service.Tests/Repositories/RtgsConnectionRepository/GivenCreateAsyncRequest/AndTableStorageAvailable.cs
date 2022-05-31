﻿using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.RtgsConnectionRepository.GivenCreateAsyncRequest;

public class AndTableStorageAvailable : IAsyncLifetime
{
	private readonly Service.Repositories.RtgsConnectionRepository _rtgsConnectionRepository;
	private readonly RtgsConnection _connection;
	private readonly Mock<IStorageTableResolver> _storageTableResolverMock;
	private readonly Mock<TableClient> _tableClientMock;

	public AndTableStorageAvailable()
	{
		var referenceDate = DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

		_connection = new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias",
			CreatedAt = referenceDate
		};

		Func<RtgsConnection, bool> connectionMatches = request =>
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
				It.Is<RtgsConnection>(connection => connectionMatches(connection)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		_storageTableResolverMock = new Mock<IStorageTableResolver>();
		_storageTableResolverMock
			.Setup(resolver => resolver.GetTable("rtgsConnections"))
			.Returns(_tableClientMock.Object)
			.Verifiable();

		var logger = new FakeLogger<Service.Repositories.RtgsConnectionRepository>();

		var options = Options.Create(new ConnectionsConfig
		{
			RtgsConnectionsTableName = "rtgsConnections"
		});

		var dateTimeProviderMock = new Mock<IDateTimeProvider>();
		dateTimeProviderMock.SetupGet(provider => provider.UtcNow).Returns(referenceDate);

		_rtgsConnectionRepository = new Service.Repositories.RtgsConnectionRepository(
			_storageTableResolverMock.Object,
			options,
			logger,
			dateTimeProviderMock.Object);
	}

	public async Task InitializeAsync() => await _rtgsConnectionRepository.CreateAsync(_connection);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenExpectedTableIsResolved() => _storageTableResolverMock.Verify();

	[Fact]
	public void ThenConnectionIsWritten() => _tableClientMock.Verify();
}
