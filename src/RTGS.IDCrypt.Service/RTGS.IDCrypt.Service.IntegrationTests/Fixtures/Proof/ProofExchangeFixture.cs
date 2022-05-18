using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler.TestData;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Proof;

public class ProofExchangeFixture : TestFixtureBase
{
	public ProofExchangeFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SendProofRequest.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
