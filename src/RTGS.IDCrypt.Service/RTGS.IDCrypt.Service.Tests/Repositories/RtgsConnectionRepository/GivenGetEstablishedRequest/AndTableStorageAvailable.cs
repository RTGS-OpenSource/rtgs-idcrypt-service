using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.RtgsConnectionRepository.GivenGetEstablishedRequest;

public class AndTableStorageAvailable : IAsyncLifetime
{
	private readonly Service.Repositories.RtgsConnectionRepository _rtgsConnectionRepository;
	private readonly Mock<IStorageTableResolver> _storageTableResolverMock;
	private readonly RtgsConnection _establishedConnection;

	private RtgsConnection _retrievedConnection;

	public AndTableStorageAvailable()
	{
		var referenceDate = DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

		const int maximumConnectionAgeInMinutes = 5;

		var config = new ConnectionsConfig
		{
			RtgsConnectionsTableName = "rtgsConnections",
			MinimumConnectionAge = TimeSpan.FromMinutes(maximumConnectionAgeInMinutes)
		};

		_establishedConnection = new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias",
			Status = "Active",
			CreatedAt = referenceDate.Subtract(TimeSpan.FromMinutes(maximumConnectionAgeInMinutes + 1))
		};

		var tooNewConnection = new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias",
			Status = "Active",
			CreatedAt = referenceDate.Subtract(TimeSpan.FromMinutes(maximumConnectionAgeInMinutes - 1))
		};

		var staleConnection = new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias",
			Status = "Active",
			CreatedAt = referenceDate.Subtract(TimeSpan.FromMinutes(maximumConnectionAgeInMinutes + 10))
		};

		var inactiveConnection = new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias",
			Status = "Pending",
			CreatedAt = referenceDate.Subtract(TimeSpan.FromMinutes(maximumConnectionAgeInMinutes + 1))
		};

		var rtgsConnectionsMock = new Mock<AsyncPageable<RtgsConnection>>();

		rtgsConnectionsMock.Setup(rtgsConnections => rtgsConnections.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
			.Returns(new List<RtgsConnection>
			{
				_establishedConnection,
				tooNewConnection,
				staleConnection,
				inactiveConnection
			}.ToAsyncEnumerable().GetAsyncEnumerator());

		var tableClientMock = new Mock<TableClient>();

		tableClientMock.Setup(tableClient =>
				tableClient.QueryAsync<RtgsConnection>(
					It.IsAny<string>(),
					It.IsAny<int?>(),
					It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()))
			.Returns(rtgsConnectionsMock.Object);

		_storageTableResolverMock = new Mock<IStorageTableResolver>();

		_storageTableResolverMock
			.Setup(resolver => resolver.GetTable("rtgsConnections"))
			.Returns(tableClientMock.Object)
			.Verifiable();

		var logger = new FakeLogger<Service.Repositories.RtgsConnectionRepository>();

		var dateTimeProviderMock = new Mock<IDateTimeProvider>();
		dateTimeProviderMock.SetupGet(provider => provider.UtcNow).Returns(referenceDate);

		_rtgsConnectionRepository = new Service.Repositories.RtgsConnectionRepository(
			_storageTableResolverMock.Object,
			Options.Create(config),
			logger,
			dateTimeProviderMock.Object);
	}

	public async Task InitializeAsync() =>
		_retrievedConnection = await _rtgsConnectionRepository.GetEstablishedAsync();

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenExpectedTableIsResolved() => _storageTableResolverMock.Verify();

	[Fact]
	public void ThenExpectedConnectionIsReturned() => _retrievedConnection.Should().Be(_establishedConnection);
}
