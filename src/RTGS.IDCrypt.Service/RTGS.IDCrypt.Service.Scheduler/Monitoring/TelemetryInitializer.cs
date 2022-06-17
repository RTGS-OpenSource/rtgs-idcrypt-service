using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

namespace RTGS.IDCrypt.Service.Scheduler.Monitoring;

public class TelemetryInitializer : ITelemetryInitializer
{
	private readonly string _rtgsGlobalId;

	public TelemetryInitializer(IConfiguration configuration)
	{
		_rtgsGlobalId = configuration.GetValue<string>("RtgsGlobalId") ?? "Undefined!!!";
	}

	public void Initialize(ITelemetry telemetry)
	{
		telemetry.Context.Cloud.RoleName = $"RTGS ID Crypt Service Scheduler (RtgsGlobalId: {_rtgsGlobalId})";
		telemetry.Context.Cloud.RoleInstance = "RTGS ID Crypt Service Scheduler";
	}
}
