param location string
param aksIdentityName string
param deploymentScriptIdentityName string
param appGatewayIdentityName string

resource aksIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: aksIdentityName
  location: location
}

resource deploymentScriptIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: deploymentScriptIdentityName
  location: location
}

resource appGatewayIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: appGatewayIdentityName
  location: location
}

output aksIdentityObjectId string = aksIdentity.properties.principalId
output aksIdentityTenantId string = aksIdentity.properties.tenantId
output aksIdentityResourceId string = aksIdentity.id
output deploymentScriptIdentityObjectId string = deploymentScriptIdentity.properties.principalId
output deploymentScriptIdentityTenantId string = deploymentScriptIdentity.properties.tenantId
output deploymentScriptIdentityResourceId string = deploymentScriptIdentity.id
output appGatewayIdentityName string = appGatewayIdentity.name
output appGatewayIdentityObjectId string = appGatewayIdentity.properties.principalId
output appGatewayIdentityClientId string = appGatewayIdentity.properties.clientId
output appGatewayIdentityResourceId string = appGatewayIdentity.id
