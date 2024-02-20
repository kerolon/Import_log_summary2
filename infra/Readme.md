# deploy順番
1. keyvault.bicep
2. app.bicep

1.で作成したKeyVaultに対し、シークレットとして「tokenSecret」「addDataToken」を設定（十分に長い任意の文字列）
その後２．をデプロイする

# tips
```
Tenant ID, application ID, principal ID, and scope are not allowed to be updated. (Code:RoleAssignmentUpdateNotPermitted)  
```
のエラーが出た場合、FunctionがStorageにアクセスできずにうまく動かない問題が出ます。
その場合基本的には既に名前が同じだけど別の権限割り当て設定がある可能性が高いです。
（紐づいてるFunctionを消したりすると発生しがち）
その場合、対象を見つけて削除する or `name: guid(~)`の引数に追加で適当な文字を追加すると違うＩＤになって作成できるようになります。


Functionに設定されているkeyVaultReferenceIdentityがなぜか無視されることがあるので、その場合には手動でキーコンテナに対してシークレット参照権限を付与する