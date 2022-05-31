using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Proof;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler.GivenAgentAvailable.AndConnectionIsActive;

public class AndFromRtgs : IClassFixture<ConnectionsWebhookFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly ConnectionsWebhookFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public AndFromRtgs(ConnectionsWebhookFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "active",
			TheirLabel = "RTGS_Jurisdiction_Agent"
		};

		_httpResponse = await _client.PostAsJsonAsync("v1/idcrypt/topic/connections", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenActivateConnection() =>
		_testFixture.RtgsConnectionsTable
			.Query<RtgsConnection>()
			.Single(connection => connection.PartitionKey == "alias")
			.Status.Should().Be("Active");

	[Fact]
	public void ThenReturnOk() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
}
