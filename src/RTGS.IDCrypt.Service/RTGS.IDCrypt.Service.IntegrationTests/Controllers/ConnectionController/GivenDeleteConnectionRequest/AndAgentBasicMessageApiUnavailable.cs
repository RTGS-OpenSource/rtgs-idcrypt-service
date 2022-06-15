using System.Net;
using System.Net.Http;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenDeleteConnectionRequest;

public class AndAgentBasicMessageApiUnavailable : IClassFixture<DeleteConnectionAgentBasicMessageApiUnavailableFixture>
{
	private readonly HttpClient _client;

	public AndAgentBasicMessageApiUnavailable(DeleteConnectionAgentBasicMessageApiUnavailableFixture testFixture)
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	[Fact]
	public async Task ThenReturnInternalServerError()
	{
		var response = await _client.DeleteAsync("api/connection/connection-id-1");

		using var _ = new AssertionScope();

		response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		var content = await response.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"Error sending basic message to agent\"}");
	}
}
