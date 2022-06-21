using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.GivenAcceptConnectionInvitationRequest;

public class AndAgentAvailable : IClassFixture<ConnectionInvitationFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly ConnectionInvitationFixture _testFixture;
	private AcceptConnectionInvitationRequest _request;
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
		_request = new AcceptConnectionInvitationRequest
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint",
			AgentPublicDid = "agent-public-did",
			RtgsGlobalId = "rtgs-global-id"
		};

		_httpResponse = await _client.PostAsJsonAsync("api/bank-connection/accept", _request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

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
				connection.PartitionKey == _request.RtgsGlobalId
				&& connection.RowKey == AcceptInvitation.ExpectedResponse.Alias
				&& connection.Status == "Pending")
			.Should().ContainSingle();

	[Fact]
	public void ThenReturnAccepted() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
}
