namespace api
{
    public class AppSettings
    {
        /// <summary>
        /// Settingsの中のどのキーに接続文字列があるか
        /// </summary>
        public string SignalRConnectionStringSetting { get; set; }
        
        /// <summary>
        /// SignalRのHub名
        /// </summary>
        public string SignalRHubName { get; set; }
        public string GoogleLoginClientId { get; internal set; }
        public string TokenSecret { get; internal set; }
        public string AllowedDomain { get; internal set; }
        public string AddDataToken { get; internal set; }

        /// <summary>
        /// 基準時間、イベントデータ取得対象をこの値～+24hで取得する。例えば-1なら前日の23時から翌23時までのデータを取得する。
        /// </summary>
        public int BaseTime { get; internal set; }
    }
}
