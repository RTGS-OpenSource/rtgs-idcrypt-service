using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage.Models;
using RTGSIDCryptWorker.Contracts;

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
		await _testFixture.TestSeed();
		_message = new BasicMessageContent<DeleteBankRequest>
		{
			MessageType = nameof(DeleteBankPartnerConnectionBasicMessage),
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
	public void WhenPosting_ThenShouldCallDeleteOnAllConnectionsForThePartner()
	{
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			"/connections/connection-id-1");

		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			"/connections/connection-id-4");
	}

	[Fact]
	public void WhenPosting_ThenDeleteRelatedPartnerConnectionsFromTableStorage() =>
		_testFixture.BankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Where(connection => connection.ConnectionId == "connection-id-1" || connection.ConnectionId == "connection-id-4")
			.Should()
			.BeEmpty();

	[Fact]
	public void ThenReturnOk() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
}
