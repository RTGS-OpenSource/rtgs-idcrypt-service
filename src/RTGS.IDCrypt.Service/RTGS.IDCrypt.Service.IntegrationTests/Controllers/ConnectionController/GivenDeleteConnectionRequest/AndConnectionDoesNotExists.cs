using System.Net;
using System.Net.Http;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenDeleteConnectionRequest;

public class AndConnectionDoesNotExists : IClassFixture<MissingDeleteConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private HttpResponseMessage _httpResponse;
	private const string ConnectionId = "connection-id-1";

	public AndConnectionDoesNotExists(MissingDeleteConnectionFixture testFixture)
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync() => _httpResponse = await _client.DeleteAsync($"api/connection/{ConnectionId}");

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenReturnNoContent() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
}
