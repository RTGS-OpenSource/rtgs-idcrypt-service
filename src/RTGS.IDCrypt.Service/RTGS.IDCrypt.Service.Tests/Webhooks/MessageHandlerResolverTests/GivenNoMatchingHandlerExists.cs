using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks;
using RTGS.IDCrypt.Service.Webhooks.Handlers;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.MessageHandlerResolverTests;

public class GivenNoMatchingHandlerExists : IAsyncLifetime
{
	private readonly FakeLogger<MessageHandlerResolver> _logger;
	private readonly Mock<IMessageHandler> _mockMessageHandler;
	private readonly MessageHandlerResolver _resolver;
	private DefaultHttpContext _defaultHttpContext;

	public GivenNoMatchingHandlerExists()
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
					{ "route", "invalid-message-type" }
				}
			}
		};

		await _resolver.ResolveAsync(_defaultHttpContext);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void ThenHandlerIsNotCalled() =>
		_mockMessageHandler.Verify(handler => handler.HandleAsync(
			It.IsAny<string>(), It.IsAny<CancellationToken>()), 
			Times.Never);

	[Fact]
	public void ThenLog() =>
		_logger.Logs[LogLevel.Debug].Should().ContainInOrder(new List<string>
		{
			"Handling request...",
			"No message handler found for message type invalid-message-type"
		});

	[Fact]
	public void ThenResponseIsOk() =>
		_defaultHttpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
}
