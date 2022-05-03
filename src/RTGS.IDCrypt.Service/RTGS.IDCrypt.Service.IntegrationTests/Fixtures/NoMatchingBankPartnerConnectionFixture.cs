using System.Threading.Tasks;
using RTGS.IDCrypt.Service.Contracts.SignMessage;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public class NoMatchingBankPartnerConnectionFixture : BankPartnerTestFixtureBase
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
