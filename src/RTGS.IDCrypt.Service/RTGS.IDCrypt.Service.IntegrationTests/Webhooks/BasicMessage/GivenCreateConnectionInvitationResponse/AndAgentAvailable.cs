using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenCreateConnectionInvitationResponse;

public class AndAgentAvailable : IClassFixture<ConnectionInvitationFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly ConnectionInvitationFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public AndAgentAvailable(ConnectionInvitationFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();

		AcceptInvitation.ConnectionId = "connection-id" + Guid.NewGuid();
		AcceptInvitation.Alias = "alias" + Guid.NewGuid();
	}

	public async Task InitializeAsync()
	{
		var connectionInvitation = new ConnectionInvitation
		{
			Alias = "alias",
			ImageUrl = "image-url",
			InvitationUrl = "invitation-url",
			Did = "did",
			Label = "label",
			RecipientKeys = new[] { "recipient-key-1" },
			ServiceEndpoint = "service-endpoint",
			Id = "id",
			PublicDid = "public-did",
			Type = "type"
		};

		var basicMessage = new IdCryptBasicMessage
		{
			MessageType = nameof(ConnectionInvitation),
			ConnectionId = "connection_id",
			Content = JsonSerializer.Serialize(connectionInvitation)
		};

		_httpResponse = await _client.PostAsJsonAsync("/v1/idcrypt/topic/basicmessage", basicMessage);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenExpectedIdCryptAgentPathsAreCalled()
	{
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			"/connections/receive-invitation",
			"/connections/connection-id/accept-invitation");
	}

	[Fact]
	public void WhenPosting_ThenIdCryptAgentBaseAddressIsExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[ReceiveInvitation.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[AcceptInvitation.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[ReceiveInvitation.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[AcceptInvitation.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);
	}

	[Fact]
	public void WhenPosting_ThenWriteToTableStorage() =>
		_testFixture.PendingBankPartnerConnectionsTable
			.Query<PendingBankPartnerConnection>()
			.Where(connection =>
				connection.PartitionKey == AcceptInvitation.ExpectedResponse.ConnectionId
				&& connection.RowKey == AcceptInvitation.ExpectedResponse.Alias)
			.Should().ContainSingle();

	[Fact]
	public void ThenReturnOk() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
}
