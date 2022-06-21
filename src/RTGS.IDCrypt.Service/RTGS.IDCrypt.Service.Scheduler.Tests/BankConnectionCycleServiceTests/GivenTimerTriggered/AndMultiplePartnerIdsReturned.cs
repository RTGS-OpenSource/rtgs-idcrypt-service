using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Scheduler.Tests.Http;

namespace RTGS.IDCrypt.Service.Scheduler.Tests.BankConnectionCycleServiceTests.GivenTimerTriggered;

public class AndMultiplePartnerIdsReturned : IAsyncLifetime
{
	private readonly StatusCodeHttpHandler _statusCodeHandler;
	private readonly BankConnectionCycleService _bankConnectionCycleService;
	private readonly string[] _partnerIds = { "rtgs-global-id-1", "rtgs-global-id-2" };

	public AndMultiplePartnerIdsReturned()
	{
		_statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext("/api/bank-connection/InvitedPartnerIds", JsonSerializer.Serialize(_partnerIds)))
			.WithOkResponse(new HttpRequestResponseContext("/api/bank-connection/cycle", string.Empty))
			.Build();

		var client = new HttpClient(_statusCodeHandler)
		{
			BaseAddress = new Uri("https://localhost")
		};

		var loggerMock = new Mock<ILogger<BankConnectionCycleService>>();
		var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

		var httpClientFactoryMock = new Mock<IHttpClientFactory>();
		httpClientFactoryMock
			.Setup(factory => factory.CreateClient("IdCryptServiceClient"))
			.Returns(client);

		_bankConnectionCycleService =
			new BankConnectionCycleService(loggerMock.Object, hostApplicationLifetimeMock.Object, httpClientFactoryMock.Object);
	}

	public async Task InitializeAsync() =>
		await _bankConnectionCycleService.StartAsync(default);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenShouldGetInvitedPartnersFromService() =>
		_statusCodeHandler.Requests.Should().ContainKey("/api/bank-connection/InvitedPartnerIds");

	[Fact]
	public async Task ThenShouldCallCycleForEachInvitedPartner() =>
		(await Task.WhenAll(_statusCodeHandler.Requests["/api/bank-connection/cycle"]
			.Select(async request => await request.Content!.ReadAsStringAsync())))
			.Should().Contain(_partnerIds.Select(id => @$"{{""rtgsGlobalId"":""{id}""}}"));
}
