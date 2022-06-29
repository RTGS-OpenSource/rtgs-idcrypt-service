using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Proof;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;
using VerifyXunit;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler;

[UsesVerify]
public class GivenCycledBankConnectionAndStatusIsActive : IClassFixture<ConnectionsWebhookFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly ConnectionsWebhookFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public GivenCycledBankConnectionAndStatusIsActive(ConnectionsWebhookFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new IdCryptConnection
		{
			Alias = "bank-alias-3",
			ConnectionId = "bank-connection-id-3",
			State = "active",
			TheirLabel = "RTGS_Bank_Agent"
		};

		_httpResponse = await _client.PostAsJsonAsync("v1/idcrypt/topic/connections", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenActivateConnection() =>
		_testFixture.BankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Single(connection => connection.RowKey == "bank-alias-3")
			.Status.Should().Be("Active");

	[Fact]
	public void ThenReturnOk() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
}
