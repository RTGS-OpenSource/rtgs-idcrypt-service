using RTGS.IDCrypt.Service.Contracts.Connection;

namespace RTGS.IDCrypt.Service.Extensions;

public static class ModelMappingExtensions
{
	public static CreateConnectionInvitationResponse MapToContract(
		this Models.ConnectionInvitation model) =>
			new()
			{
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
}
