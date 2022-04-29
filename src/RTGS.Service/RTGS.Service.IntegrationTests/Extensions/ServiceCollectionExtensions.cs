using Microsoft.Extensions.DependencyInjection;
using RTGS.Service.IntegrationTests.Helpers;

namespace RTGS.Service.IntegrationTests.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTestIdCryptHttpClient(
		this IServiceCollection services,
		StatusCodeHttpHandler statusCodeHttpHandler)
	{
			services
				.AddSingleton(statusCodeHttpHandler)
				.AddHttpClient("AgentHttpClient")
				.AddHttpMessageHandler<StatusCodeHttpHandler>();
	
		return services;
	}
}
