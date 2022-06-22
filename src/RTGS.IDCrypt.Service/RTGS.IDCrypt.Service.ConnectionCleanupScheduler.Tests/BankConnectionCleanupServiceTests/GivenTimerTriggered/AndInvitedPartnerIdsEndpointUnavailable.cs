using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.Http;
using RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.Logging;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.BankConnectionCleanupServiceTests.GivenTimerTriggered;

public class AndInvitedPartnerIdsEndpointUnavailable
{
	private readonly FakeLogger<BankConnectionCleanupService> _fakeLogger;
	private readonly BankConnectionCleanupService _bankConnectionCleanupService;

	public AndInvitedPartnerIdsEndpointUnavailable()
	{
		StatusCodeHttpHandler statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithServiceUnavailableResponse("/api/bank-connection/StaleConnectionIds")
			.Build();

		var client = new HttpClient(statusCodeHandler)
		{
			BaseAddress = new Uri("https://localhost")
		};

		_fakeLogger = new FakeLogger<BankConnectionCleanupService>();
		var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

		var httpClientFactoryMock = new Mock<IHttpClientFactory>();
		httpClientFactoryMock
			.Setup(factory => factory.CreateClient("IdCryptServiceClient"))
			.Returns(client);

		_bankConnectionCleanupService =
			new BankConnectionCleanupService(_fakeLogger, hostApplicationLifetimeMock.Object, httpClientFactoryMock.Object);
	}

	[Fact]
	public async Task ThenThrowExceptionAndLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _bankConnectionCleanupService.StartAsync(default))
			.Should()
			.ThrowAsync<Exception>();

		_fakeLogger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error while getting stale connection ids");
	}
}
