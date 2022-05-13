using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenAcceptConnectionInvitationRequest;

public class AndAgentAvailableButInvitationIsADuplicate : IClassFixture<ConnectionInvitationFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private HttpResponseMessage _secondHttpResponse;

	public AndAgentAvailableButInvitationIsADuplicate(ConnectionInvitationFixture testFixture)
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

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

		await _client.PostAsJsonAsync("api/connection/accept", request);
		_secondHttpResponse = await _client.PostAsJsonAsync("api/connection/accept", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenReturnInternalServerError() =>
		_secondHttpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
}
