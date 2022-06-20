using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace RTGS.IDCrypt.Service.Scheduler;

public static class Program
{
	public static async Task<int> Main(string[] args)
	{
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
					telemetryClient = provider.GetRequiredService<TelemetryClient>()
				)
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
				await telemetryClient.FlushAsync(default);
			}

			Log.CloseAndFlush();
			host?.Dispose();
		}
	}
}
