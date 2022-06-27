using System.Collections.Generic;
using System.Text.Json;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.Proof;
using RTGS.IDCryptSDK.Proof.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class IdCryptConnectionMessageHandler : IMessageHandler
{
	private readonly ILogger<IdCryptConnectionMessageHandler> _logger;
	private readonly IProofClient _proofClient;
	private readonly IRtgsConnectionRepository _rtgsConnectionRepository;
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;

	public IdCryptConnectionMessageHandler(
		ILogger<IdCryptConnectionMessageHandler> logger,
		IProofClient proofClient,
		IRtgsConnectionRepository rtgsConnectionRepository,
		IBankPartnerConnectionRepository bankPartnerConnectionRepository)
	{
		_logger = logger;
		_proofClient = proofClient;
		_rtgsConnectionRepository = rtgsConnectionRepository;
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
	}

	public string MessageType => "connections";

	public async Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		var connection = JsonSerializer.Deserialize<IdCryptConnection>(jsonMessage);

		if (connection!.State is not "active")
		{
			_logger.LogDebug("Ignoring connection with alias {Alias} because state is {State}",
				connection.Alias, connection.State);

			return;
		}

		if (connection.TheirLabel.StartsWith("RTGS_Jurisdiction_Agent"))
		{
			await HandleJurisdictionConnection(connection, cancellationToken);

			return;
		}

		if (connection.TheirLabel.StartsWith("RTGS_Bank_Agent"))
		{
			await HandleBankConnection(connection, cancellationToken);
		}
	}

	private async Task HandleJurisdictionConnection(IdCryptConnection connection, CancellationToken cancellationToken) =>
		await _rtgsConnectionRepository.ActivateAsync(connection.ConnectionId, cancellationToken);

	private async Task HandleBankConnection(IdCryptConnection connection, CancellationToken cancellationToken)
	{
		var cycling = await _bankPartnerConnectionRepository.ActiveConnectionForBankExists(connection.Alias, cancellationToken);

		if (cycling)
		{
			await HandleCycledBankConnection(connection, cancellationToken);
		}
		else
		{
			await HandleInitialBankConnection(connection, cancellationToken);
		}
	}

	private async Task HandleCycledBankConnection(IdCryptConnection connection, CancellationToken cancellationToken) =>
		await _bankPartnerConnectionRepository.ActivateAsync(connection.ConnectionId, cancellationToken);

	private async Task HandleInitialBankConnection(IdCryptConnection connection, CancellationToken cancellationToken)
	{
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
			KeyValuePair.Create("participant", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:65212:default"),
			KeyValuePair.Create("RTGS_global", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:65212:default"),
			KeyValuePair.Create("base_currency", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:65212:default"),
			KeyValuePair.Create("parent", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:65212:default"),
			KeyValuePair.Create("products_and_services", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("category", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("description", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("full_legal_name", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("date_of_establishment_year", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("identifier_RTGSg", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("contacts_phone", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("establishment_country_name", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("establishment_country_code", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("business_line", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("csleid", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("identifier_LEI", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("listing_status", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("aliases", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("preferred_label", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("contacts_country_name", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("contacts_country_code", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("contacts_email", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("full_legal_name_local", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("idprefix", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("id", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("risk_monitoring_status", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("monitored", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default"),
			KeyValuePair.Create("risk_monitoring_subscription_uri", "RDmfeHMBEy7w8AQ7KXyGNi:3:CL:68627:default")
		};

		var proofAttributes = rtgsNetworkParticipationProofAttributes.ToDictionary(
			pair => $"0_{pair.Key}_uuid",
			pair => new RequestedAttribute
			{
				Name = pair.Key,
				Restrictions = new List<RequestedClaimCredentialDefinition>
				{
					new() { CredentialDefinitionId = pair.Value }
				}
			});

		return proofAttributes;
	}
}
