using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage.Models;
using RTGSIDCryptWorker.Contracts;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenDeleteBank;
public class AndNoMatchingBankPartners : IClassFixture<DeleteBankFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly DeleteBankFixture _testFixture;
	private HttpResponseMessage _httpResponse;
	private BasicMessageContent<DeleteBankRequest> _message;

	public AndNoMatchingBankPartners(DeleteBankFixture testFixture)
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
			MessageContent = new DeleteBankRequest("rtgs-global-id-99"),
			Source = "RTGS"
		};

		var basicMessage = new IdCryptBasicMessage
		{
			ConnectionId = "connection-id-1",
			Content = JsonSerializer.Serialize(_message)
		};

		_httpResponse = await _client.PostAsJsonAsync("/v1/idcrypt/topic/basicmessages", basicMessage);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenAgentDeleteCalledAndSelectedConnectionsDeleted()
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

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}
