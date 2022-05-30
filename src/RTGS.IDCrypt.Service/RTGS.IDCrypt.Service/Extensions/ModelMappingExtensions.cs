using RTGS.IDCrypt.Service.Contracts.Connection;

namespace RTGS.IDCrypt.Service.Extensions;

public static class ModelMappingExtensions
{
	public static CreateConnectionInvitationResponse MapToContract(
		this Models.ConnectionInvitation model) =>
			new()
			{
				FromRtgsGlobalId = model.FromRtgsGlobalId,
				AgentPublicDid = model.PublicDid,
				Alias = model.Alias,
				InvitationUrl = model.InvitationUrl,
				Invitation = new ConnectionInvitation
				{
					Did = model.Did,
					Type = model.Type,
					Label = model.Label,
					ImageUrl = model.ImageUrl,
					RecipientKeys = model.RecipientKeys,
					ServiceEndpoint = model.ServiceEndpoint,
					Id = model.Id
				}
			};

	public static Models.ConnectionInvitation MapToConnectionInvitation(
		this IDCryptSDK.Connections.Models.CreateConnectionInvitationResponse createConnectionInvitationResponse,
		string publicDid,
		string fromRtgsGlobalId) =>
			new()
			{
				Type = createConnectionInvitationResponse.Invitation.Type,
				Alias = createConnectionInvitationResponse.Alias,
				Label = createConnectionInvitationResponse.Invitation.Label,
				RecipientKeys = createConnectionInvitationResponse.Invitation.RecipientKeys,
				ServiceEndpoint = createConnectionInvitationResponse.Invitation.ServiceEndpoint,
				Id = createConnectionInvitationResponse.Invitation.Id,
				PublicDid = publicDid,
				Did = createConnectionInvitationResponse.Invitation.Did,
				ImageUrl = createConnectionInvitationResponse.Invitation.ImageUrl,
				InvitationUrl = createConnectionInvitationResponse.InvitationUrl,
				FromRtgsGlobalId = fromRtgsGlobalId
			};
}
