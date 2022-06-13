using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenInvitedPartnerIdsRequest;

public class AndConnectionsExist : IClassFixture<InvitedPartnerIdsConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private HttpResponseMessage _httpResponse;

	public AndConnectionsExist(InvitedPartnerIdsConnectionFixture testFixture)
	{
		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync() => _httpResponse = await _client.GetAsync("api/connection/InvitedPartnerIds");

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenReturnOk() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

	[Fact]
	public async Task ThenReturnCorrectIds()
	{
		var rtgsGlobalIds = await _httpResponse.Content.ReadFromJsonAsync<string[]>();

		rtgsGlobalIds.Should().BeEquivalentTo("rtgs-global-id", "rtgs-global-id-1");
	}
}
