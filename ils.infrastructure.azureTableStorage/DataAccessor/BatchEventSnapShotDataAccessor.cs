using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ils.core.Domain.Entities;
using ils.infrastructure.azureTableStorage.Client;
using ils.infrastructure.azureTableStorage.DataEntity;

namespace ils.infrastructure.DataAccessor
{
    internal class BatchEventSnapShotDataAccessor : TableStorageDataAccessorBase, IBatchEventSnapShotDataAccessor
    {
        public BatchEventSnapShotDataAccessor(IDatabaseClient azureTableStorageClient, string tableName) : base(azureTableStorageClient, tableName)
        {
        }

        /// <summary>
        /// 対象の日付のイベントのスナップショットを取得する
        /// </summary>
        /// <param name="targetDate">対象の日付、省略時には本日</param>
        /// <param name="baseTime">日付の開始時間をずらしたい場合に、ずらしたい時間(hour)を入れる。例えば前日23時～翌23時を取得範囲にしたい場合には-1</param>
        /// <returns></returns>
        /// <remarks>baseTimeは-23～23の範囲までしか設定できません</remarks>
        public async Task<IEnumerable<BatchEvent>> GetAsync(DateTime? targetDate = null, int baseTime = 0)
        {
            if (baseTime > 23 || baseTime < -23)
            {
                throw new Exception("baseTimeの指定が不正です。baseTimeは-23～23の範囲までしか設定できません");
            }
            var _t = (targetDate ?? DateTime.Now);

            var tableItems = new List<BatchEventTableEntity>();

            var resp = _tableClient.QueryAsync<BatchEventTableEntity>(x => x.PartitionKey == _t.ToString("yyyyMMdd"));
            await foreach (var item in resp)
            {
                tableItems.Add(item);
            }

            if (baseTime != 0)
            {
                var resp2 = _tableClient.QueryAsync<BatchEventTableEntity>(x => x.PartitionKey == _t.AddDays(baseTime > 0 ? 1 : -1).ToString("yyyyMMdd"));
                await foreach (var item in resp2)
                {
                    tableItems.Add(item);
                }
            }

            return GetSnapShots(tableItems);
        }

        /// <summary>
        /// 各Name毎に最新のEventのリストを取得する
        /// </summary>
        /// <param name="tableItems"></param>
        /// <returns></returns>
        internal IEnumerable<BatchEvent> GetSnapShots(List<BatchEventTableEntity> tableItems)
        {
            var tempDic = new Dictionary<string, List<BatchEventTableEntity>>();
            foreach (var item in tableItems)
            {
                if (!tempDic.ContainsKey(item.Name))
                {
                    tempDic.Add(item.Name, new List<BatchEventTableEntity>());
                }
                tempDic[item.Name].Add(item);
            }

            var result = new List<BatchEvent>();
            foreach (var key in tempDic.Keys)
            {
                result.Add(tempDic[key].OrderByDescending(v => v.DateTime).First().ToBatchEvent());
            }
            return result;
        }
    }

}

