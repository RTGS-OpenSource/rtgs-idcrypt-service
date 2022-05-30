using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.ConnectionRepository.GivenActivateAsyncRequest;

public class AndTableStorageUnavailable
{
	private readonly Service.Repositories.ConnectionRepository _connectionRepository;
	private readonly FakeLogger<Service.Repositories.ConnectionRepository> _logger = new();

	public AndTableStorageUnavailable()
	{
		var storageTableResolverMock = new Mock<IStorageTableResolver>();

		storageTableResolverMock
			.Setup(resolver => resolver.GetTable("bankPartnerConnections"))
			.Throws<Exception>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_connectionRepository =
			new Service.Repositories.ConnectionRepository(storageTableResolverMock.Object, options, _logger);
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() => await FluentActions
		.Awaiting(() => _connectionRepository.ActivateAsync("connection-id"))
		.Should()
		.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionRepository.ActivateAsync("connection-id"))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error]
			.Should().BeEquivalentTo("Error occurred when activating connection");
	}
}
