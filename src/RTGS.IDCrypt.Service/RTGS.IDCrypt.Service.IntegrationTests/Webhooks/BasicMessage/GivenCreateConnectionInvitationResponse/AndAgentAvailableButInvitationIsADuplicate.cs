﻿using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.BasicMessage.GivenCreateConnectionInvitationResponse;

public class AndAgentAvailableButInvitationIsADuplicate : IClassFixture<ConnectionInvitationFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private HttpResponseMessage _secondHttpResponse;

	public AndAgentAvailableButInvitationIsADuplicate(ConnectionInvitationFixture testFixture)
	{
		testFixture.IdCryptStatusCodeHttpHandler.Reset();

		_client = testFixture.CreateClient();
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

		await _client.PostAsJsonAsync("/v1/idcrypt/topic/basicmessage", basicMessage);
		_secondHttpResponse = await _client.PostAsJsonAsync("/v1/idcrypt/topic/basicmessage", basicMessage);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public async Task ThenReturnInternalServerError()
	{
		using var _ = new AssertionScope();

		_secondHttpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		var content = await _secondHttpResponse.Content.ReadAsStringAsync();

		content.Should().Contain("{\"error\":\"The specified entity already exists.");
	}
}