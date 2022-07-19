using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.BasicMessage;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenDeleteRtgsConnection;

public class AndAgentAvailable : IClassFixture<DeleteRtgsConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly DeleteRtgsConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;
	private BasicMessageContent<DeleteRtgsConnectionBasicMessage> _message;

	public AndAgentAvailable(DeleteRtgsConnectionFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		await _testFixture.TestSeed();
		_message = new BasicMessageContent<DeleteRtgsConnectionBasicMessage>
		{
			MessageType = nameof(DeleteRtgsConnectionBasicMessage),
			MessageContent = new DeleteRtgsConnectionBasicMessage()
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
	public void WhenPosting_ThenExpectedIdCryptAgentPathsAreCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			"/connections/connection-id-1");

	[Fact]
	public void WhenPosting_ThenIdCryptAgentBaseAddressIsExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[DeleteConnection.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests[DeleteConnection.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

	[Fact]
	public void WhenPosting_DoesNotNotifyPartnerBank() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Keys.Should().NotContain(SendBasicMessage.Path);

	[Fact]
	public void WhenPosting_ThenDeleteFromTableStorage() =>
		_testFixture.RtgsConnectionsTable
			.Query<RtgsConnection>()
			.Where(connection => connection.ConnectionId == "connection-id-1")
			.Should().BeEmpty();

	[Fact]
	public void ThenReturnOk() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
}
