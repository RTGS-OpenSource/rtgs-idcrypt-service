using System.Collections.Generic;
using RTGS.IDCryptSDK.Proof.Models;

namespace RTGS.IDCrypt.Service.Extensions;

public static class ListExtensions
{
	public static Dictionary<string, RequestedAttribute> ToProofAttributes(this List<KeyValuePair<string, string>> keyValuePairs) =>
		keyValuePairs.ToDictionary(
			pair => $"0_{pair.Key}_uuid",
			pair => new RequestedAttribute
			{
				Name = pair.Key,
				Restrictions = new List<RequestedClaimCredentialDefinition>
				{
					new RequestedClaimCredentialDefinition { CredentialDefinitionId = pair.Value}
				}
			});
}
