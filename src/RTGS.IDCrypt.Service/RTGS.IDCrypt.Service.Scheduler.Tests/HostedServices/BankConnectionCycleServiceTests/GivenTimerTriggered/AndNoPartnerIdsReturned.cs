using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Scheduler.HostedServices;
using RTGS.IDCrypt.Service.Scheduler.Tests.Http;

namespace RTGS.IDCrypt.Service.Scheduler.Tests.HostedServices.BankConnectionCycleServiceTests.GivenTimerTriggered;

public class AndNoPartnerIdsReturned : IAsyncLifetime
{
	private readonly Mock<ILogger<BankConnectionCycleService>> _loggerMock;
	private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
	private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;
	private readonly StatusCodeHttpHandler _statusCodeHandler;
	private readonly BankConnectionCycleService _bankConnectionCycleService;

	public AndNoPartnerIdsReturned()
	{
		_statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext("/api/connection/InvitedPartnerIds", "[]"))
			.WithOkResponse(new HttpRequestResponseContext("/api/connection/cycle", string.Empty))
			.Build();

		var client = new HttpClient(_statusCodeHandler)
		{
			BaseAddress = new Uri("https://localhost")
		};

		_loggerMock = new Mock<ILogger<BankConnectionCycleService>>();
		_hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

		_httpClientFactoryMock = new Mock<IHttpClientFactory>();
		_httpClientFactoryMock
			.Setup(factory => factory.CreateClient("IdCryptServiceClient"))
			.Returns(client);

		_bankConnectionCycleService =
			new BankConnectionCycleService(_loggerMock.Object, _hostApplicationLifetimeMock.Object, _httpClientFactoryMock.Object);
	}

	public async Task InitializeAsync() =>
		await _bankConnectionCycleService.StartAsync(default);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenShouldGetInvitedPartnersFromService() =>
		_statusCodeHandler.Requests.Should().ContainKey("/api/connection/InvitedPartnerIds");

	[Fact]
	public void ThenShouldNotCallCycleForEachInvitedPartner() =>
		_statusCodeHandler.Requests.Should().NotContainKey("/api/connection/cycle");
}
