using System.Text.Json;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptConnectionMessageHandlerTests;

public class GivenBasicMessageHandlerExisis : IAsyncLifetime
{
	public async Task InitializeAsync()
	{
		var logger = new FakeLogger<IdCryptConnectionMessageHandler>();

		var activeConnection = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "active"
		};

		var handler = new IdCryptConnectionMessageHandler(logger);

		var message = JsonSerializer.Serialize(activeConnection);

		await handler.HandleAsync(message);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;
}
