using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler;

public static class Program
{
	public static async Task<int> Main(string[] args)
	{
		CreateSerilogLogger();

		TelemetryClient telemetryClient = null;
		IHost host = null;

		try
		{
			host = Host.CreateDefaultBuilder(args)
				.ConfigureServices((context, services) =>
				{
					services.AddRtgsDependencies(context.Configuration);
					services.AddHostedService<BankConnectionCleanupService>();
				})
				.UseSerilog((_, provider, config) =>
				{
					telemetryClient = provider.GetRequiredService<TelemetryClient>();

					ConfigureLogging(config)
						.WriteTo.ApplicationInsights(telemetryClient, TelemetryConverter.Traces);
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
				await telemetryClient.FlushAsync(default);
			}

			Log.CloseAndFlush();
			host?.Dispose();
		}
	}

	private static void CreateSerilogLogger() =>
		Log.Logger = ConfigureLogging(new LoggerConfiguration())
			.CreateLogger();

	private static LoggerConfiguration ConfigureLogging(LoggerConfiguration loggerConfiguration) =>
		loggerConfiguration
			.MinimumLevel.Debug()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
			.Enrich.FromLogContext()
			.WriteTo.Console();
}
