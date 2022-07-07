using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenDeleteBank;

public class AndHaveMatchingBankPartner : IClassFixture<DeleteBankFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly DeleteBankFixture _testFixture;
	private HttpResponseMessage _httpResponse;
	private BasicMessageContent<DeleteBankRequest> _message;

	public AndHaveMatchingBankPartner(DeleteBankFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		_message = new BasicMessageContent<DeleteBankRequest>
		{
			MessageType = nameof(DeleteBankRequest),
			MessageContent = new DeleteBankRequest("rtgs-global-id-1"),
			Source = "RTGS"
		};

		var basicMessage = new IdCryptBasicMessage
		{
			ConnectionId = "rtgs-connection-id-1",
			Content = JsonSerializer.Serialize(_message),
		};

		_httpResponse = await _client.PostAsJsonAsync("/v1/idcrypt/topic/basicmessages", basicMessage);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenAgentDeleteCalledAndSelectedConnectionsDeleted()
	{
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			"/connections/connection-id-1");

		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			"/connections/connection-id-4");

		_testFixture.BankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Where(connection => connection.ConnectionId is "connection-id-1" or "connection-id-4")
			.Should()
			.BeEmpty();

		_testFixture.BankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Count()
			.Should()
			.Be(3);

		_testFixture.RtgsConnectionsTable
			.Query<RtgsConnection>()
			.Count()
			.Should()
			.Be(2);

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}
