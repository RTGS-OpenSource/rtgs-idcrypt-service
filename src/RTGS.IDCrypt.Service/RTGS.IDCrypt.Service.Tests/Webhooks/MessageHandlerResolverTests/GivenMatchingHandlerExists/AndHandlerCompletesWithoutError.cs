using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks;
using RTGS.IDCrypt.Service.Webhooks.Handlers;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.MessageHandlerResolverTests.GivenMatchingHandlerExists;

public class AndHandlerCompletesWithoutError : IAsyncLifetime
{
	private readonly FakeLogger<MessageHandlerResolver> _logger;
	private readonly Mock<IMessageHandler> _mockMessageHandler;
	private readonly MessageHandlerResolver _resolver;
	private DefaultHttpContext _defaultHttpContext;

	public AndHandlerCompletesWithoutError()
	{
		_logger = new FakeLogger<MessageHandlerResolver>();

		_mockMessageHandler = new Mock<IMessageHandler>();

		_mockMessageHandler.SetupGet(handler => handler.MessageType).Returns("message-type");

		var messageHandlers = new List<IMessageHandler>()
		{
			_mockMessageHandler.Object,
		};

		_resolver = new MessageHandlerResolver(_logger, messageHandlers);
	}

	public async Task InitializeAsync()
	{
		_defaultHttpContext = new DefaultHttpContext
		{
			Request =
			{
				Path = "/the-path/",
				Body = new MemoryStream(Encoding.UTF8.GetBytes("the-body")),
				RouteValues = new RouteValueDictionary
				{
					{ "route", "message-type" }
				}
			}
		};

		await _resolver.ResolveAsync(_defaultHttpContext);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenHandlerIsCalledWithExpected() =>
		_mockMessageHandler.Verify(handler => handler.HandleAsync(
			"the-body", It.IsAny<CancellationToken>()),
			Times.Once);

	[Fact]
	public void ThenLog() =>
		_logger.Logs[LogLevel.Debug].Should().ContainInOrder(new List<string>
		{
			"Handling request...",
			"Finished handling request"
		});

	[Fact]
	public void ThenResponseIsOk() =>
		_defaultHttpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
}
