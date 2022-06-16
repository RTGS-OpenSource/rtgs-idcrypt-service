using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Scheduler.HostedServices;
using RTGS.IDCrypt.Service.Scheduler.Tests.Http;
using RTGS.IDCrypt.Service.Scheduler.Tests.Logging;

namespace RTGS.IDCrypt.Service.Scheduler.Tests.HostedServices.BankConnectionCycleServiceTests.GivenTimerTriggered;

public class AndInvitedPartnerIdsEndpointUnavailable
{
	private readonly FakeLogger<BankConnectionCycleService> _fakeLogger;
	private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
	private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;
	private readonly StatusCodeHttpHandler _statusCodeHandler;
	private readonly BankConnectionCycleService _bankConnectionCycleService;

	public AndInvitedPartnerIdsEndpointUnavailable()
	{
		_statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithServiceUnavailableResponse("/api/connection/InvitedPartnerIds")
			.Build();

		var client = new HttpClient(_statusCodeHandler)
		{
			BaseAddress = new Uri("https://localhost")
		};

		_fakeLogger = new FakeLogger<BankConnectionCycleService>();
		_hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

		_httpClientFactoryMock = new Mock<IHttpClientFactory>();
		_httpClientFactoryMock
			.Setup(factory => factory.CreateClient("IdCryptServiceClient"))
			.Returns(client);

		_bankConnectionCycleService =
			new BankConnectionCycleService(_fakeLogger, _hostApplicationLifetimeMock.Object, _httpClientFactoryMock.Object);
	}

	[Fact]
	public async Task ThenThrowExceptionAndLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _bankConnectionCycleService.StartAsync(default))
			.Should()
			.ThrowAsync<Exception>();

		_fakeLogger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error while getting partner ids");
	}
}
