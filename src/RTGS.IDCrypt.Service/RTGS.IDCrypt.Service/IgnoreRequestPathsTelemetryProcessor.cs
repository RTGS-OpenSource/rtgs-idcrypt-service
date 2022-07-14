using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace RTGS.IDCrypt.Service;

public class IgnoreRequestPathsTelemetryProcessor : ITelemetryProcessor
{
	private readonly ITelemetryProcessor _next;

	public IgnoreRequestPathsTelemetryProcessor(ITelemetryProcessor next)
	{
		_next = next;
	}

	public void Process(ITelemetry item)
	{
		if (item is RequestTelemetry request && 
		    request.Url.AbsolutePath.StartsWith("/healthz/"))
		{
			return;
		}

		_next.Process(item);
	}
}
