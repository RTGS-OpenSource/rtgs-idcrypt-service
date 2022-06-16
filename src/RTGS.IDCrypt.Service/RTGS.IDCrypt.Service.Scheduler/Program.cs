using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.IDCrypt.Service.Scheduler;
using RTGS.IDCrypt.Service.Scheduler.HostedServices;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
		.MinimumLevel.Debug()
		.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
		.Enrich.FromLogContext()
		.WriteTo.Console()
		.WriteTo.ApplicationInsights(TelemetryConverter.Traces)
		.CreateLogger();

TelemetryClient telemetryClient = null;
IHost host = null;

try
{
	host = Host.CreateDefaultBuilder(args)
		.ConfigureServices((context, services) =>
		{
			services.AddRtgsDependencies(context.Configuration);
			services.AddHostedService<BankConnectionCycleService>();
		})
		.UseSerilog((_, provider, _) =>
		{
			telemetryClient = provider.GetRequiredService<TelemetryClient>();
		})
		.Build();

	await host.RunAsync();
	return 0;
}
catch (Exception exception)
{
	Log.Fatal(exception, "Host terminated unexpectedly");
	return 1;
}
finally
{
	if (telemetryClient != null)
	{
		telemetryClient.Flush();

		// Flush is not blocking so estimate how long the flush requires.
		await Task.Delay(TimeSpan.FromSeconds(5));
	}

	Log.CloseAndFlush();
	host?.Dispose();
}
