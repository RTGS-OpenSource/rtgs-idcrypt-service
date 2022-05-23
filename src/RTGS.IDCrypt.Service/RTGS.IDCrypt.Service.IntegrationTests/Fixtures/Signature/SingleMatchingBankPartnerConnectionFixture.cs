using Moq;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.Sign.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.Verify.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Signature;

public class SingleMatchingBankPartnerConnectionFixture : BankPartnerTestFixtureBase
{
	private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

	private readonly DateTime _referenceDate =
		DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

	public SingleMatchingBankPartnerConnectionFixture()
	{
		_dateTimeProviderMock.SetupGet(provider => provider.UtcNow)
			.Returns(_referenceDate);

		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SignDocument.HttpRequestResponseContext)
			.WithOkResponse(VerifyPrivateSignature.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override async Task Seed()
	{
		var bankPartnerConnections = new List<BankPartnerConnection>
		{
			new()
			{
				PartitionKey = "rtgs-global-id",
				RowKey = "alias",
				ConnectionId = "connection-id",
				Alias = "alias",
				CreatedAt = _referenceDate.Subtract(TimeSpan.FromMinutes(6)),
				Status = "Active"
			},
			new()
			{
				PartitionKey = "rtgs-global-id",
				RowKey = "alias",
				ConnectionId = "connection-id-1",
				Alias = "alias",
				CreatedAt = _referenceDate.Subtract(TimeSpan.FromMinutes(5)),
				Status = "Pending"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias",
				ConnectionId = "connection-id-2",
				Alias = "alias",
				CreatedAt = _referenceDate.Subtract(TimeSpan.FromMinutes(5)),
				Status = "Active"
			}
		};

		foreach (var connection in bankPartnerConnections)
		{
			await InsertBankPartnerConnectionAsync(connection);
		}
	}
	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services
				.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
				.AddDateTimeProvider(_dateTimeProviderMock.Object)
		);
}
