using System.Net;
using System.Net.Http;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenDeleteConnectionRequest;

public class AndConnectionExists : IClassFixture<DeleteConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly DeleteConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;
	private const string ConnectionId = "connection-id-1";

	public AndConnectionExists(DeleteConnectionFixture testFixture)
	{
		_testFixture = testFixture;

		_testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		await _testFixture.TestSeed();
		_httpResponse = await _client.DeleteAsync($"api/connection/{ConnectionId}");
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenExpectedIdCryptAgentPathsAreCalled()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys($"/connections/{ConnectionId}");
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Should().ContainKeys($"/connections/{ConnectionId}/send-message");
	}

	[Fact]
	public void WhenPosting_ThenIdCryptAgentBaseAddressIsExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[DeleteConnection.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[SendBasicMessage.Path].Single()
			.RequestUri!.GetLeftPart(UriPartial.Authority)
			.Should().Be(_testFixture.Configuration["AgentApiAddress"]);
	}


	[Fact]
	public void WhenCallingIdCryptAgent_ThenApiKeyHeadersAreExpected()
	{
		using var _ = new AssertionScope();

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[DeleteConnection.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);

		_testFixture.IdCryptStatusCodeHttpHandler.Requests[SendBasicMessage.Path].Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(_testFixture.Configuration["AgentApiKey"]);
	}


	[Fact]
	public void WhenDeleting_ThenDeleteFromTableStorage() =>
		_testFixture.BankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Where(connection => connection.ConnectionId == ConnectionId)
			.Should().BeEmpty();

	[Fact]
	public void ThenReturnNoContent() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
}
