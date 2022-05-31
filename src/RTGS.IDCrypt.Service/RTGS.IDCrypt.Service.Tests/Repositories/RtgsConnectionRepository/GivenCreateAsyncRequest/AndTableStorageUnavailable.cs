﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.RtgsConnectionRepository.GivenCreateAsyncRequest;

public class AndTableStorageUnavailable
{
	private readonly Service.Repositories.RtgsConnectionRepository _rtgsConnectionRepository;
	private readonly RtgsConnection _connection;
	private readonly FakeLogger<Service.Repositories.RtgsConnectionRepository> _logger = new();

	public AndTableStorageUnavailable()
	{
		_connection = new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias"
		};

		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		storageTableResolverMock
			.Setup(resolver => resolver.GetTable("rtgsConnections"))
			.Throws<Exception>();

		var options = Options.Create(new ConnectionsConfig
		{
			RtgsConnectionsTableName = "rtgsConnections"
		});

		_rtgsConnectionRepository = new Service.Repositories.RtgsConnectionRepository(
			storageTableResolverMock.Object,
			options,
			_logger,
			Mock.Of<IDateTimeProvider>());
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() => await FluentActions
		.Awaiting(() => _rtgsConnectionRepository.CreateAsync(_connection))
		.Should()
		.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _rtgsConnectionRepository.CreateAsync(_connection))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error occurred when saving RTGS connection"
		});
	}
}
