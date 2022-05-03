using System.Collections.Generic;
using System.Threading.Tasks;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public class SingleMatchingBankPartnerConnectionFixture : BankPartnerTestFixtureBase
{
	public List<BankPartnerConnection> BankPartnerConnections = new()
	{
		new() {
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "connection-id",
			RtgsGlobalId = "rtgs-global-id",
			Alias = "alias"
		},
		new()
		{
			PartitionKey = "rtgs-global-id-1",
			RowKey = "alias",
			ConnectionId = "connection-id-1",
			RtgsGlobalId = "rtgs-global-id",
			Alias = "alias"
		}
	};

	public static SignMessageRequest SignMessageRequest => new()
	{
		Alias = "alias",
		RtgsGlobalId = "rtgs-global-id",
		Message = @"{ ""Message"": ""I am the walrus"" }"
	};

	public override async Task Seed()
	{
		foreach (var connection in BankPartnerConnections)
		{
			await InsertBankPartnerConnectionAsync(connection);
		}
	}
}
