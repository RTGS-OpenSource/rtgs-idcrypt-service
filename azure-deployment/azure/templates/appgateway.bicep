param location string
param appGatewayName string
param subnetId string
param managedIdentityResourceId string
param logAnalyticsWorkspaceId string
param dnsPrefix string

var privateIpAddress = '10.220.10.4'

resource publicIp 'Microsoft.Network/publicIPAddresses@2021-05-01' = {
  name: '${appGatewayName}PublicIp'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    publicIPAllocationMethod: 'Static'
    dnsSettings: {
      domainNameLabel: dnsPrefix
    }
  }
}

resource appGateway 'Microsoft.Network/applicationGateways@2021-05-01' = {
  name: appGatewayName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityResourceId}': {}
    }
  }
  properties: {
    sku: {
      name: 'Standard_v2'
      tier: 'Standard_v2'
    }
    gatewayIPConfigurations: [
      {
        name: 'appGatewayIPConfiguration'
        properties: {
          subnet: {
            id: subnetId
          }
        }
      }
    ]
    frontendIPConfigurations: [
      {
        name: 'publicFrontendIPConfiguration'
        properties: {
          publicIPAddress: {
            id: publicIp.id
          }
        }
      }
      {
        name: 'privateFrontendIPConfiguration'
        properties: {
          privateIPAllocationMethod: 'Static'
          privateIPAddress: privateIpAddress
          subnet: {
            id: subnetId
          }
        }
      }
    ]
    frontendPorts: [
      {
        name: 'publicFrontendPort'
        properties: {
          port: 80
        }
      }
      {
        name: 'privateFrontendPort'
        properties: {
          port: 8080
        }
      }
    ]
    backendAddressPools: [
      {
        name: 'appGatewayBackendAddressPool'
      }
    ]
    backendHttpSettingsCollection: [
      {
        name: 'appGatewayBackendHttpSettings'
        properties: {
          port: 80
          protocol: 'Http'
        }
      }
    ]
    httpListeners: [
      {
        name: 'publicHttpListener'
        properties: {
          frontendIPConfiguration: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendIPConfigurations', appGatewayName, 'publicFrontendIPConfiguration')
          }
          frontendPort: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendPorts', appGatewayName, 'publicFrontendPort')
          }
          protocol: 'Http'
        }
      }
      {
        name: 'privateHttpListener'
        properties: {
          frontendIPConfiguration: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendIPConfigurations', appGatewayName, 'privateFrontendIPConfiguration')
          }
          frontendPort: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendPorts', appGatewayName, 'privateFrontendPort')
          }
          protocol: 'Http'
        }
      }
    ]
    requestRoutingRules: [
      {
        name: 'appGatewayRequestRoutingRule'
        properties: {
          ruleType: 'Basic'
          httpListener: {
            id: resourceId('Microsoft.Network/applicationGateways/httpListeners', appGatewayName, 'publicHttpListener')
          }
          backendAddressPool: {
            id: resourceId('Microsoft.Network/applicationGateways/backendAddressPools', appGatewayName, 'appGatewayBackendAddressPool')
          }
          backendHttpSettings: {
            id: resourceId('Microsoft.Network/applicationGateways/backendHttpSettingsCollection', appGatewayName, 'appGatewayBackendHttpSettings')
          }
        }
      }
    ]
    autoscaleConfiguration: {
      minCapacity: 1
      maxCapacity: 3
    }
  }
}

resource diagSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'writeToLogAnalytics'
  scope: appGateway
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        category: 'ApplicationGatewayAccessLog'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 20
        }
      }
      {
        category: 'ApplicationGatewayPerformanceLog'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 20
        }
      }
      {
        category: 'ApplicationGatewayFirewallLog'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 20
        }
      }
    ]
    metrics: [
      {
        enabled: true
        timeGrain: 'PT1M'
        retentionPolicy: {
          enabled: true
          days: 20
        }
      }
    ]
  }
}

output appGatewayName string = appGateway.name
output appGatewayResourceId string = appGateway.id
output publicFqdn string = publicIp.properties.dnsSettings.fqdn
