using System.Net;
using System.Net.Http;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenDeleteConnectionRequest;

public class AndAgentUnavailable : IClassFixture<DeleteConnectionAgentUnavailableFixture>
{
	private readonly HttpClient _client;

	public AndAgentUnavailable(DeleteConnectionAgentUnavailableFixture testFixture)
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	[Fact]
	public async Task ThenReturnInternalServerError()
	{
		var response = await _client.DeleteAsync($"api/connection/connection-id-1");

		using var _ = new AssertionScope();

		response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		var content = await response.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"One or more errors occurred. (Error deleting connection from agent)\"}");
	}
}
