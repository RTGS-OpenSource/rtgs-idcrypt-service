﻿using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.SignMessageController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures;
using Xunit;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.SignMessageController;

public class GivenNoMatchingBankPartnerConnectionExists : IClassFixture<NoMatchingBankPartnerConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly NoMatchingBankPartnerConnectionFixture _testFixture;
	private HttpResponseMessage _httpResponse;

	public GivenNoMatchingBankPartnerConnectionExists(NoMatchingBankPartnerConnectionFixture testFixture)
	{
		_testFixture = testFixture;

		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync()
	{
		_httpResponse = await _client.PostAsync(
			"api/signmessage",
			new StringContent(
				JsonSerializer.Serialize(NoMatchingBankPartnerConnectionFixture.SignMessageRequest),
				Encoding.UTF8,
				MediaTypeNames.Application.Json));
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenIdCryptAgentIsNotCalled() =>
		_testFixture.IdCryptStatusCodeHttpHandler.Requests.Keys.Should().NotContain(SignDocument.Path);

	[Fact]
	public void ThenNotFoundResponseReceived() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
}