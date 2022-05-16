using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenCreateConnectionInvitationRequest;

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
	}

	public async Task InitializeAsync() =>
		_httpResponse = await _client.PostAsync("api/connection", null);

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
		await _client.PostAsync("api/connection", null);

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

		var inviteRequestQueryParams = QueryHelpers.ParseQuery(
			_testFixture.IdCryptStatusCodeHttpHandler.Requests[CreateInvitation.Path].First().RequestUri!.Query);
		var alias = inviteRequestQueryParams["alias"];

		actualResponse.Should().BeEquivalentTo(new CreateConnectionInvitationResponse()
		{
			Alias = alias,
			AgentPublicDid = GetPublicDid.ExpectedDid,
			ConnectionId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
			Invitation = new ConnectionInvitation
			{
				Id = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
				Type = "https://didcomm.org/my-family/1.0/my-message-type",
				Label = "Bob",
				RecipientKeys = new[]
				{
					"H3C2AVvLMv6gmMNam3uVAjZpfkcJCwDwnZn6z3wXmqPV"
				},
				ServiceEndpoint = "http://192.168.56.101:8020"
			}
		});
	}

	[Fact]
	public void WhenPosting_ThenWriteToTableStorage()
	{
		var inviteRequestQueryParams = QueryHelpers.ParseQuery(
			_testFixture.IdCryptStatusCodeHttpHandler.Requests[CreateInvitation.Path].First().RequestUri!.Query);

		var alias = inviteRequestQueryParams["alias"];

		_testFixture.PendingBankPartnerConnectionsTable
			.Query<PendingBankPartnerConnection>()
			.Where(connection =>
				connection.PartitionKey == CreateInvitation.Response.ConnectionId
				&& connection.RowKey == alias)
			.Should().ContainSingle();
	}
}
