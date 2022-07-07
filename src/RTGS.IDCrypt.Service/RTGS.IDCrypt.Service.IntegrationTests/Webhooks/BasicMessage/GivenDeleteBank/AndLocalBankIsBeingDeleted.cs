using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenDeleteBank;
public class AndLocalBankIsBeingDeleted : IClassFixture<DeleteBankFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly DeleteBankFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	private readonly IdCryptBasicMessage _basicMessage;

	public AndLocalBankIsBeingDeleted(DeleteBankFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();

		var message = new BasicMessageContent<DeleteBankRequest>
		{
			MessageType = nameof(DeleteBankRequest),
			MessageContent = new DeleteBankRequest("rtgs-global-id"),
			Source = "RTGS"
		};

		_basicMessage = new IdCryptBasicMessage
		{
			ConnectionId = "rtgs-connection-id-1",
			Content = JsonSerializer.Serialize(message),
		};
	}

	public async Task InitializeAsync() =>
		_httpResponse = await _client.PostAsJsonAsync("/v1/idcrypt/topic/basicmessages", _basicMessage);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenAgentDeleteCalledAndConnectionsDeleted()
	{
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Count.Should().Be(7);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			"/connections/connection-id-1",
			"/connections/connection-id-2",
			"/connections/connection-id-3",
			"/connections/connection-id-4",
			"/connections/connection-id-5",
			"/connections/rtgs-connection-id-1",
			"/connections/rtgs-connection-id-2");

		_testFixture.BankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Should()
			.BeEmpty();

		_testFixture.RtgsConnectionsTable
			.Query<RtgsConnection>()
			.Should()
			.BeEmpty();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}
