using System.Net;
using System.Net.Http;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenDeleteConnectionRequest;

public class AndConnectionDoesNotExists : IClassFixture<MissingDeleteConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly MissingDeleteConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;
	private const string ConnectionId = "connection-id-1";

	public AndConnectionDoesNotExists(MissingDeleteConnectionFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync() => _httpResponse = await _client.DeleteAsync($"api/connection/{ConnectionId}");

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenExpectedIdCryptAgentPathsAreCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			$"/connections/{ConnectionId}");

	[Fact]
	public void WhenPosting_ThenIdCryptAgentBaseAddressIsExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[DeleteConnection.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[DeleteConnection.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

	[Fact]
	public void ThenReturnNoContent() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
}
