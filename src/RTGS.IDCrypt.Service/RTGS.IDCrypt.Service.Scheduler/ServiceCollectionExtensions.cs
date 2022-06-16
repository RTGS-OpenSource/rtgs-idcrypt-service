using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RTGS.IDCrypt.Service.Scheduler.Monitoring;

namespace RTGS.IDCrypt.Service.Scheduler;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddRtgsDependencies(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddHttpClient("IdCryptServiceClient", cfg =>
			cfg.BaseAddress = new Uri(configuration.GetValue<string>("IdCryptServiceBaseAddress"))
		);
		services.AddSingleton<ITelemetryInitializer>(new TelemetryInitializer());
		services.AddApplicationInsightsTelemetryWorkerService();
		
		return services;
	}
}
