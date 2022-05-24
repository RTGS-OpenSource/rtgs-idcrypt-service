param deploymentScriptIdentityObjectId string

var networkContributorRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4d97b98b-1d4f-4787-a291-c67834d212e7')

// This role assignment is needed for the Deployment Script identity to list and update Public IPs on the AKS resource group.
resource deploymentScriptIdentityNetworkContributor 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid('deploymentScriptIdentityNetworkContributor', resourceGroup().id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: networkContributorRole
    principalId: deploymentScriptIdentityObjectId
    principalType: 'ServicePrincipal'
  }
}
