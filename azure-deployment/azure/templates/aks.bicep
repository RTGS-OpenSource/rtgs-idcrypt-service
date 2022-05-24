param location string
param clusterName string
param vmSize string
param nodeResourceGroup string
param managedIdentityResourceId string
param subnetId string
param logAnalyticsWorkspaceId string
param appGatewayResourceId string

var kubernetesVersion = '1.22.6'
var clusterSize = 1
var dnsPrefix = toLower(clusterName)

resource aks 'Microsoft.ContainerService/managedClusters@2022-01-01' = {
  name: clusterName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityResourceId}': {}
    }
  }
  properties: {
    kubernetesVersion: kubernetesVersion
    nodeResourceGroup: nodeResourceGroup
    dnsPrefix: dnsPrefix
    agentPoolProfiles: [
      {
        name: 'system'
        count: clusterSize
        enableAutoScaling: false
        vmSize: vmSize
        mode: 'System'
        type: 'VirtualMachineScaleSets'
        availabilityZones: [
          '1'
          '2'
          '3'
        ]
        vnetSubnetID: subnetId
      }
    ]
    addonProfiles: {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: logAnalyticsWorkspaceId
        }
      }
      ingressApplicationGateway: {
        config: {
          applicationGatewayId: appGatewayResourceId
        }
        enabled: true
      }
    }
    enableRBAC: true
    networkProfile: {
      networkPlugin: 'azure'
      loadBalancerSku: 'standard'
    }
    // podIdentityProfile: {
    //   enabled: true
    //   userAssignedIdentities: [
    //     {
    //       name: 'agent'
    //       identity: {
    //         clientId: agentIdentityClientId
    //         objectId: agentIdentityObjectId
    //         resourceId: agentIdentityResourceId
    //       }
    //       namespace: 'default'
    //     }
    //   ]
    // }
  }
}

output clusterName string = aks.name
output nodeResourceGroup string = aks.properties.nodeResourceGroup
output kubeletIdentityObjectId string = aks.properties.identityProfile.kubeletidentity.objectId
output kubeletIdentityClientId string = aks.properties.identityProfile.kubeletidentity.clientId
output kubeletIdentityResourceId string = aks.properties.identityProfile.kubeletidentity.resourceId
output agicIdentityObjectId string = aks.properties.addonProfiles.ingressApplicationGateway.identity.objectId
