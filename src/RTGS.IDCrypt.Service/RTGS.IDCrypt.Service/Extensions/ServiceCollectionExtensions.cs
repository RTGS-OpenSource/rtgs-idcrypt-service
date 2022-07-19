using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Webhooks;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCryptSDK;
using RTGS.IDCryptSDK.Extensions;

namespace RTGS.IDCrypt.Service.Extensions;

public static class ServiceCollectionExtensions
{
	public static void AddRtgsDependencies(this IServiceCollection services, IConfiguration config)
	{
		services.Configure<CoreConfig>(config);
		services.Configure<ConnectionsConfig>(config);

		services.AddSingleton(_ =>
		{
			var bankPartnerConnectionsConfig = new ConnectionsConfig
			{
				BankPartnerConnectionsTableName = "bankPartnerConnections",
				RtgsConnectionsTableName = "rtgsConnections",
				MinimumConnectionAge = TimeSpan.FromMinutes(5)
			};

			config.Bind(bankPartnerConnectionsConfig);

			return Options.Create(bankPartnerConnectionsConfig);
		});

		services.AddSingleton<IBankPartnerConnectionRepository, BankPartnerConnectionRepository>();
		services.AddSingleton<IRtgsConnectionRepository, RtgsConnectionRepository>();

		services.AddSingleton<IBankConnectionService, BankConnectionService>();
		services.AddSingleton<IRtgsConnectionService, RtgsConnectionService>();
		services.AddSingleton<IStorageTableResolver, StorageTableResolver>();
		services.AddSingleton<IAliasProvider, AliasProvider>();
		services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

		services.AddSingleton<MessageHandlerResolver>();
		services.AddSingleton<IMessageHandler, IdCryptConnectionMessageHandler>();
		services.AddSingleton<IMessageHandler, IdCryptBasicMessageHandler>();
		services.AddSingleton<IMessageHandler, PresentProofMessageHandler>();

		services.AddSingleton<IBasicMessageHandler, BankConnectionInvitationBasicMessageHandler>();
		services.AddSingleton<IBasicMessageHandler, RtgsConnectionInvitationBasicMessageHandler>();
		services.AddSingleton<IBasicMessageHandler, DeleteBankPartnerConnectionBasicMessageHandler>();
		services.AddSingleton<IBasicMessageHandler, DeleteRtgsConnectionBasicMessageHandler>();
		services.AddSingleton<IBasicMessageHandler, DeleteBankBasicMessageHandler>();

		services.AddIdCryptSdk(new IdCryptSdkConfiguration(
			new Uri(config["AgentApiAddress"]),
			config["AgentApiKey"],
			new Uri(config["AgentServiceEndpointAddress"]),
			$"Bank-{config["RtgsGlobalId"]}"));

		services.AddSingleton<ITelemetryInitializer>(new TelemetryInitializer(config));

		services.AddApplicationInsightsTelemetry();

		services.AddHealthChecks();

		services.AddControllers();
	}
}
