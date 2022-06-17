using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;

namespace RTGS.IDCrypt.Service.Extensions;

public static class ModelMappingExtensions
{
	public static CreateConnectionInvitationResponse MapToContract(
		this ConnectionInvitationBase model) =>
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

	public static T MapToConnectionInvitation<T>(
		this IDCryptSDK.Connections.Models.CreateConnectionInvitationResponse createConnectionInvitationResponse,
		string publicDid,
		string fromRtgsGlobalId) where T : ConnectionInvitationBase, new() =>
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
