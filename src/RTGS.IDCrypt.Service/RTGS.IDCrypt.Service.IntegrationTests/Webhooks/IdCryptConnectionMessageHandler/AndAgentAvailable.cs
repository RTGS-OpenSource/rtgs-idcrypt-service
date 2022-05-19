using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Proof;
using RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler.TestData;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler;

public class AndAgentAvailable : IClassFixture<ProofExchangeFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly ProofExchangeFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public AndAgentAvailable(ProofExchangeFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	public async Task InitializeAsync()
	{
		var request = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "active"
		};

		_httpResponse = await _client.PostAsJsonAsync("v1/idcrypt/topic/connection", request);
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenBaseAddressIsExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[SendProofRequest.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

	[Fact]
	public void WhenCallingIdCryptAgent_ThenExpectedPathsAreCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKey("/present-proof/send-request");

	[Fact]
	public async Task WhenCallingIdCryptAgent_ThenBodyIsExpected()
	{
		var content = await _testFixture.IdCryptStatusCodeHttpHandler.Requests[SendProofRequest.Path].Single().Content!.ReadAsStringAsync();
		content.Should().BeEquivalentTo(@"{""connection_id"":""connection-id"",""comment"":null,""proof_request"":null}");
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[SendProofRequest.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);
}
