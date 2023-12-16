// プログラムの構造は以下の記事を参考にしました。
// https://docs.microsoft.com/ja-jp/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures
// https://docs.microsoft.com/ja-jp/azure/architecture/best-practices/api-design

// ドメイン層
// ビジネスロジックやエンティティを定義する

// イベントのエンティティ
public class Event
{
    public string Id { get; set; } // Table StorageのRowKeyとして使用する
    public string Category { get; set; } // Table StorageのPartitionKeyとして使用する
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime Time { get; set; }
}

// イベントの集約結果のエンティティ
public class EventSummary
{
    public string Category { get; set; }
    public int Count { get; set; }
    public DateTime LatestTime { get; set; }
}

// イベントのリポジトリのインターフェース
// 永続化のストレージにアクセスするための抽象化
public interface IEventRepository
{
    // イベントを保存する
    Task SaveEventAsync(Event event);

    // カテゴリ別にイベントの集約結果を取得する
    Task<IEnumerable<EventSummary>> GetEventSummariesAsync();
}

// アプリケーション層
// ドメイン層とインフラストラクチャ層をつなぐ

// イベントのサービスのインターフェース
// ビジネスロジックを実装する
public interface IEventService
{
    // イベントを処理する
    Task ProcessEventAsync(Event event);

    // イベントの集約結果を通知する
    Task NotifyEventSummariesAsync();
}

// イベントのサービスの実装クラス
public class EventService : IEventService
{
    // リポジトリとメッセージングのサービスを依存性注入する
    private readonly IEventRepository _eventRepository;
    private readonly IMessagingService _messagingService;

    public EventService(IEventRepository eventRepository, IMessagingService messagingService)
    {
        _eventRepository = eventRepository;
        _messagingService = messagingService;
    }

    // イベントを処理する
    public async Task ProcessEventAsync(Event event)
    {
        // リポジトリを使ってイベントを保存する
        await _eventRepository.SaveEventAsync(event);

        // メッセージングのサービスを使ってイベントの集約結果を通知する
        await NotifyEventSummariesAsync();
    }

    // イベントの集約結果を通知する
    public async Task NotifyEventSummariesAsync()
    {
        // リポジトリを使ってイベントの集約結果を取得する
        var summaries = await _eventRepository.GetEventSummariesAsync();

        // メッセージングのサービスを使ってクライアントにイベントの集約結果を送信する
        await _messagingService.SendEventSummariesAsync(summaries);
    }
}

// インフラストラクチャ層
// 外部のサービスやリソースにアクセスするための実装

// イベントのリポジトリの実装クラス
// Table Storageにアクセスする
public class EventRepository : IEventRepository
{
    // Table Storageのクライアントを依存性注入する
    private readonly TableClient _tableClient;

    public EventRepository(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    // イベントを保存する
    public async Task SaveEventAsync(Event event)
    {
        // イベントをTableEntityに変換する
        var entity = new TableEntity(event.Category, event.Id)
        {
            {"Name", event.Name},
            {"Description", event.Description},
            {"Time", event.Time}
        };

        // Table Storageにエンティティを追加する
        await _tableClient.AddEntityAsync(entity);
    }

    // カテゴリ別にイベントの集約結果を取得する
    public async Task<IEnumerable<EventSummary>> GetEventSummariesAsync()
    {
        // 空のリストを作成する
        var summaries = new List<EventSummary>();

        // Table Storageからすべてのエンティティを取得する
        var entities = _tableClient.QueryAsync<TableEntity>();

        // カテゴリ別にエンティティをグループ化する
        var groups = entities.GroupBy(e => e.PartitionKey);

        // 各グループに対して集約結果を計算する
        foreach (var group in groups)
        {
            // カテゴリを取得する
            var category = group.Key;

            // イベントの数を取得する
            var count = group.Count();

            // 最新のイベントの時間を取得する
            var latestTime = group.Max(e => e["Time"]);

            // 集約結果を作成する
            var summary = new EventSummary
            {
                Category = category,
                Count = count,
                LatestTime = latestTime
            };

            // リストに追加する
            summaries.Add(summary);
        }

        // リストを返す
        return summaries;
    }
}

// メッセージングのサービスのインターフェース
// Azure SignalR Serviceにアクセスするための抽象化
public interface IMessagingService
{
    // クライアントにイベントの集約結果を送信する
    Task SendEventSummariesAsync(IEnumerable<EventSummary> summaries);
}

// メッセージングのサービスの実装クラス
// Azure SignalR Serviceにアクセスする
public class MessagingService : IMessagingService
{
    // Azure SignalR Serviceのクライアントを依存性注入する
    private readonly HubConnection _hubConnection;

    public MessagingService(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;
    }

    // クライアントにイベントの集約結果を送信する
    public async Task SendEventSummariesAsync(IEnumerable<EventSummary> summaries)
    {
        // Azure SignalR Serviceのハブに接続する
        await _hubConnection.StartAsync();

        // クライアントにイベントの集約結果をブロードキャストする
        await _hubConnection.SendAsync("SendEventSummaries", summaries);

        // Azure SignalR Serviceのハブから
