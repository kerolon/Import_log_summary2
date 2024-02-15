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
        /// �Ώۂ̓��t�̃C�x���g�̃X�i�b�v�V���b�g���擾����
        /// </summary>
        /// <param name="targetDate">�Ώۂ̓��t�A�ȗ����ɂ͖{��</param>
        /// <param name="baseTime">���t�̊J�n���Ԃ����炵�����ꍇ�ɁA���炵��������(hour)������B�Ⴆ�ΑO��23���`��23�����擾�͈͂ɂ������ꍇ�ɂ�-1</param>
        /// <returns></returns>
        /// <remarks>baseTime��-23�`23�͈̔͂܂ł����ݒ�ł��܂���</remarks>
        public async Task<IEnumerable<BatchEvent>> GetAsync(DateTime? targetDate = null, int baseTime = 0)
        {
            if (baseTime > 23 || baseTime < -23)
            {
                throw new Exception("baseTime�̎w�肪�s���ł��BbaseTime��-23�`23�͈̔͂܂ł����ݒ�ł��܂���");
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
        /// �eName���ɍŐV��Event�̃��X�g���擾����
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

