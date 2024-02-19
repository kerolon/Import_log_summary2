param location string = 'japaneast'
var identity_name = 'ils-apps-secret-keyvault-identity'
param keyvalut_name string = 'ils-apps-secret-keyvault'

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identity_name
  location: location
}

//認証関係の情報
resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyvalut_name
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    enabledForDeployment: true
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: identity.properties.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
  }
}

