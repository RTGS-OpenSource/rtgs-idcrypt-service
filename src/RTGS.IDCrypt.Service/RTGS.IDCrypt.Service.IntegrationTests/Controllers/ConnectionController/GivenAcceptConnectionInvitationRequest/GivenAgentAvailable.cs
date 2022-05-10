using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenAcceptConnectionInvitationRequest;

public class GivenAgentAvailable : IClassFixture<ConnectionInvitationFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly ConnectionInvitationFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public GivenAgentAvailable(ConnectionInvitationFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new AcceptConnectionInvitationRequest
		{
			Alias = "alias",
			Id = "id",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint"
		};

		_httpResponse = await _client.PostAsJsonAsync("api/connection/accept", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenExpectedIdCryptAgentPathsAreCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			"/connections/receive-invitation",
			"/connections/connection-id/accept-invitation");

	[Fact]
	public void WhenPosting_ThenIdCryptAgentBaseAddressIsExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[ReceiveInvitation.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[AcceptInvitation.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[ReceiveInvitation.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[AcceptInvitation.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);
	}

	[Fact]
	public void ThenReturnAccepted() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
}
