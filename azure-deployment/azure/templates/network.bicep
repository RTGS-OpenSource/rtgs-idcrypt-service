param location string
param vnetName string

resource vnet 'Microsoft.Network/virtualNetworks@2021-05-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.220.0.0/16'
      ]
    }
    enableDdosProtection: false
    enableVmProtection: false
    subnets: [
      {
        name: 'AksSubnet'
        properties: {
          addressPrefix: '10.220.0.0/21'
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
      {
        name: 'DbSubnet'
        properties: {
          addressPrefix: '10.220.8.0/24'
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
      {
        name: 'AppGwSubnet'
        properties: {
          addressPrefix: '10.220.10.0/24'
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
    ]
  }

  resource aksSubnet 'subnets' existing = {
    name: 'AksSubnet'
  }

  resource dbSubnet 'subnets' existing = {
    name: 'DbSubnet'
  }

  resource appGwSubnet 'subnets' existing = {
    name: 'AppGwSubnet'
  }
}

output aksSubnetId string = vnet::aksSubnet.id
output dbSubnetId string = vnet::dbSubnet.id
output appGwSubnetId string = vnet::appGwSubnet.id
