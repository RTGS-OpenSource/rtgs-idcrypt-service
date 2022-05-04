using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace RTGS.IDCrypt.Service;

public class TelemetryInitializer : ITelemetryInitializer
{
	private readonly string _buildId;
	private readonly string _rtgsGlobalId;

	public TelemetryInitializer(IConfiguration configuration)
	{
		_buildId = Environment.GetEnvironmentVariable("BuildId");
		_rtgsGlobalId = configuration.GetValue<string>("RtgsGlobalId") ?? "Undefined!!!";
	}

	public void Initialize(ITelemetry telemetry)
	{
		telemetry.Context.Cloud.RoleName = $"RTGS ID Crypt Service (RtgsGlobalId: {_rtgsGlobalId})";
		telemetry.Context.Cloud.RoleInstance = $"RTGS ID Crypt Service {_buildId}";
	}
}
