using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.SignMessageController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyControllerTests.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public class SingleMatchingBankPartnerConnectionFixture : BankPartnerTestFixtureBase
{
	private readonly List<BankPartnerConnection> _bankPartnerConnections = new()
	{
		new()
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "connection-id",
			Alias = "alias"
		},
		new()
		{
			PartitionKey = "rtgs-global-id-1",
			RowKey = "alias",
			ConnectionId = "connection-id-1",
			Alias = "alias"
		}
	};

	public SingleMatchingBankPartnerConnectionFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SignDocument.HttpRequestResponseContext)
			.WithOkResponse(VerifyPrivateSignature.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; set; }

	protected override async Task Seed()
	{
		foreach (var connection in _bankPartnerConnections)
		{
			await InsertBankPartnerConnectionAsync(connection);
		}
	}

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
