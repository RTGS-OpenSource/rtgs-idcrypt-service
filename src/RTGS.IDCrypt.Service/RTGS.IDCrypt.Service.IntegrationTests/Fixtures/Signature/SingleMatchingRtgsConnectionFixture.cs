using Moq;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Signature;

public class SingleMatchingRtgsConnectionFixture : ConnectionsTestFixtureBase
{
	private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

	private readonly DateTime _referenceDate =
		DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

	public SingleMatchingRtgsConnectionFixture()
	{
		_dateTimeProviderMock.SetupGet(provider => provider.UtcNow)
			.Returns(_referenceDate);

		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SignDocument.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override async Task Seed()
	{
		await InsertRtgsConnectionAsync(new()
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias",
			CreatedAt = new DateTime(2000, 01, 01).ToUniversalTime(),
			ActivatedAt = _referenceDate.Subtract(TimeSpan.FromMinutes(6)),
			Status = "Active",
		});

	}
	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services
				.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
				.AddDateTimeProvider(_dateTimeProviderMock.Object)
		);
}
