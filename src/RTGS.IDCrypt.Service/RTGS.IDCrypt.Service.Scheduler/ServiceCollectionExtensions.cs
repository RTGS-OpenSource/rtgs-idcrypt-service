﻿using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RTGS.IDCrypt.Service.Scheduler.Monitoring;

namespace RTGS.IDCrypt.Service.Scheduler;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddRtgsDependencies(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddHttpClient("IdCryptServiceClient", cfg =>
		{
			var baseAddress = configuration.GetValue<string>("IdCryptServiceAddress");
			if (string.IsNullOrEmpty(baseAddress))
			{
				throw new Exception("IdCryptServiceAddress not set.");
			}
			cfg.BaseAddress = new Uri(baseAddress);
		});
		services.AddSingleton<ITelemetryInitializer>(new TelemetryInitializer(configuration));
		services.AddApplicationInsightsTelemetryWorkerService();

		return services;
	}
}
