﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.RtgsConnectionRepository.GivenActivateAsyncRequest;

public class AndTableStorageUnavailable
{
	private readonly Service.Repositories.RtgsConnectionRepository _bankPartnerConnectionRepository;
	private readonly FakeLogger<Service.Repositories.RtgsConnectionRepository> _logger = new();

	public AndTableStorageUnavailable()
	{
		var storageTableResolverMock = new Mock<IStorageTableResolver>();

		storageTableResolverMock
			.Setup(resolver => resolver.GetTable("rtgsConnections"))
			.Throws<Exception>();

		var options = Options.Create(new ConnectionsConfig
		{
			RtgsConnectionsTableName = "rtgsConnections"
		});

		_bankPartnerConnectionRepository =
			new Service.Repositories.RtgsConnectionRepository(storageTableResolverMock.Object, options, _logger);
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() => await FluentActions
		.Awaiting(() => _bankPartnerConnectionRepository.ActivateAsync("connection-id"))
		.Should()
		.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _bankPartnerConnectionRepository.ActivateAsync("connection-id"))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error]
			.Should().BeEquivalentTo("Error occurred when activating connection");
	}
}
