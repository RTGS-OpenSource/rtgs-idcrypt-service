using Moq;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Signature;

public class MultipleMatchingRtgsConnectionFixture : ConnectionsTestFixtureBase
{
	private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

	private readonly DateTime _referenceDate =
		DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

	public MultipleMatchingRtgsConnectionFixture()
	{
		_dateTimeProviderMock.SetupGet(provider => provider.UtcNow)
			.Returns(_referenceDate);

		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SignDocument.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; set; }

	protected override async Task Seed()
	{
		var tooOldConnection = new RtgsConnection
		{
			PartitionKey = "alias-1",
			RowKey = "connection-1",
			ConnectionId = "connection-1",
			Alias = "alias-1",
			CreatedAt = new DateTime(2000, 01, 01).ToUniversalTime(),
			ActivatedAt = _referenceDate.Subtract(TimeSpan.FromDays(3)),
			Status = "Active",
		};

		var tooNewConnection = new RtgsConnection
		{
			PartitionKey = "alias-2",
			RowKey = "connection-2",
			ConnectionId = "connection-2",
			Alias = "alias-2",
			CreatedAt = new DateTime(2000, 01, 01).ToUniversalTime(),
			ActivatedAt = _referenceDate.Subtract(TimeSpan.FromMinutes(3)),
			Status = "Active",
		};

		ValidConnection = new RtgsConnection
		{
			PartitionKey = "alias-3",
			RowKey = "connection-3",
			ConnectionId = "connection-3",
			Alias = "alias-3",
			CreatedAt = new DateTime(2000, 01, 01).ToUniversalTime(),
			ActivatedAt = _referenceDate.Subtract(TimeSpan.FromDays(1)),
			Status = "Active",
		};

		var rtgsConnections = new List<RtgsConnection>
		{
			tooOldConnection,
			tooNewConnection,
			ValidConnection
		};

		foreach (var connection in rtgsConnections)
		{
			await InsertRtgsConnectionAsync(connection);
		}
	}

	public RtgsConnection ValidConnection { get; private set; }

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services
				.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
				.AddDateTimeProvider(_dateTimeProviderMock.Object)
		);
}
