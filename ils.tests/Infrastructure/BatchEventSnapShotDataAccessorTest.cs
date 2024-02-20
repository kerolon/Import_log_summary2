using ils.infrastructure.azureTableStorage.Client;
using ils.infrastructure.azureTableStorage.DataEntity;
using ils.infrastructure.DataAccessor;
using Moq;

namespace ils.tests.Infrastructure
{
    internal class BatchEventSnapShotDataAccessorTest
    {
        [Test]
        public async Task Nameごとに最新の情報だけを取得できること()
        {
            var d = new BatchEventSnapShotDataAccessor(new Mock<IDatabaseClient>().Object, "test");

            //3種類のNameを用意
            var testDataName1 = "test1";
            var testDataName2 = "test2";
            var testDataName3 = "test3";


            var testData = new List<BatchEventTableEntity>();

            var now = DateTime.Now;

            //3種類のNameのデータを10個ずつ用意、それぞれのName_0が最新のデータのID
            foreach(var index in Enumerable.Range(0, 10))
            {
                var guid = Guid.NewGuid().ToString();
                testData.Add(new BatchEventTableEntity { PartitionKey = testDataName1, RowKey = "_" + index, Name = testDataName1, DateTime = now.AddSeconds(-1 * index), Description = guid });
            }
            foreach (var index in Enumerable.Range(0, 10))
            {
                var guid = Guid.NewGuid().ToString();
                testData.Add(new BatchEventTableEntity { PartitionKey = testDataName2, RowKey = "_" + index, Name = testDataName2, DateTime = now.AddSeconds(-1 * index), Description = guid });
            }
            foreach (var index in Enumerable.Range(0, 10))
            {
                var guid = Guid.NewGuid().ToString();
                testData.Add(new BatchEventTableEntity { PartitionKey = testDataName3, RowKey = "_" + index, Name = testDataName3, DateTime = now.AddSeconds(-1 * index), Description = guid });
            }

            //Guidが入ってるDescriptionでソート = ある程度順番がランダムになる
            testData =  testData.OrderBy(x => x.Description).ToList();

            var actualData = d.GetSnapShots(testData);

            //Nameごとに最新のデータだけが取得できていること
            Assert.That(actualData.Count(), Is.EqualTo(3));
            Assert.That(actualData, Has.One.Property("Id").EqualTo(testDataName1 + "_0"));
            Assert.That(actualData, Has.One.Property("Id").EqualTo(testDataName2 + "_0"));
            Assert.That(actualData, Has.One.Property("Id").EqualTo(testDataName3 + "_0"));
        }
    }
}
