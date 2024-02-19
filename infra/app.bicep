// リソース名
param function_name string = 'ils-api'
param storageAccount_name string = 'ilsstorage'
param function_hostingPlanName string = 'ils-api-host'
param signalr_name string = 'ils-azsignalr'

//シークレット関係
param keyvalut_name string = 'ils-apps-secret-keyvault'
var keyvault_access_identity_name = 'ils-apps-secret-keyvault-identity'
param key_name_jwttoken string = 'tokenSecret'
param key_name_addDataToken string = 'addDataToken'

param location string = 'japaneast'

//リソースのスペック関連
param function_linuxFxVersion string = 'DOTNET|6.0'
param function_sku string = 'Dynamic'
param function_skuCode string = 'Y1'
param signalr_skuName string = 'Free_F1'
param signalr_tier string = 'Free'
param signalr_capacity int = 1

param googleLoginClientId string = ''
param googleLoginAllowedDomain string = 'pa-consul.co.jp'

resource keyvault_access_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing ={
  name: keyvault_access_identity_name
}

//API作成
resource function 'Microsoft.Web/sites@2023-01-01' = {
  name: function_name
  kind: 'functionapp,linux'
  location: location
  tags: {}
  properties: {
    keyVaultReferenceIdentity: keyvault_access_identity.id
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount_name};AccountKey=${listKeys(storageAccount.id, '2019-06-01').keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'AzureSignalRServiceTransportType'
          value: 'Transient'
        }
        {
          name: 'AzureSignalRConnectionString'
          value: 'Endpoint=https://${signalr_name}.service.signalr.net;AuthType=azure.msi;Version=1.0;'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'WEBSITE_TIME_ZONE'
          value: 'Tokyo Standard Time'
        }
        {
          name: 'GoogleLoginClientId'
          value: googleLoginClientId
        }
        {
          name: 'AllowedDomain'
          value: googleLoginAllowedDomain
        }
        {
          name: 'TokenSecret'
          value: '@Microsoft.KeyVault(VaultName=${keyvalut_name};SecretName=${key_name_jwttoken})'
        }
        {
          name: 'StorageConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount_name};AccountKey=${listKeys(storageAccount.id, '2019-06-01').keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'BaseTime'
          value: '0'
        }
        {
          name: 'AddDataToken'
          value: '@Microsoft.KeyVault(VaultName=${keyvalut_name};SecretName=${key_name_addDataToken})'
        }
      ]
      cors: {
        allowedOrigins: [
          'https://portal.azure.com'
        ]
      }
      use32BitWorkerProcess: true
      ftpsState: 'disabled'
      linuxFxVersion: function_linuxFxVersion
      keyVaultReferenceIdentity: keyvault_access_identity.id
    }    
    clientAffinityEnabled: false
    httpsOnly: true
    serverFarmId: hostingPlan.id
  }
  identity: {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: {
      '${keyvault_access_identity.id}': {}
    }
  }  
  dependsOn: [
  ]
}

//APIのホスティングサーバ
resource hostingPlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: function_hostingPlanName
  location: location
  kind: 'linux'
  tags: {}
  properties: {
    reserved: true
  }
  sku: {
    tier: function_sku
    name: function_skuCode
  }
  dependsOn: []
}

//ストレージアカウントはFunctionにも使うしFunctionで動くアプリでも使う
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: storageAccount_name
  location: location
  tags: {}
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    defaultToOAuthAuthentication: true
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
  kind: 'StorageV2'
}

//テーブルストレージをアプリで使う
resource apps_tablestorage 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount  
}
//実際に使うテーブル
resource apps_table 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-04-01' = {
  name: 'BatchEvent'
  parent: apps_tablestorage
}

//FunctionのManaged Identity. SignalRに接続するために使う
resource function_identity 'Microsoft.ManagedIdentity/identities@2018-11-30' existing = {
  scope: function
  name: 'default'
}

//SignalRの作成
resource signalr 'Microsoft.SignalRService/SignalR@2023-08-01-preview' = {
  name: signalr_name
  location: location
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
      {
        flag: 'EnableConnectivityLogs'
        value: 'true'
      }
    ]
    upstream: {
      templates: [
        {
          hubPattern: '*'
          eventPattern: '*'
          categoryPattern: '*'
          urlTemplate: 'https://${function.properties.defaultHostName}/runtime/webhooks/signalr'
          auth: {
            type: 'ManagedIdentity'
            managedIdentity: {
              resource: function_identity.properties.clientId
            }
          }
        }
      ]
    }
    cors: {
      allowedOrigins: [
        '*'
      ]
    }
    tls: {
      clientCertEnabled: false
    }
  }
  sku: {
    name: signalr_skuName
    tier: signalr_tier
    capacity: signalr_capacity
  }
  identity: {
    type: 'SystemAssigned'
  }
  tags: {}
  dependsOn: []
}

// Functionがテーブルにアクセスできるようにする
var tableStorageAdmin_roleDefinitionId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
resource tableStorageAdmin_roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(function.id,apps_table.id,tableStorageAdmin_roleDefinitionId, resourceGroup().id)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', tableStorageAdmin_roleDefinitionId)
    principalId: function.identity.principalId
    principalType: 'ServicePrincipal'
  }
  scope: apps_table
}
