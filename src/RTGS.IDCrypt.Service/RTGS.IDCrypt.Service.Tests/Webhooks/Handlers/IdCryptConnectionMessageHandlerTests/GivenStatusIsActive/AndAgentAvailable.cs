using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.Proof;
using RTGS.IDCryptSDK.Proof.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptConnectionMessageHandlerTests.GivenStatusIsActive;

public class AndAgentAvailable : IAsyncLifetime
{
	private readonly IdCryptConnection _activeConnection;
	private readonly Mock<IProofClient> _proofClientMock;
	private readonly IdCryptConnectionMessageHandler _handler;
	private SendProofRequestRequest _expectedRequest;

	public AndAgentAvailable()
	{
		_activeConnection = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "active"
		};

		_proofClientMock = new Mock<IProofClient>();

		SetupExpectedRequest();

		Func<SendProofRequestRequest, bool> requestMatches = request =>
		{
			request.Should().BeEquivalentTo(_expectedRequest);

			return true;
		};

		_proofClientMock
			.Setup(client => client.SendProofRequestAsync(
				It.Is<SendProofRequestRequest>(request => requestMatches(request)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var logger = new FakeLogger<IdCryptConnectionMessageHandler>();

		_handler = new IdCryptConnectionMessageHandler(logger, _proofClientMock.Object);
	}

	public async Task InitializeAsync()
	{
		var message = JsonSerializer.Serialize(_activeConnection);

		await _handler.HandleAsync(message, default);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenRequestProofAsyncWithExpected() =>
		_proofClientMock.Verify();

	private void SetupExpectedRequest()
	{
		_expectedRequest = new SendProofRequestRequest()
		{
			ConnectionId = _activeConnection.ConnectionId,
			Comment = "Requesting identification",
			RequestedProofDetails = new()
			{
				Name = "RTGS.global Network Participation",
				Version = "1.0",
				Attributes = new()
				{
					{
						"0_participant_uuid",
						new()
						{
							Name = "participant",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6153:default"
								}
							}
						}
					},
					{
						"0_RTGS_global_uuid",
						new()
						{
							Name = "RTGS_global",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6153:default"
								}
							}
						}
					},
					{
						"0_base_currency_uuid",
						new()
						{
							Name = "base_currency",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6153:default"
								}
							}
						}
					},
					{
						"0_parent_uuid",
						new()
						{
							Name = "parent",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6153:default"
								}
							}
						}
					},
					{
						"0_products_and_services_uuid",
						new()
						{
							Name = "products_and_services",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_category_uuid",
						new()
						{
							Name = "category",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_description_uuid",
						new()
						{
							Name = "description",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_full_legal_name_uuid",
						new()
						{
							Name = "full_legal_name",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_date_of_establishment_year_uuid",
						new()
						{
							Name = "date_of_establishment_year",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_identifier_RTGSg_uuid",
						new()
						{
							Name = "identifier_RTGSg",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_contacts_phone_uuid",
						new()
						{
							Name = "contacts_phone",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_establishment_country_name_uuid",
						new()
						{
							Name = "establishment_country_name",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_establishment_country_code_uuid",
						new()
						{
							Name = "establishment_country_code",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_business_line_uuid",
						new()
						{
							Name = "business_line",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_csleid_uuid",
						new()
						{
							Name = "csleid",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_identifier_LEI_uuid",
						new()
						{
							Name = "identifier_LEI",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_listing_status_uuid",
						new()
						{
							Name = "listing_status",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_aliases_uuid",
						new()
						{
							Name = "aliases",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_preferred_label_uuid",
						new()
						{
							Name = "preferred_label",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_contacts_country_name_uuid",
						new()
						{
							Name = "contacts_country_name",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_contacts_country_code_uuid",
						new()
						{
							Name = "contacts_country_code",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_contacts_email_uuid",
						new()
						{
							Name = "contacts_email",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_full_legal_name_local_uuid",
						new()
						{
							Name = "full_legal_name_local",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6194:default"
								}
							}
						}
					},
					{
						"0_idprefix_uuid",
						new()
						{
							Name = "idprefix",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6206:default"
								}
							}
						}
					},
					{
						"0_id_uuid",
						new()
						{
							Name = "id",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6206:default"
								}
							}
						}
					},
					{
						"0_risk_monitoring_status_uuid",
						new()
						{
							Name = "risk_monitoring_status",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6206:default"
								}
							}
						}
					},
					{
						"0_monitored_uuid",
						new()
						{
							Name = "monitored",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6206:default"
								}
							}
						}
					},
					{
						"0_risk_monitoring_subscription_uri_uuid",
						new()
						{
							Name = "risk_monitoring_subscription_uri",
							Restrictions = new()
							{
								new()
								{
									CredentialDefinitionId = "XvCtmx54WgYNcwAycYaFzT:3:CL:6206:default"
								}
							}
						}
					},
				},
				RequestedPredicates = new()
			}
		};
	}
}
