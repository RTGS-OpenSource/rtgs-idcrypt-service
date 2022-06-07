using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.PresentProofMessageHandler.GivenRoleIsInviterAndStateIsRequestReceived;

public class AndBankPartnerConnectionDoesNotExist : IClassFixture<BankPartnerConnectionDoesNotExistFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;

	private HttpResponseMessage _httpResponse;
	private Proof _request;

	public AndBankPartnerConnectionDoesNotExist(BankPartnerConnectionDoesNotExistFixture testFixture)
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		_request = new Proof
		{
			ConnectionId = "bank-connection-id-1",
			State = "request_received"
		};

		_httpResponse = await _client.PostAsJsonAsync("v1/idcrypt/topic/present_proof", _request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenReturnsInternalServerError() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

	[Fact]
	public void WhenPosting_ThenReturnsErrorMessage() =>
		_httpResponse.Content.ReadAsStringAsync().Result.Should().Be(
			"{\"error\":\"Bank partner connection with ID bank-connection-id-1 not found\"}");
}
