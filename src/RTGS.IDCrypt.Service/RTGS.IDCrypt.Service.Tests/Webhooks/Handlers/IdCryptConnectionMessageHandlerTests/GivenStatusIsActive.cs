using System.Text.Json;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptConnectionMessageHandlerTests;

public class GivenStatusIsActive : IAsyncLifetime
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
