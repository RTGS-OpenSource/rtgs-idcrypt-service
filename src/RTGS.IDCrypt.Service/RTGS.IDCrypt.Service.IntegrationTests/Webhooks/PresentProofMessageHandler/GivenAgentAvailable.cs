using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.PresentProofMessageHandler;
public class AndConnectionIsActive : IClassFixture<PendingConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly PendingConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public AndConnectionIsActive(PendingConnectionFixture testFixture)
	{
		_testFixture = testFixture;

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new Proof
		{
			ConnectionId = "connection-id",
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
			.Single(connection => connection.ConnectionId == "connection-id")
			.Status.Should().Be("Active");
}
