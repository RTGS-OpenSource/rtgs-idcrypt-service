using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.RtgsConnectionRepository.GivenGetEstablishedRequest;

public class AndConnectionDoesNotExist
{
	private readonly Service.Repositories.RtgsConnectionRepository _rtgsConnectionRepository;
	private readonly Mock<IStorageTableResolver> _storageTableResolverMock;
	private readonly Mock<TableClient> _tableClientMock;
	private readonly FakeLogger<Service.Repositories.RtgsConnectionRepository> _logger;

	public AndConnectionDoesNotExist()
	{
		var referenceDate = DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

		const int maximumConnectionAgeInMinues = 5;

		var config = new ConnectionsConfig
		{
			RtgsConnectionsTableName = "rtgsConnections",
			MinimumConnectionAge = TimeSpan.FromMinutes(maximumConnectionAgeInMinues)
		};

		var tooNewConnection = new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias",
			Status = "Active",
			CreatedAt = referenceDate.Subtract(TimeSpan.FromMinutes(maximumConnectionAgeInMinues - 1))
		};

		var staleConnection = new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias",
			Status = "Active",
			CreatedAt = referenceDate.Subtract(TimeSpan.FromMinutes(maximumConnectionAgeInMinues + 10))
		};

		var inactiveConnection = new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias",
			Status = "Pending",
			CreatedAt = referenceDate.Subtract(TimeSpan.FromMinutes(maximumConnectionAgeInMinues + 1))
		};

		var rtgsConnectionsMock = new Mock<AsyncPageable<RtgsConnection>>();

		rtgsConnectionsMock.Setup(rtgsConnections => rtgsConnections.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
			.Returns(new List<RtgsConnection>
			{
				tooNewConnection,
				staleConnection,
				inactiveConnection
			}.ToAsyncEnumerable().GetAsyncEnumerator());

		_tableClientMock = new Mock<TableClient>();

		_tableClientMock.Setup(tableClient =>
				tableClient.QueryAsync<RtgsConnection>(
					It.IsAny<string>(),
					It.IsAny<int?>(),
					It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()))
			.Returns(rtgsConnectionsMock.Object);

		_storageTableResolverMock = new Mock<IStorageTableResolver>();

		_storageTableResolverMock
			.Setup(resolver => resolver.GetTable("rtgsConnections"))
			.Returns(_tableClientMock.Object)
			.Verifiable();

		_logger = new FakeLogger<Service.Repositories.RtgsConnectionRepository>();

		var options = Options.Create(new ConnectionsConfig
		{
			RtgsConnectionsTableName = "rtgsConnections"
		});

		_rtgsConnectionRepository = new Service.Repositories.RtgsConnectionRepository(
			_storageTableResolverMock.Object,
			options,
			_logger,
			Mock.Of<IDateTimeProvider>());
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() => await FluentActions
		.Awaiting(() => _rtgsConnectionRepository.GetEstablishedAsync())
		.Should()
		.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _rtgsConnectionRepository.GetEstablishedAsync())
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error]
			.Should().BeEquivalentTo("No established RTGS connection found");
	}
}
