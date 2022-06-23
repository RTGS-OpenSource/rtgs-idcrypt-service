using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.Http;
using RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.Logging;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.BankConnectionCleanupServiceTests.GivenTimerTriggered;

public class AndCycleEndpointUnavailable
{
	private readonly FakeLogger<BankConnectionCleanupService> _fakeLogger;
	private readonly BankConnectionCleanupService _bankConnectionCleanupService;
	private readonly string[] _connectionIds = { "connection-id-1", "connection-id-2" };

	public AndCycleEndpointUnavailable()
	{
		StatusCodeHttpHandler statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext(new MockHttpRequest(HttpMethod.Get, "/api/bank-connection/ObsoleteConnectionIds"), JsonSerializer.Serialize(_connectionIds)))
			.WithServiceUnavailableResponse(new MockHttpRequest(HttpMethod.Delete, "/api/bank-connection/connection-id-1"))
			.WithServiceUnavailableResponse(new MockHttpRequest(HttpMethod.Delete, "/api/bank-connection/connection-id-2"))
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
			.ThrowAsync<BankConnectionCleanupException>()
			.WithMessage("One or more delete attempts failed.");

		_fakeLogger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error deleting connection for connection-id-1",
			"Error deleting connection for connection-id-2"
		});
	}
}
