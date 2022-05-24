targetScope = 'resourceGroup'

param location string = resourceGroup().location
param vmSize string = 'Standard_D4ds_v4'
param deploymentScriptContent string = loadFileAsBase64('install.sh')

@allowed([
  'dev'
  'prod'
])
param environment string
@secure()
param agentLabel string
@secure()
param agentEndpoint string
@secure()
param agentAdminApiKey string
@secure()
param idcryptToken string
@secure()
param idcryptApiKey string
@secure()
param idcryptCredentialDefinition string
@secure()
param idcryptOrganizationId string
param webhookUrl string
@secure()
param walletSeed string
@secure()
param walletName string
@secure()
param walletKey string

var appName = 'IdCryptAgent'
var uniqueAppName = '${toLower(appName)}${uniqueString(resourceGroup().id)}'
var uniqueTruncatedAppName = (length(uniqueAppName) > 24) ? substring(uniqueAppName, 0, 24) : uniqueAppName
var aksNodeResouceGroup = '${resourceGroup().name}-aks'
var postgresUser = 'pgadmin'
var postgresPassword = 'Passw0rd'
var dnsPrefix = uniqueAppName

module analytics 'analytics.bicep' = {
  name: '${appName}Analytics'
  params: {
    location: location
    logAnalyticsWorkspaceName: '${appName}LogAnalyticsWorkspace'
  }
}

module storage 'storage.bicep' = {
  name: '${appName}Storage'
  params: {
    location: location
    storageAccountName: uniqueTruncatedAppName
  }
}

module identity 'identity.bicep' = {
  name: '${appName}Identity'
  params: {
    location: location
    deploymentScriptIdentityName: '${appName}DeploymentScriptIdentity'
    aksIdentityName: '${appName}AksIdentity'
    appGatewayIdentityName: '${appName}AppGatewayIdentity'
  }
}

module keyvault 'keyvault.bicep' = {
  name: '${appName}KeyVault'
  params: {
    location: location
    keyVaultName: uniqueTruncatedAppName
    identityObjectId: identity.outputs.deploymentScriptIdentityObjectId
    identityTenantId: identity.outputs.deploymentScriptIdentityTenantId
    environment: environment
    agentLabel: agentLabel
    agentEndpoint: agentEndpoint
    agentAdminApiKey: agentAdminApiKey
    idcryptToken: idcryptToken
    idcryptApiKey: idcryptApiKey
    idcryptCredentialDefinition: idcryptCredentialDefinition
    idcryptOrganizationId: idcryptOrganizationId
    walletSeed: walletSeed
    walletName: walletName
    walletKey: walletKey
    postgresUrl: postgres.outputs.url
    postgresUser: postgres.outputs.user
    postgresPassword: postgresPassword
  }
}

module network 'network.bicep' = {
  name: '${appName}Network'
  params: {
    location: location
    vnetName: '${appName}Vnet'
  }
}

module postgres 'postgres.bicep' = {
  name: '${appName}Postgres'
  params: {
    location: location
    serverName: uniqueTruncatedAppName
    administratorLogin: postgresUser
    administratorLoginPassword: postgresPassword
  }
}

module appGateway 'appgateway.bicep' = {
  name: '${appName}AppGateway'
  params: {
    location: location
    appGatewayName: '${appName}AppGateway'
    subnetId: network.outputs.appGwSubnetId
    managedIdentityResourceId: identity.outputs.appGatewayIdentityResourceId
    logAnalyticsWorkspaceId: analytics.outputs.logAnalyticsWorkspaceId
    dnsPrefix: dnsPrefix
  }
}

module aks 'aks.bicep' = {
  name: '${appName}Aks'
  params: {
    location: location
    clusterName: '${appName}AksCluster'
    logAnalyticsWorkspaceId: analytics.outputs.logAnalyticsWorkspaceId
    managedIdentityResourceId: identity.outputs.aksIdentityResourceId
    nodeResourceGroup: aksNodeResouceGroup
    subnetId: network.outputs.aksSubnetId
    vmSize: vmSize
    appGatewayResourceId: appGateway.outputs.appGatewayResourceId
  }
}

module roleAssignments 'roleassignments.bicep' = {
  name: '${appName}RoleAssignments'
  params: {
    appGatewayName: appGateway.outputs.appGatewayName
    appGatewayIdentityName: identity.outputs.appGatewayIdentityName
    deploymentScriptIdentityObjectId: identity.outputs.deploymentScriptIdentityObjectId
    agicIdentityObjectId: aks.outputs.agicIdentityObjectId
  }
}

module roleAssignmentsAksGroup 'roleassignments-aks.bicep' = {
  name: '${appName}RoleAssignmentsAksGroup'
  scope: resourceGroup(aksNodeResouceGroup)
  params: {
    deploymentScriptIdentityObjectId: identity.outputs.deploymentScriptIdentityObjectId
  }
  dependsOn: [
    aks
  ]
}

module deploymentScript 'deploymentscript.bicep' = {
  name: '${appName}DeploymentScript'
  params: {
    location: location
    deploymentScriptName: '${appName}DeploymentScript'
    managedIdentityResourceId: identity.outputs.deploymentScriptIdentityResourceId
    deploymentScriptContent: deploymentScriptContent
    clusterName: aks.outputs.clusterName
    keyVaultName: keyvault.outputs.keyVaultName
    kubeletIdentityClientId: aks.outputs.kubeletIdentityClientId
    publicFqdn: appGateway.outputs.publicFqdn
  }
  dependsOn: [
    roleAssignments
    roleAssignmentsAksGroup
  ]
}
