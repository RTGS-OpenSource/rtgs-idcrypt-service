using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenDeleteBank;

public class AndMessageNotSentUsingRtgsConnection : IClassFixture<DeleteBankFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly DeleteBankFixture _testFixture;
	private HttpResponseMessage _httpResponse;
	private BasicMessageContent<DeleteBankRequest> _message;

	public AndMessageNotSentUsingRtgsConnection(DeleteBankFixture testFixture)
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
			ConnectionId = "connection-id-1",
			Content = JsonSerializer.Serialize(_message),
		};

		_httpResponse = await _client.PostAsJsonAsync("/v1/idcrypt/topic/basicmessages", basicMessage);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public async Task WhenPosting_ThenAgentDeleteCalledAndSelectedConnectionsDeleted()
	{
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Count.Should().Be(0);

		_testFixture.BankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Count()
			.Should()
			.Be(5);

		_testFixture.RtgsConnectionsTable
			.Query<RtgsConnection>()
			.Count()
			.Should()
			.Be(2);

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		var content = await _httpResponse.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"Message did not originate from RTGS.\"}");
	}
}
