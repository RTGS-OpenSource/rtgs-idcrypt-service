using System.Net.Http;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Function.Tests.Http;
using RTGS.IDCrypt.Service.Functions;

namespace RTGS.IDCrypt.Service.Function.Tests.BankConnectionCycleFunction.GivenTimerTriggered;

public class AndMultiplePartnerIdsReturned : IAsyncLifetime
{
	private readonly Mock<ILogger<BankConnectionCycle>> _loggerMock;
	private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
	private readonly StatusCodeHttpHandler _statusCodeHandler;
	private readonly BankConnectionCycle _bankConnectionCycleFunction;
	private readonly string[] _partnerIds = new string[] { "rtgs-global-id-1", "rtgs-global-id-2" };

	public AndMultiplePartnerIdsReturned()
	{
		_statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext("/api/connection/InvitedPartnerIds", JsonSerializer.Serialize(_partnerIds)))
			.WithOkResponse(new HttpRequestResponseContext("/api/connection/cycle", string.Empty))
			.Build();

		var client = new HttpClient(_statusCodeHandler)
		{
			BaseAddress = new Uri("https://localhost")
		};

		_loggerMock = new Mock<ILogger<BankConnectionCycle>>();

		_httpClientFactoryMock = new Mock<IHttpClientFactory>();
		_httpClientFactoryMock
			.Setup(factory => factory.CreateClient("IdCryptServiceClient"))
			.Returns(client);

		_bankConnectionCycleFunction =
			new BankConnectionCycle(_loggerMock.Object, _httpClientFactoryMock.Object);
	}

	public async Task InitializeAsync() =>
		await _bankConnectionCycleFunction.RunAsync(new TimerInfo());

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenShouldGetInvitedPartnersFromService() =>
		_statusCodeHandler.Requests.Should().ContainKey("/api/connection/InvitedPartnerIds");

	[Fact]
	public async Task ThenShouldCallCycleForEachInvitedPartner() =>
		(await Task.WhenAll(_statusCodeHandler.Requests["/api/connection/cycle"]
			.Select(async request => await request.Content!.ReadAsStringAsync())))
			.Should().Contain(_partnerIds.Select(id => @$"{{""rtgsGlobalId"":""{id}""}}"));
}
