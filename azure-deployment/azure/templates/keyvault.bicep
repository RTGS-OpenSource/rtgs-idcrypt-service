param location string
param keyVaultName string
param identityTenantId string
param identityObjectId string

@secure()
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
@secure()
param walletSeed string
@secure()
param walletName string
@secure()
param walletKey string
@secure()
param postgresUrl string
@secure()
param postgresUser string
@secure()
param postgresPassword string

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        objectId: identityObjectId
        tenantId: identityTenantId
        permissions: {
          keys: [
            'get'
          ]
          secrets: [
            'get'
          ]
        }
      }
    ]
  }

  resource secret_environment 'secrets' = {
    name: 'environment'
    properties: {
      value: environment
    }
  }

  resource secret_agentLabel 'secrets' = {
    name: 'agentLabel'
    properties: {
      value: agentLabel
    }
  }

  resource secret_agentEndpoint 'secrets' = {
    name: 'agentEndpoint'
    properties: {
      value: agentEndpoint
    }
  }
  resource secret_agentAdminApiKey 'secrets' = {
    name: 'agentAdminApiKey'
    properties: {
      value: agentAdminApiKey
    }
  }

  resource secret_idcryptToken 'secrets' = {
    name: 'idcryptToken'
    properties: {
      value: idcryptToken
    }
  }

  resource secret_idcryptApiKey 'secrets' = {
    name: 'idcryptApiKey'
    properties: {
      value: idcryptApiKey
    }
  }

  resource secret_idcryptCredentialDefinition 'secrets' = {
    name: 'idcryptCredentialDefinition'
    properties: {
      value: idcryptCredentialDefinition
    }
  }

  resource secret_idcryptOrganizationId 'secrets' = {
    name: 'idcryptOrganizationId'
    properties: {
      value: idcryptOrganizationId
    }
  }

  resource secret_walletSeed 'secrets' = {
    name: 'walletSeed'
    properties: {
      value: walletSeed
    }
  }

  resource secret_walletName 'secrets' = {
    name: 'walletName'
    properties: {
      value: walletName
    }
  }

  resource secret_walletKey 'secrets' = {
    name: 'walletKey'
    properties: {
      value: walletKey
    }
  }

  resource secret_postgresUrl 'secrets' = {
    name: 'postgresUrl'
    properties: {
      value: postgresUrl
    }
  }

  resource secret_postgresUser 'secrets' = {
    name: 'postgresUser'
    properties: {
      value: postgresUser
    }
  }

  resource secret_postgresPassword 'secrets' = {
    name: 'postgresPassword'
    properties: {
      value: postgresPassword
    }
  }
}

output keyVaultName string = keyVault.name
