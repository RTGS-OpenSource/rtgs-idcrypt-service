param appGatewayName string
param appGatewayIdentityName string
param deploymentScriptIdentityObjectId string
param agicIdentityObjectId string

var aksClusterUserRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4abbcc35-e782-43d8-92c5-2d3f1bd2253f')
var managedIdentityOperatorRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'f1a07417-d97a-45cb-824c-7a7467783830')
var contributorRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
var readerRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'acdd72a7-3385-48ef-bd42-f606fba81ae7')

// This role assignment allows the Deployment Script identity to access AKS and install the agent and other dependencies
resource deploymentScriptIdentityAksClusterUserRole 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid('deploymentScriptIdentityAksClusterUserRole', resourceGroup().id)
  scope: resourceGroup()
  properties: {
    principalId: deploymentScriptIdentityObjectId
    principalType: 'ServicePrincipal'
    roleDefinitionId: aksClusterUserRole
  }
}

//This Role Assignment allows the Deployment Script to read details about the Key Vault, Identities and other resources deployed
resource deploymentScriptIdentityReader 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid('deploymentScriptIdentityReader', resourceGroup().id)
  scope: resourceGroup()
  properties: {
    principalId: deploymentScriptIdentityObjectId
    principalType: 'ServicePrincipal'
    roleDefinitionId: readerRole
  }
}

resource appGateway 'Microsoft.Network/applicationGateways@2021-05-01' existing = {
  name: appGatewayName
}

// AGIC identity needs Contributor role on the Application Gateway resource
resource agicIdentityContributorRole 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid('agicIdentityContributorRole', resourceGroup().id)
  scope: appGateway
  properties: {
    principalId: agicIdentityObjectId
    roleDefinitionId: contributorRole
  }
}

// AGIC identity needs Reader role on the resource group
resource agicIdentityReaderRole 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid('agicIdentityReaderRole', resourceGroup().id)
  scope: resourceGroup()
  properties: {
    principalId: agicIdentityObjectId
    roleDefinitionId: readerRole
  }
}

resource appGatewayIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: appGatewayIdentityName
}

// AGIC identity needs to managed App Gateway identity
resource agicIdentityManagedIdentityOperatorRole 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid('agicIdentityManagedIdentityOperatorRole', resourceGroup().id)
  scope: appGatewayIdentity
  properties: {
    principalId: agicIdentityObjectId
    principalType: 'ServicePrincipal'
    roleDefinitionId: managedIdentityOperatorRole
  }
}
