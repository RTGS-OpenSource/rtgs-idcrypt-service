using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Function.Tests.Helpers;
using Xunit;

namespace RTGS.IDCrypt.Service.Function.Tests.BankConnectionCycle.GivenTimerTriggered;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private Mock<ILogger<Functions.BankConnectionCycle>> _loggerMock;
	private Mock<IHttpClientFactory> _httpClientFactoryMock;

	private readonly StatusCodeHttpHandler _statusCodeHandler;
	private readonly Functions.BankConnectionCycle _bankConnectionCycleFunction;
	private readonly string[] _partnerIds = new string[] { "rtgs-global-id-1", "rtgs-global-id-2" };

	public AndIdCryptApiAvailable()
	{
		_statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext("/api/connection/InvitedPartnerIds", JsonSerializer.Serialize(_partnerIds)))
			.WithOkResponse(new HttpRequestResponseContext("/api/connection/cycle", string.Empty))
			.Build();

		var client = new HttpClient(_statusCodeHandler);
		client.BaseAddress = new Uri("https://localhost");

		_loggerMock = new Mock<ILogger<Functions.BankConnectionCycle>>();

		_httpClientFactoryMock = new Mock<IHttpClientFactory>();
		_httpClientFactoryMock
			.Setup(factory => factory.CreateClient("IdCryptServiceClient"))
			.Returns(client);

		_bankConnectionCycleFunction =
			new Functions.BankConnectionCycle(_loggerMock.Object, _httpClientFactoryMock.Object);
	}

	public async Task InitializeAsync() =>
		await _bankConnectionCycleFunction.RunAsync(new TimerInfo());

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenShouldGetInvitedPartnersFromService() =>
		_statusCodeHandler.Requests.Should().ContainKey("/api/connection/InvitedPartnerIds");

	[Fact]
	public async void ThenShouldCallCycleForEachInvitedPartner() =>
		(await Task.WhenAll(_statusCodeHandler.Requests["/api/connection/cycle"]
			.Select(async request => await request.Content!.ReadAsStringAsync())))
			.Should().Contain(_partnerIds.Select(id => @$"{{""rtgsGlobalId"":""{id}""}}"));
}
