using System.Net.Http;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Function.Tests.Http;
using RTGS.IDCrypt.Service.Function.Tests.Logging;
using RTGS.IDCrypt.Service.Functions;

namespace RTGS.IDCrypt.Service.Function.Tests.BankConnectionCycleFunction.GivenTimerTriggered;

public class AndCycleEndpointUnavailable
{
	private readonly FakeLogger<BankConnectionCycle> _fakeLogger;
	private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
	private readonly StatusCodeHttpHandler _statusCodeHandler;
	private readonly BankConnectionCycle _bankConnectionCycleFunction;
	private readonly string[] _partnerIds = new string[] { "rtgs-global-id-1", "rtgs-global-id-2" };

	public AndCycleEndpointUnavailable()
	{
		_statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext("/api/connection/InvitedPartnerIds", JsonSerializer.Serialize(_partnerIds)))
			.WithServiceUnavailableResponse("/api/connection/cycle")
			.Build();

		var client = new HttpClient(_statusCodeHandler)
		{
			BaseAddress = new Uri("https://localhost")
		};

		_fakeLogger = new FakeLogger<BankConnectionCycle>();

		_httpClientFactoryMock = new Mock<IHttpClientFactory>();
		_httpClientFactoryMock
			.Setup(factory => factory.CreateClient("IdCryptServiceClient"))
			.Returns(client);

		_bankConnectionCycleFunction =
			new BankConnectionCycle(_fakeLogger, _httpClientFactoryMock.Object);
	}

	[Fact]
	public async Task ThenThrowExceptionAndLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _bankConnectionCycleFunction.RunAsync(new TimerInfo()))
			.Should()
			.ThrowAsync<Exception>()
			.WithMessage("One or more cycling attempts failed.");

		_fakeLogger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error cycling connection for rtgs-global-id-1",
			"Error cycling connection for rtgs-global-id-2"
		});
	}
}
