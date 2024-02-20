# Import_log_summary2

# 設定方法
1. 「Google でログイン」を使用するため、[Google API Console](https://console.developers.google.com/apis?hl=ja)でOAuth2.0クライアントIDを作成する
   - 参考：https://developers.google.com/identity/gsi/web/guides/get-google-api-clientid?hl=ja
1. Azriteをインストールして起動
   - 参考：https://learn.microsoft.com/ja-jp/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage
   - 起動したらTableStorageにアクセスして`BatchEvent`というテーブルを作成しておく
1. Azure SignalR ローカル エミュレーターをダウンロード
```
dotnet tool install  -g Microsoft.Azure.SignalR.Emulator
```
4. 設定ファイルのあるフォルダに移動してAzure SignalR ローカルエミュレータを起動
```
cd azure_signalR_emulator
asrs-emulator start
```
5. 起動画面に表示されるConnectionStringをコピペ（Endpoint=http://～で始まる文字列）
6. webapi/api/_sample.settings.jsonをコピペしてlocal.setting.jsonを作成し、以下を設定
    - AzureWebJobsStorage：UseDevelopmentStorage=true ※Azriteを使う設定
    - StorageConnectionString：UseDevelopmentStorage=true ※Azriteを使う設定
    - AzureSignalRConnectionString：上記でコピペしたAzure SignalR ローカル エミュレーターへの接続文字列を設定
    - GoogleLoginClientId：上記で作成したOAuth2.0クライアントIDの「クライアントID」を設定
    - AllowedDomain：ログイン可能とするGoogleアカウントのドメインを指定
    - TokenSecret：任意の長い文字列を設定
    - AddDataToken：任意の長い文字列を設定
7. web/Website/website.client/.env(または.env.dev)に、`VITE_ENV_CLIENT_ID=（GoogleLoginClientIdと同じ文字列）` を指定

