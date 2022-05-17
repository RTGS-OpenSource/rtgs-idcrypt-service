﻿using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenCreateConnectionInvitationResponse;

public class AndReceiveInvitationApiUnavailable : IClassFixture<ReceiveInvitationEndpointUnavailableFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;

	private HttpResponseMessage _httpResponse;

	public AndReceiveInvitationApiUnavailable(ReceiveInvitationEndpointUnavailableFixture testFixture)
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var createConnectionInvitationResponse = new CreateConnectionInvitationResponse
		{
			Alias = "alias",
			AgentPublicDid = "agent-public-did",
			Invitation = new ConnectionInvitation
			{
				Id = "id",
				Type = "type",
				Label = "label",
				RecipientKeys = new[] { "recipient-key" },
				ServiceEndpoint = "service-endpoint"
			}
		};

		var basicMessage = new IdCryptBasicMessage
		{
			MessageType = nameof(CreateConnectionInvitationResponse),
			ConnectionId = "connection_id",
			Content = JsonSerializer.Serialize(createConnectionInvitationResponse)
		};

		_httpResponse = await _client.PostAsJsonAsync("/v1/idcrypt/topic/basicmessage", basicMessage);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public async Task ThenReturnInternalServerError()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		var content = await _httpResponse.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"Error receiving invitation\"}");
	}
}
