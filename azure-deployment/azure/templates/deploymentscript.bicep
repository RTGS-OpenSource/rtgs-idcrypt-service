param location string
param deploymentScriptName string
param deploymentScriptContent string
param managedIdentityResourceId string
param clusterName string
param keyVaultName string
param kubeletIdentityClientId string
param publicFqdn string

param utcValue string = utcNow()

resource deploymentScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: deploymentScriptName
  location: location
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityResourceId}': {}
    }
  }
  properties: {
    azCliVersion: '2.34.1'
    retentionInterval: 'P1D'
    timeout: 'PT60M'
    cleanupPreference: 'OnExpiration'
    forceUpdateTag: utcValue
    scriptContent: base64ToString(deploymentScriptContent)
    environmentVariables: [
      {
        name: 'TENANT_ID'
        value: subscription().tenantId
      }
      {
        name: 'RESOURCE_GROUP_NAME'
        value: resourceGroup().name
      }
      {
        name: 'CLUSTER_NAME'
        value: clusterName
      }
      {
        name: 'KUBELET_IDENTITY_CLIENT_ID'
        value: kubeletIdentityClientId
      }
      {
        name: 'KEYVAULT_NAME'
        value: keyVaultName
      }
      {
        name: 'PUBLIC_FQDN'
        value: publicFqdn
      }
    ]
    arguments: ''
  }
}
