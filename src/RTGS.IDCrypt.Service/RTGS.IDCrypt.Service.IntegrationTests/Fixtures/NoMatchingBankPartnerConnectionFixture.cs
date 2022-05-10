using System.Threading.Tasks;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public class NoMatchingBankPartnerConnectionFixture : BankPartnerTestFixtureBase
{
	public NoMatchingBankPartnerConnectionFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.Build();
	}

	protected override Task Seed() =>
		Task.CompletedTask;
}
