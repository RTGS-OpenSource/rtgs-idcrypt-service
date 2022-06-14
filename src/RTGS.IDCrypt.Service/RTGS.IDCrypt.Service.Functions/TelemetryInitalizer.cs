using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace RTGS.IDCrypt.Service.Functions;

public class TelemetryInitalizer : ITelemetryInitializer
{
	public void Initialize(ITelemetry telemetry)
	{
		telemetry.Context.Cloud.RoleName = "IDCrypt Service Functions";
		telemetry.Context.Cloud.RoleInstance = "IDCrypt Service Functions Instance";

		if (
			telemetry is DependencyTelemetry dependencyTelemetry &&
			dependencyTelemetry.ResultCode == "409" &&
			dependencyTelemetry.Type == "Azure table" &&
			dependencyTelemetry.Name.EndsWith("Tables") &&
			dependencyTelemetry.Success == false)
		{
			dependencyTelemetry.Success = true;
		}
	}
}
