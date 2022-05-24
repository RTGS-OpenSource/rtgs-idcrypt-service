param administratorLogin string
@secure()
param administratorLoginPassword string
param location string
param serverName string

resource server 'Microsoft.DBforPostgreSQL/servers@2017-12-01' = {
  location: location
  name: serverName
  properties: {
    createMode: 'Default'
    version: '11'
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    sslEnforcement: 'Disabled'
    storageProfile: {
      storageMB: 51200
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
      storageAutogrow: 'Disabled'
    }
    infrastructureEncryption: 'Disabled'
  }
  sku: {
    name: 'B_Gen5_1'
    tier: 'Basic'
    capacity: 1
    size: '51200'
    family: 'Gen5'
  }

  resource db 'databases' = {
    name: 'pgadmin'
    properties: {
      charset: 'utf8'
      collation: 'English_United States.1252'
    }
  }

  resource firewall 'firewallRules' = {
    name: 'allow-all'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '255.255.255.255'
    }
  }
}

output connectionString string = 'Server=tcp:${server.properties.fullyQualifiedDomainName},1433;Initial Catalog=${server.name};Persist Security Info=False;User ID=${administratorLogin};Password=${administratorLoginPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
output url string = '${server.properties.fullyQualifiedDomainName}:5432'
output user string = '${administratorLogin}@${server.name}'
