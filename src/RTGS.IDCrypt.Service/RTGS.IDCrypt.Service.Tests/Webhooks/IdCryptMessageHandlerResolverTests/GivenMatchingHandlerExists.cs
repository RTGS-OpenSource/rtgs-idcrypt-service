using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks;
using RTGS.IDCrypt.Service.Webhooks.Handlers;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.IdCryptMessageHandlerResolverTests;

public class GivenMatchingHandlerExists : IAsyncLifetime
{
	private readonly FakeLogger<IdCryptMessageHandlerResolver> _logger;
	private readonly Mock<IIdCryptMessageHandler> _mockIdCryptMessageHandler;
	private readonly IdCryptMessageHandlerResolver _resolver;

	public GivenMatchingHandlerExists()
	{
		_logger = new FakeLogger<IdCryptMessageHandlerResolver>();

		_mockIdCryptMessageHandler = new Mock<IIdCryptMessageHandler>();

		_mockIdCryptMessageHandler.SetupGet(handler => handler.MessageType).Returns("test-route");

		var idCryptMessageHandlers = new List<IIdCryptMessageHandler>()
		{
			_mockIdCryptMessageHandler.Object,
		};

		_resolver = new IdCryptMessageHandlerResolver(_logger, idCryptMessageHandlers);
	}

	public async Task InitializeAsync()
	{
		var defaultHttpContext = new DefaultHttpContext
		{
			Request =
			{
				Path = "/the-path/",
				Body = new MemoryStream(Encoding.UTF8.GetBytes("the-body")),
				RouteValues = new RouteValueDictionary
				{
					{ "route", "test-route" }
				}
			}
		};

		await _resolver.ResolveAsync(defaultHttpContext);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenHandlerIsCalledWithExpected() =>
		_mockIdCryptMessageHandler.Verify(handler => handler.HandleAsync("the-body"), Times.Once);

	[Fact]
	public void ThenLog() =>
		_logger.Logs[LogLevel.Information].Should().ContainInOrder(new List<string>
		{
			"Handling request...",
			"Finished handling request"
		});
}
