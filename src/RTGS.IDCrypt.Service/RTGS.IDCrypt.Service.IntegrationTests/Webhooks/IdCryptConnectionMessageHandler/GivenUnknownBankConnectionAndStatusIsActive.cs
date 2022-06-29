using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Proof;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler;

public class GivenUnknownBankConnectionAndStatusIsActive : IClassFixture<ConnectionsWebhookFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private HttpResponseMessage _httpResponse;

	public GivenUnknownBankConnectionAndStatusIsActive(ConnectionsWebhookFixture testFixture)
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new IdCryptConnection
		{
			Alias = "unknown-alias",
			ConnectionId = "connection-id",
			State = "active",
			TheirLabel = "RTGS_Bank_Agent"
		};

		_httpResponse = await _client.PostAsJsonAsync("v1/idcrypt/topic/connections", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenReturnInternalServerError() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
}
