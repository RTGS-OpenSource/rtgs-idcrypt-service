using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.ConnectionCycleScheduler;
using RTGS.IDCrypt.Service.Scheduler.Tests.Http;
using RTGS.IDCrypt.Service.Scheduler.Tests.Logging;

namespace RTGS.IDCrypt.Service.Scheduler.Tests.BankConnectionCycleServiceTests.GivenTimerTriggered;

public class AndCycleEndpointUnavailable
{
	private readonly FakeLogger<BankConnectionCycleService> _fakeLogger;
	private readonly BankConnectionCycleService _bankConnectionCycleService;
	private readonly string[] _partnerIds = { "rtgs-global-id-1", "rtgs-global-id-2" };

	public AndCycleEndpointUnavailable()
	{
		StatusCodeHttpHandler statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext("/api/bank-connection/InvitedPartnerIds", JsonSerializer.Serialize(_partnerIds)))
			.WithServiceUnavailableResponse("/api/bank-connection/cycle")
			.Build();

		var client = new HttpClient(statusCodeHandler)
		{
			BaseAddress = new Uri("https://localhost")
		};

		_fakeLogger = new FakeLogger<BankConnectionCycleService>();
		var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

		var httpClientFactoryMock = new Mock<IHttpClientFactory>();
		httpClientFactoryMock
			.Setup(factory => factory.CreateClient("IdCryptServiceClient"))
			.Returns(client);

		_bankConnectionCycleService =
			new BankConnectionCycleService(_fakeLogger, hostApplicationLifetimeMock.Object, httpClientFactoryMock.Object);
	}

	[Fact]
	public async Task ThenThrowExceptionAndLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _bankConnectionCycleService.StartAsync(default))
			.Should()
			.ThrowAsync<BankConnectionCycleException>()
			.WithMessage("One or more cycling attempts failed.");

		_fakeLogger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error cycling connection for rtgs-global-id-1",
			"Error cycling connection for rtgs-global-id-2"
		});
	}
}
