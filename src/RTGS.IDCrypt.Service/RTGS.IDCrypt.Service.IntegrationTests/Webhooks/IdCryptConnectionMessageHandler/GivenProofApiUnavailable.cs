using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Proof;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler;

public class GivenProofApiUnavailable : IClassFixture<SendProofRequestEndpointUnavailableFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private HttpResponseMessage _httpResponse;

	public GivenProofApiUnavailable(SendProofRequestEndpointUnavailableFixture testFixture)
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "active",
			TheirLabel = "RTGS_Bank_Agent"
		};

		_httpResponse = await _client.PostAsJsonAsync("v1/idcrypt/topic/connections", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public async Task ThenReturnInternalServerError()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		var content = await _httpResponse.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"Error sending send proof request request to agent\"}");
	}
}
