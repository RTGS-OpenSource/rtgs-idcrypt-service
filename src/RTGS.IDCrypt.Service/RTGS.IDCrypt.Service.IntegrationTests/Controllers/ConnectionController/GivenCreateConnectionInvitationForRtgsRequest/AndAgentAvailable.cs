using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using ConnectionInvitation = RTGS.IDCrypt.Service.Contracts.Connection.ConnectionInvitation;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenCreateConnectionInvitationForRtgsRequest;

public class AndAgentAvailable : IClassFixture<ConnectionInvitationFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly CreateConnectionInvitationForBankRequest _request;
	private readonly ConnectionInvitationFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public AndAgentAvailable(ConnectionInvitationFixture testFixture)
	{
		_request = new CreateConnectionInvitationForBankRequest
		{
			RtgsGlobalId = "rtgs-global-id"
		};

		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync() =>
		_httpResponse = await _client.PostAsJsonAsync("api/connection/for-rtgs", _request);

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
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys("/connections/create-invitation", "/wallet/did/public");

	[Fact]
	public async Task WhenPostingMultipleTimes_ThenAliasIsAlwaysUnique()
	{
		await _client.PostAsJsonAsync("api/connection/for-bank", _request);

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
	public async Task ThenReturnOkWithCreateConnectionInvitationResponse()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var actualResponse = await _httpResponse.Content.ReadFromJsonAsync<CreateConnectionInvitationResponse>();

		actualResponse.Should().BeEquivalentTo(new CreateConnectionInvitationResponse
		{
			FromRtgsGlobalId = _testFixture.Configuration["RtgsGlobalId"],
			Alias = "alias",
			AgentPublicDid = GetPublicDid.ExpectedDid,
			Invitation = new ConnectionInvitation
			{
				Id = "id",
				Type = "type",
				Label = "label",
				RecipientKeys = new[]
				{
					"recipient-key-1"
				},
				ServiceEndpoint = "service-endpoint"
			}
		});
	}
}
