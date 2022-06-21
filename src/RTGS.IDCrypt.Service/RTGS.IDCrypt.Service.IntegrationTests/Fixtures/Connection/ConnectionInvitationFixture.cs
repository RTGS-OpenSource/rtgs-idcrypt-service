using RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class ConnectionInvitationFixture : ConnectionsTestFixtureBase
{
	public ConnectionInvitationFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(CreateInvitation.HttpRequestResponseContext)
			.WithOkResponse(GetPublicDid.HttpRequestResponseContext)
			.WithOkResponse(ReceiveInvitation.HttpRequestResponseContext)
			.WithOkResponse(AcceptInvitation.HttpRequestResponseContext)
			.Build();
	}

	public StatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; }

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services =>
			services.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
		);
}
