using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RTGS.IDCrypt.Service.Functions;

public class Program
{
	public static void Main()
	{
		var host = new HostBuilder()
			.ConfigureAppConfiguration(configurationBuilder => configurationBuilder
				.AddJsonFile("local.settings.json", true, false))
			.ConfigureFunctionsWorkerDefaults()
			.ConfigureServices((context, services) =>
			{
				services.AddHttpClient<BankConnectionCycle>(cfg =>
				{
					cfg.BaseAddress = new Uri(context.Configuration.GetValue<string>("IdCryptServiceBaseAddress"));
				});
				services.AddSingleton<ITelemetryInitializer>(new TelemetryInitalizer());
				services.AddApplicationInsightsTelemetryWorkerService();
			})
			.Build();
		
		host.Run();
	}
}
