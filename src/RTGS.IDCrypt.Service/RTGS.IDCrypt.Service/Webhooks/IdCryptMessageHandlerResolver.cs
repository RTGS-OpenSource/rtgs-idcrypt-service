using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RTGS.IDCrypt.Service.Webhooks.Handlers;

namespace RTGS.IDCrypt.Service.Webhooks;

public class IdCryptMessageHandlerResolver
{
	private readonly ILogger<IdCryptMessageHandlerResolver> _logger;
	private readonly IEnumerable<IIdCryptMessageHandler> _idCryptMessageHandlers;

	public IdCryptMessageHandlerResolver(
		ILogger<IdCryptMessageHandlerResolver> logger,
		IEnumerable<IIdCryptMessageHandler> idCryptMessageHandlers)
	{
		_logger = logger;
		_idCryptMessageHandlers = idCryptMessageHandlers;
	}

	public async Task Resolve(HttpContext context)
	{
		_logger.LogInformation("Handling request...");

		try
		{
			var route = context.GetRouteValue("route") as string;

			var handler = _idCryptMessageHandlers.SingleOrDefault(idCryptMessageHandler =>
				idCryptMessageHandler.MessageType.Equals(route, StringComparison.OrdinalIgnoreCase));

			using var reader = new StreamReader(context.Request.Body);
			var message = await reader.ReadToEndAsync();

			handler.Handle(message);

			_logger.LogInformation("Finished handling request");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to handle request");
		}

		await context.Response.CompleteAsync();
	}
}
