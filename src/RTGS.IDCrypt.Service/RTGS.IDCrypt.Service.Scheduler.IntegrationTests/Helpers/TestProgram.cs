using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.IDCrypt.Service.Scheduler.HostedServices;
using Serilog;
using Serilog.Events;

namespace RTGS.IDCrypt.Service.Scheduler.IntegrationTests.Helpers;
public class TestProgram
{
	public MultiMessageStatusCodeHttpHandler IdCryptStatusCodeHttpHandler { get; } = new(IdCryptServiceEndpoints.MockHttpResponses);
	public IConfigurationRoot Configuration { get; private set; }

	public async Task<int> Run(string[] args, CancellationToken cancellationToken)
	{
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.CreateLogger();

		Configuration = new ConfigurationBuilder()
			.AddJsonFile("testsettings.json")
			.Build();

		try
		{
			Log.Information("Starting integration test IDCrypt Service scheduler host");
			using var host = CreateHostBuilder(args).Build();

			await host.RunAsync(cancellationToken);

			return 0;
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, "Host terminated unexpectedly");
			return 1;
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}

	private IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureHostConfiguration(configBuilder => configBuilder
				.AddJsonFile("testsettings.json")
				.Build())
			.ConfigureServices((hostContext, services) =>
			{
				services
					.AddRtgsDependencies(hostContext.Configuration)
					.AddTestIdCryptHttpClient(IdCryptStatusCodeHttpHandler)
					.AddHostedService<BankConnectionCycleService>();
			})
			.UseSerilog(Log.Logger);
}
