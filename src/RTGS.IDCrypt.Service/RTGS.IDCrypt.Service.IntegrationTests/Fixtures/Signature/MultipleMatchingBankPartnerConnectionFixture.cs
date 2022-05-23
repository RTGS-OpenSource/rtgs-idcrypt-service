using Moq;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.Sign.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.Verify.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Signature;

public class MultipleMatchingBankPartnerConnectionFixture : BankPartnerTestFixtureBase
{
	private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

	private readonly DateTime _referenceDate =
		DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

	public MultipleMatchingBankPartnerConnectionFixture()
	{
		_dateTimeProviderMock.SetupGet(provider => provider.UtcNow)
			.Returns(_referenceDate);

		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SignDocument.HttpRequestResponseContext)
			.WithOkResponse(VerifyPrivateSignature.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; set; }

	protected override async Task Seed()
	{
		var tooOldConnection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias-1",
			ConnectionId = "connection-1",
			Alias = "alias-1",
			CreatedAt = _referenceDate.Subtract(TimeSpan.FromDays(3)),
			PublicDid = "public-did-1",
			Status = "Active"
		};

		var tooNewConnection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias-2",
			ConnectionId = "connection-2",
			Alias = "alias-2",
			CreatedAt = _referenceDate.Subtract(TimeSpan.FromMinutes(3)),
			PublicDid = "public-did-2",
			Status = "Active"
		};

		ValidConnection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias-3",
			ConnectionId = "connection-3",
			Alias = "alias-3",
			CreatedAt = _referenceDate.Subtract(TimeSpan.FromDays(1)),
			PublicDid = "public-did-3",
			Status = "Active"
		};

		var bankPartnerConnections = new List<BankPartnerConnection>
		{
			tooOldConnection,
			tooNewConnection,
			ValidConnection
		};

		foreach (var connection in bankPartnerConnections)
		{
			await InsertBankPartnerConnectionAsync(connection);
		}
	}

	public BankPartnerConnection ValidConnection { get; private set; }

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services
				.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
				.AddDateTimeProvider(_dateTimeProviderMock.Object)
		);
}
