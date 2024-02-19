# deploy順番
1. keyvault.bicep
2. app.bicep

1.で作成したKeyVaultに対し、シークレットとして「tokenSecret」「addDataToken」を設定（十分に長い任意の文字列）
その後２．をデプロイする
