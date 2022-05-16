using System.Text.Json;
using Microsoft.Extensions.Logging;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptConnectionMessageHandlerTests;

public class GivenStatusIsNotActive
{
	[Fact]
	public async Task ThenLog()
	{
		var logger = new FakeLogger<IdCryptConnectionMessageHandler>();

		var handler = new IdCryptConnectionMessageHandler(logger);

		var notActiveConnection = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "not-active"
		};

		var message = JsonSerializer.Serialize(notActiveConnection);

		await handler.HandleAsync(message);

		logger.Logs[LogLevel.Debug].Should().BeEquivalentTo(new List<string>
		{
			"Ignoring connection with alias alias because state is not-active"
		});
	}
}
