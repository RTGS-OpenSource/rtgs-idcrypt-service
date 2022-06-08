using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.PresentProofMessageHandler.GivenRoleIsInvitee;

public class AndBankPartnerConnectionIsPending : IClassFixture<BankPartnerConnectionPendingFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly BankPartnerConnectionPendingFixture _testFixture;

	private HttpResponseMessage _httpResponse;

	public AndBankPartnerConnectionIsPending(BankPartnerConnectionPendingFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new Proof
		{
			ConnectionId = "bank-connection-id-2",
			State = "request_received"
		};

		_httpResponse = await _client.PostAsJsonAsync("v1/idcrypt/topic/present_proof", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenReturnOk() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

	[Fact]
	public void WhenPosting_ThenActivateConnection() =>
		_testFixture.BankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Single(connection => connection.ConnectionId == "bank-connection-id-2")
			.Status.Should().Be("Active");

	[Fact]
	public void WhenPosting_ThenExpectedIdCryptAgentPathsAreCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKey(
			"/connections/rtgs-connection-id-1/send-message");

	[Fact]
	public void WhenPosting_ThenIdCryptAgentBaseAddressIsExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[SendBasicMessage.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

	[Fact]
	public void WhenPosting_ThenIdCryptAgentApiKeyIsExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[SendBasicMessage.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

	[Fact]
	public async Task WhenPosting_ThenExpectedIdCryptAgentBasicMessageIsSent()
	{
		var content = await _testFixture.IdCryptStatusCodeHttpHandler.Requests[SendBasicMessage.Path].Single()
			.Content!.ReadAsStringAsync();

		content.Should().Be(
			@"{""content"":""{\u0022MessageType\u0022:\u0022SetBankPartnershipOnlineRequest\u0022,\u0022MessageContent\u0022:{\u0022RequestingBankDid\u0022:\u0022rtgs-global-id-2\u0022,\u0022ApprovingBankDid\u0022:\u0022rtgs-global-id\u0022}}""}");
	}
}
