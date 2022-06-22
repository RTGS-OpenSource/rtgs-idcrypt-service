using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.Http;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.BankConnectionCleanupServiceTests.GivenTimerTriggered;

public class AndMultipleConnectionIdsReturned : IAsyncLifetime
{
	private readonly StatusCodeHttpHandler _statusCodeHandler;
	private readonly BankConnectionCleanupService _bankConnectionCleanupService;
	private readonly string[] _connectionIds = { "connection-id-1", "connection-id-2" };

	public AndMultipleConnectionIdsReturned()
	{
		_statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext("/api/bank-connection/StaleConnectionIds", JsonSerializer.Serialize(_connectionIds)))
			.WithOkResponse(new HttpRequestResponseContext("/api/bank-connection/connection-id-1", string.Empty))
			.WithOkResponse(new HttpRequestResponseContext("/api/bank-connection/connection-id-2", string.Empty))
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
	public void ThenShouldCallDeleteForEachStaleConnection()
	{
		using var _ = new AssertionScope();

		_statusCodeHandler.Requests.Should().ContainKey("/api/bank-connection/connection-id-1");
		_statusCodeHandler.Requests.Should().ContainKey("/api/bank-connection/connection-id-2");
	}
}
