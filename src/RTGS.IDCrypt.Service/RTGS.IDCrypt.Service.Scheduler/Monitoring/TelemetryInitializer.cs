using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace RTGS.IDCrypt.Service.Scheduler.Monitoring;

public class TelemetryInitializer : ITelemetryInitializer
{
	public TelemetryInitializer()
	{
		//TODO: config for instance?	
	}

	public void Initialize(ITelemetry telemetry)
	{
		telemetry.Context.Cloud.RoleName = "IDCrypt Scheduler";
		telemetry.Context.Cloud.RoleInstance = "IDCrypt Scheduler";
	}
}
