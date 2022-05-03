using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

namespace RTGS.IDCrypt.Service.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly BankPartnerTestFixtureBase _testFixture;

	public TestWebApplicationFactory(BankPartnerTestFixtureBase testFixture)
	{
		_testFixture = testFixture;
	}

	protected override IHost CreateHost(IHostBuilder builder)
	{
		if (_testFixture.IdCryptStatusCodeHttpHandler is not null)
		{
			builder.ConfigureServices(services =>
				services.AddTestIdCryptHttpClient(_testFixture.IdCryptStatusCodeHttpHandler)
			);
		}

		builder.ConfigureHostConfiguration(config =>
		{
			var jsonTestConfig = new ConfigurationBuilder()
				.AddJsonFile("testsettings.json")
				.AddEnvironmentVariables()
				.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string, string>("BankPartnerConnectionsTableName", _testFixture.BankPartnerConnectionsTableName)
				})
				.Build();

			config.AddConfiguration(jsonTestConfig);
		});

		return base.CreateHost(builder);
	}
}
