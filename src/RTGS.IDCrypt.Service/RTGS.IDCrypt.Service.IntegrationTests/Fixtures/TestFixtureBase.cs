using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public abstract class TestFixtureBase : WebApplicationFactory<Program>
{
	public TestFixtureBase()
	{
		LoadConfig();
	}

	public IConfigurationRoot Configuration { get; private set; }

	private void LoadConfig() =>
		Configuration = new ConfigurationBuilder()
			.AddJsonFile("testsettings.json")
			.AddEnvironmentVariables()
			.Build();

	protected override IHost CreateHost(IHostBuilder builder)
	{
		CustomiseHost(builder);

		builder.ConfigureHostConfiguration(config =>
		{
			var testConfig = new ConfigurationBuilder()
				.AddJsonFile("testsettings.json")
				.AddEnvironmentVariables()
				.Build();

			config.AddConfiguration(testConfig);
		});

		return base.CreateHost(builder);
	}

	protected abstract void CustomiseHost(IHostBuilder builder);
}
