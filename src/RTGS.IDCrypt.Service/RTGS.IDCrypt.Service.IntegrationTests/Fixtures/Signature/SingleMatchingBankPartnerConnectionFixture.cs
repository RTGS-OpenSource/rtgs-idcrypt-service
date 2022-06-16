using Moq;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.Sign.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.Verify.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Signature;

public class SingleMatchingBankPartnerConnectionFixture : ConnectionsTestFixtureBase
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
				CreatedAt = new DateTime(2000, 01, 01).ToUniversalTime(),
				ActivatedAt = _referenceDate.Subtract(TimeSpan.FromMinutes(6)),
				PublicDid = "public-did",
				Status = "Active",
				Role = "Inviter"
			},
			new()
			{
				PartitionKey = "rtgs-global-id",
				RowKey = "alias-1",
				ConnectionId = "connection-id-1",
				Alias = "alias-1",
				CreatedAt = new DateTime(2000, 01, 01).ToUniversalTime(),
				PublicDid = "public-did-1",
				Status = "Pending",
				Role = "Inviter"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-2",
				ConnectionId = "connection-id-2",
				Alias = "alias-2",
				CreatedAt = new DateTime(2000, 01, 01).ToUniversalTime(),
				ActivatedAt = _referenceDate.Subtract(TimeSpan.FromMinutes(5)),
				PublicDid = "public-did-2",
				Status = "Active",
				Role = "Inviter"
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
