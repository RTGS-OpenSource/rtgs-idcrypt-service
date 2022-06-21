using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.ConnectionCycleScheduler;
using RTGS.IDCrypt.Service.Scheduler.Tests.Http;

namespace RTGS.IDCrypt.Service.Scheduler.Tests.BankConnectionCycleServiceTests.GivenTimerTriggered;

public class AndNoPartnerIdsReturned : IAsyncLifetime
{
	private readonly StatusCodeHttpHandler _statusCodeHandler;
	private readonly BankConnectionCycleService _bankConnectionCycleService;

	public AndNoPartnerIdsReturned()
	{
		_statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext("/api/bank-connection/InvitedPartnerIds", "[]"))
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
	public void ThenShouldNotCallCycleForEachInvitedPartner() =>
		_statusCodeHandler.Requests.Should().NotContainKey("/api/bank-connection/cycle");
}
