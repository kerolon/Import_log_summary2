using System;
using Azure;
using Azure.Data.Tables;
using ils.core.Domain.Entities;

namespace ils.infrastructure.azureTableStorage.DataEntity
{

    public class BatchEventTableEntity : BatchEvent, ITableEntity
    {
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public BatchEventTableEntity(BatchEvent e)
        {
            //PartitionKeyの単位でデータの配置が分散される。
            //だいたい日付単位でデータをまとめて取るはずなので、日付をPartitionKeyにすることで良い感じの分散＆データ取得の効率化を図る。
            //rowKeyはインデックスに相当するため検索キーを入れたいが、特に検索キーが無いのとPartitionKey+rowKeyで一意になる必要がある関係でGUIDでも入れておく
            PartitionKey = e.DateTime.ToString("yyyyMMdd");
            RowKey = Guid.NewGuid().ToString("N");
            Type = e.Type;
            Name = e.Name;
            Description= e.Description;
            DateTime = e.DateTime;
            From = e.From;
        }
        public BatchEventTableEntity()
        {
        }
        public BatchEvent ToBatchEvent()
        {
            return new BatchEvent()
            {
                Id = this.PartitionKey + this.RowKey,
                Type = this.Type,
                TypeString = this.Type.ToString(),
                Name = this.Name,
                Description = this.Description,
                DateTime = this.DateTime,
                DatetimeString = this.DateTime.ToString("yyyy/MM/dd HH:mm:ss"),
                From = this.From
            };
        }
    }

    internal static class BatchEventExtensions
    {
        public static BatchEventTableEntity ToTableEntity(this BatchEvent e)
        {
            return new BatchEventTableEntity(e);
        }
    }
}

