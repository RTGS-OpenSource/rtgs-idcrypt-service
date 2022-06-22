using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.Http;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.BankConnectionCleanupServiceTests.GivenTimerTriggered;

public class AndNoConnectionIdsReturned : IAsyncLifetime
{
	private readonly StatusCodeHttpHandler _statusCodeHandler;
	private readonly BankConnectionCleanupService _bankConnectionCleanupService;

	public AndNoConnectionIdsReturned()
	{
		_statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext("/api/bank-connection/StaleConnectionIds", "[]"))
			.WithOkResponse(new HttpRequestResponseContext("/api/bank-connection/cycle", string.Empty))
			.Build();

		var client = new HttpClient(_statusCodeHandler)
		{
			BaseAddress = new Uri("https://localhost")
		};

		var loggerMock = new Mock<ILogger<BankConnectionCleanupService>>();
		var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

		var httpClientFactoryMock = new Mock<IHttpClientFactory>();
		httpClientFactoryMock
			.Setup(factory => factory.CreateClient("IdCryptServiceClient"))
			.Returns(client);

		_bankConnectionCleanupService =
			new BankConnectionCleanupService(loggerMock.Object, hostApplicationLifetimeMock.Object, httpClientFactoryMock.Object);
	}

	public async Task InitializeAsync() =>
		await _bankConnectionCleanupService.StartAsync(default);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenShouldGetStaleConnectionIdsFromService() =>
		_statusCodeHandler.Requests.Should().ContainKey("/api/bank-connection/StaleConnectionIds");

	[Fact]
	public void ThenShouldNotCallDeleteForEachStaleConnection() =>
		_statusCodeHandler.Requests.Keys
			.Where(key => !key.Equals("/api/bank-connection/StaleConnectionIds", StringComparison.OrdinalIgnoreCase))
			.Should().NotContainMatch("/api/bank-connection/*");
}
