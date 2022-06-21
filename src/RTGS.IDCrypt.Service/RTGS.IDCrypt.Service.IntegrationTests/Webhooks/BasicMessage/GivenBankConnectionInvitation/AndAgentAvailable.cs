using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenBankConnectionInvitation;

public class AndAgentAvailable : IClassFixture<ConnectionInvitationFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly ConnectionInvitationFixture _testFixture;
	private HttpResponseMessage _httpResponse;
	private BankConnectionInvitation _connectionInvitation;

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
		_connectionInvitation = new BankConnectionInvitation
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
			Type = "type",
			FromRtgsGlobalId = "rtgs-global-id"
		};

		var basicMessage = new IdCryptBasicMessage
		{
			ConnectionId = "connection_id",
			Content = JsonSerializer.Serialize(new BasicMessageContent<BankConnectionInvitation>
			{
				MessageType = nameof(BankConnectionInvitation),
				MessageContent = _connectionInvitation
			})
		};

		_httpResponse = await _client.PostAsJsonAsync("/v1/idcrypt/topic/basicmessages", basicMessage);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenExpectedIdCryptAgentPathsAreCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			"/connections/receive-invitation",
			"/connections/connection-id/accept-invitation");

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
		_testFixture.BankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Where(connection =>
				connection.PartitionKey == _connectionInvitation.FromRtgsGlobalId
				&& connection.RowKey == AcceptInvitation.ExpectedResponse.Alias)
			.Should().ContainSingle();

	[Fact]
	public void ThenReturnOk() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
}
