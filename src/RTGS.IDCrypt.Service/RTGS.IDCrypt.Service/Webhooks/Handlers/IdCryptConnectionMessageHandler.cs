using System.Collections.Generic;
using System.Text.Json;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.Proof;
using RTGS.IDCryptSDK.Proof.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class IdCryptConnectionMessageHandler : IMessageHandler
{
	private readonly ILogger<IdCryptConnectionMessageHandler> _logger;
	private readonly IProofClient _proofClient;

	public IdCryptConnectionMessageHandler(
		ILogger<IdCryptConnectionMessageHandler> logger,
		IProofClient proofClient)
	{
		_logger = logger;
		_proofClient = proofClient;
	}

	public string MessageType => "connection";

	public async Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		var connection = JsonSerializer.Deserialize<IdCryptConnection>(jsonMessage);

		if (connection!.State is not "active")
		{
			_logger.LogDebug("Ignoring {RequestType} with alias {Alias} because state is {State}",
				MessageType, connection.Alias, connection.State);

			return;
		}

		var request = new SendProofRequestRequest
		{
			ConnectionId = connection.ConnectionId,
			Comment = "Requesting identification",
			RequestedProofDetails = new()
			{
				Name = "RTGS.global Network Participation",
				Version = "1.0",
				Attributes = GetProofAttributes(),
				RequestedPredicates = new()
			}
		};

		try
		{
			await _proofClient.SendProofRequestAsync(request, cancellationToken);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred requesting proof");

			throw;
		}
	}

	private static Dictionary<string, RequestedAttribute> GetProofAttributes()
	{
		var rtgsNetworkParticipationProofAttributes = new List<KeyValuePair<string, string>>
		{
			KeyValuePair.Create("participant", "XvCtmx54WgYNcwAycYaFzT:3:CL:6153:default"),
			KeyValuePair.Create("RTGS_global", "XvCtmx54WgYNcwAycYaFzT:3:CL:6153:default"),
			KeyValuePair.Create("base_currency", "XvCtmx54WgYNcwAycYaFzT:3:CL:6153:default"),
			KeyValuePair.Create("parent", "XvCtmx54WgYNcwAycYaFzT:3:CL:6153:default"),
			KeyValuePair.Create("products_and_services", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("category", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("description", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("full_legal_name", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("date_of_establishment_year", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("identifier_RTGSg", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("contacts_phone", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("establishment_country_name", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("establishment_country_code", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("business_line", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("csleid", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("identifier_LEI", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("listing_status", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("aliases", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("preferred_label", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("contacts_country_name", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("contacts_country_code", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("contacts_email", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("full_legal_name_local", "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"),
			KeyValuePair.Create("idprefix", "XvCtmx54WgYNcwAycYaFzT:3:CL:6206:default"),
			KeyValuePair.Create("id", "XvCtmx54WgYNcwAycYaFzT:3:CL:6206:default"),
			KeyValuePair.Create("risk_monitoring_status", "XvCtmx54WgYNcwAycYaFzT:3:CL:6206:default"),
			KeyValuePair.Create("monitored", "XvCtmx54WgYNcwAycYaFzT:3:CL:6206:default"),
			KeyValuePair.Create("risk_monitoring_subscription_uri", "XvCtmx54WgYNcwAycYaFzT:3:CL:6206:default"),
		};

		var proofAttributes = rtgsNetworkParticipationProofAttributes.ToDictionary(
			pair => $"0_{pair.Key}_uuid",
			pair => new RequestedAttribute
			{
				Name = pair.Key,
				Restrictions = new List<RequestedClaimCredentialDefinition>
				{
					new RequestedClaimCredentialDefinition { CredentialDefinitionId = pair.Value }
				}
			});

		return proofAttributes;
	}
}
