﻿using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.GivenAcceptConnectionInvitationRequest;

public class GivenAcceptInvitationApiUnavailable : IClassFixture<AcceptInvitationEndpointUnavailableFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;

	private HttpResponseMessage _httpResponse;

	public GivenAcceptInvitationApiUnavailable(AcceptInvitationEndpointUnavailableFixture testFixture)
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var request = new AcceptConnectionInvitationRequest(
			"id",
			"type",
			"alias",
			"label",
			new[] { "recipient-key" },
			"service-endpoint");

		_httpResponse = await _client.PostAsJsonAsync("api/connection/accept", request);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public async Task ThenReturnInternalServerError()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		var content = await _httpResponse.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"Error accepting invitation\"}");
	}
}
