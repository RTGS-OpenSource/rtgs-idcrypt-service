using System.Threading.Tasks;
using RTGS.Service.Contracts.SignMessage;

namespace RTGS.Service.IntegrationTests.Fixtures;

public class NoMatchingBankPartnerConnectionFixture : TableStorageTestFixture
{
	public static SignMessageRequest SignMessageRequest => new()
	{
		Alias = "alias",
		RtgsGlobalId = "rtgs-global-id",
		Message = @"{ ""Message"": ""I am the walrus"" }"
	};

	public override Task Seed() =>
		Task.CompletedTask;
}
