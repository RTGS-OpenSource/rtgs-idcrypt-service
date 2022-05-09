using System.Collections.Generic;
using System.Threading.Tasks;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.SignMessageController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public class SingleMatchingBankPartnerConnectionFixture : BankPartnerTestFixtureBase
{
	public SingleMatchingBankPartnerConnectionFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SignDocument.HttpRequestResponseContext)
			.WithOkResponse(VerifyPrivateSignature.ConnectionsHttpRequestResponseContext)
			.WithOkResponse(VerifyPrivateSignature.VerifyHttpRequestResponseContext)
			.Build();
	}

	private List<BankPartnerConnection> _bankPartnerConnections = new()
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

	protected override async Task Seed()
	{
		foreach (var connection in _bankPartnerConnections)
		{
			await InsertBankPartnerConnectionAsync(connection);
		}
	}
}
