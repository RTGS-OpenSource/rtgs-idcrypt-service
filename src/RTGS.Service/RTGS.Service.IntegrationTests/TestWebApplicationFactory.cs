using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.Service.IntegrationTests.Fixture;
using RTGS.Service.Storage;

namespace RTGS.Service.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly TestFixture _testFixture;

	public TestWebApplicationFactory(TestFixture testFixture)
	{
		_testFixture = testFixture;
	}

	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.ConfigureHostConfiguration(config =>
		{
			var jsonTestConfig = new ConfigurationBuilder()
				.AddJsonFile("testsettings.json")
				.AddEnvironmentVariables()
				.AddConfiguration(_testFixture.Config)
				.Build();

			config.AddConfiguration(jsonTestConfig);
		});

		return base.CreateHost(builder);
	}
}
