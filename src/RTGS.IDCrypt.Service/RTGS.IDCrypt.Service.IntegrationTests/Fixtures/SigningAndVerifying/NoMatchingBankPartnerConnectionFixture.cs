using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public class NoMatchingBankPartnerConnectionFixture : BankPartnerTestFixtureBase
{
	public NoMatchingBankPartnerConnectionFixture()
		: base()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; set; }


	protected override Task Seed() =>
		Task.CompletedTask;

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
