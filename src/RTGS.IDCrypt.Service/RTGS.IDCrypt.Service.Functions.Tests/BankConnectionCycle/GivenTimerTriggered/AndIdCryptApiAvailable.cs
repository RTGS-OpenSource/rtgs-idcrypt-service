using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RTGS.IDCrypt.Service.Function.Tests.BankConnectionCycle.GivenTimerTriggered;

public class AndIdCryptApiAvailable
{
	private Mock<ILogger<Functions.BankConnectionCycle>> _loggerMock;
	private Mock<IHttpClientFactory> _httpClientFactoryMock;

	private readonly StatusCodeHttpHandler _statusCodeHandler;
	private readonly Functions.BankConnectionCycle _bankConnectionCycleFunction;

	public AndIdCryptApiAvailable()
	{
		_statusCodeHandler = StatusCodeHttpHandler.Builder.Create()
			.WithOkResponse(new HttpRequestResponseContext("/api/connection/InvitedPartnerIds", "[\"test-\"]"))
			.WithOkResponse(new HttpRequestResponseContext("/api/connection/cycle", string.Empty))
			.Build();

		var client = new HttpClient(_statusCodeHandler);
		client.BaseAddress = new Uri("https://localhost");
		
		_loggerMock = new Mock<ILogger<Functions.BankConnectionCycle>>();

		_httpClientFactoryMock = new Mock<IHttpClientFactory>();
		_httpClientFactoryMock
			.Setup(factory => factory.CreateClient("IdCryptServiceClient"))
			.Returns(client);

		_bankConnectionCycleFunction =
			new Functions.BankConnectionCycle(_loggerMock.Object, _httpClientFactoryMock.Object);
	}

	[Fact]
	public async Task ThenShouldReturnTrue() =>
		await _bankConnectionCycleFunction.RunAsync(new TimerInfo());
}
