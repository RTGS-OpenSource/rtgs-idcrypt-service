using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.GivenCycleConnectionRequest;

public class AndConnectionExists : IClassFixture<ConnectionCycleFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly CycleConnectionRequest _request;
	private readonly ConnectionCycleFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public AndConnectionExists(ConnectionCycleFixture testFixture)
	{
		_request = new CycleConnectionRequest
		{
			RtgsGlobalId = "rtgs-global-id"
		};

		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync() =>
		_httpResponse = await _client.PostAsJsonAsync("api/bank-connection/cycle", _request);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenIdCryptAgentBaseAddressIsExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[CreateInvitation.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[GetPublicDid.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);
	}

	[Fact]
	public void WhenPosting_ThenExpectedIdCryptAgentPathsAreCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys(
			"/connections/create-invitation",
			"/wallet/did/public",
			$"/connections/{_testFixture.ExistingConnection.ConnectionId}/send-message");

	[Fact]
	public async Task WhenPostingMultipleTimes_ThenAliasIsAlwaysUnique()
	{
		await _client.PostAsJsonAsync("api/bank-connection/create", _request);

		var inviteRequestQueryParams1 = QueryHelpers.ParseQuery(
			_testFixture.IdCryptStatusCodeHttpHandler.Requests[CreateInvitation.Path].First().RequestUri!.Query);
		var alias1 = inviteRequestQueryParams1["alias"];

		var inviteRequestQueryParams2 = QueryHelpers.ParseQuery(
			_testFixture.IdCryptStatusCodeHttpHandler.Requests[CreateInvitation.Path][1].RequestUri!.Query);
		var alias2 = inviteRequestQueryParams2["alias"];

		alias2.Should().NotBeEquivalentTo(alias1);
	}

	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[CreateInvitation.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[GetPublicDid.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);
	}

	[Fact]
	public void ThenReturnOk() =>
		 _httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

	[Fact]
	public async Task ThenSendBasicMessageWithCycleConnectionRequest()
	{
		var content = await _testFixture.IdCryptStatusCodeHttpHandler.Requests[SendBasicMessage.Path].Single().Content!.ReadAsStringAsync();

		content.Should().Be(@"{""content"":""{\u0022MessageContent\u0022:{\u0022Id\u0022:\u0022id\u0022,\u0022Type\u0022:\u0022type\u0022,\u0022Alias\u0022:\u0022alias\u0022,\u0022Label\u0022:\u0022label\u0022,\u0022RecipientKeys\u0022:[\u0022recipient-key-1\u0022],\u0022ServiceEndpoint\u0022:\u0022service-endpoint\u0022,\u0022PublicDid\u0022:\u0022Test Did\u0022,\u0022ImageUrl\u0022:null,\u0022Did\u0022:null,\u0022InvitationUrl\u0022:null,\u0022FromRtgsGlobalId\u0022:\u0022rtgs-global-id\u0022},\u0022MessageType\u0022:\u0022ConnectionInvitation\u0022}""}");
	}

	[Fact]
	public void WhenPosting_ThenWriteToTableStorage()
	{
		var inviteRequestQueryParams = QueryHelpers.ParseQuery(
			_testFixture.IdCryptStatusCodeHttpHandler.Requests[CreateInvitation.Path].First().RequestUri!.Query);

		var alias = inviteRequestQueryParams["alias"];

		_testFixture.BankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Where(connection =>
				connection.PartitionKey == _request.RtgsGlobalId
				&& connection.RowKey == alias
				&& connection.Status == "Pending")
			.Should().ContainSingle();
	}
}
