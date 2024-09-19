using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Random = System.Random;

namespace Tests
{
    public class KeyValueStoragePerformanceTests
    {
        private KeyValueStorage _storage;
        private string _testDirectory;
        private const int EntryCount = 10000;
        private const int UpdateCount = 5000;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Application.temporaryCachePath, "PerformanceTest");
            Directory.CreateDirectory(_testDirectory);
            _storage = new KeyValueStorage(_testDirectory, "performanceTest");
        }

        [TearDown]
        public void TearDown()
        {
            _storage.Dispose();
            Directory.Delete(_testDirectory, true);
        }

        [Test]
        public void TestWritePerformance()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < EntryCount; i++)
            {
                _storage.Set($"key{i}", $"value{i}");
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"写入 {EntryCount} 条数据耗时: {stopwatch.ElapsedMilliseconds} ms");
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 5000, "写入性能不符合预期");
        }

        [Test]
        public void TestReadPerformance()
        {
            // 首先写入数据
            for (int i = 0; i < EntryCount; i++)
            {
                _storage.Set($"key{i}", $"value{i}");
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < EntryCount; i++)
            {
                string value = _storage.GetString($"key{i}");
                Assert.AreEqual($"value{i}", value);
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"读取 {EntryCount} 条数据耗时: {stopwatch.ElapsedMilliseconds} ms");
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, "读取性能不符合预期");
        }

        [Test]
        public void TestUpdatePerformance()
        {
            // 首先写入数据
            for (int i = 0; i < EntryCount; i++)
            {
                _storage.Set($"key{i}", $"value{i}");
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < UpdateCount; i++)
            {
                _storage.Set($"key{i}", $"updatedValue{i}");
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"更新 {UpdateCount} 条数据耗时: {stopwatch.ElapsedMilliseconds} ms");
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 3000, "更新性能不符合预期");
        }

        [Test]
        public void TestLargeValuePerformance()
        {
            string largeValue = new string('a', 1000000); // 1MB 字符串

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _storage.Set("largeKey", largeValue);

            stopwatch.Stop();
            UnityEngine.Debug.Log($"写入 1MB 数据耗时: {stopwatch.ElapsedMilliseconds} ms");

            stopwatch.Restart();

            string retrievedValue = _storage.GetString("largeKey");

            stopwatch.Stop();
            UnityEngine.Debug.Log($"读取 1MB 数据耗时: {stopwatch.ElapsedMilliseconds} ms");

            Assert.AreEqual(largeValue, retrievedValue);
        }

        [Test]
        public void TestRandomAccessPerformance()
        {
            Random random = new Random();

            // 首先写入数据
            for (int i = 0; i < EntryCount; i++)
            {
                _storage.Set($"key{i}", $"value{i}");
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < 10000; i++)
            {
                int randomIndex = random.Next(0, EntryCount);
                string value = _storage.GetString($"key{randomIndex}");
                Assert.AreEqual($"value{randomIndex}", value);
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"随机读取 10000 次数据耗时: {stopwatch.ElapsedMilliseconds} ms");
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 500, "随机读取性能不符合预期");
        }

        [Test]
        public void TestMixedOperationsPerformance()
        {
            Random random = new Random();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < 10000; i++)
            {
                int operation = random.Next(3);
                int key = random.Next(1000);

                switch (operation)
                {
                    case 0: // 写入
                        _storage.Set($"key{key}", $"value{i}");
                        break;
                    case 1: // 读取
                        _storage.GetString($"key{key}");
                        break;
                    case 2: // 更新
                        _storage.Set($"key{key}", $"updatedValue{i}");
                        break;
                }
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"混合操作 10000 次耗时: {stopwatch.ElapsedMilliseconds} ms");
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 3000, "混合操作性能不符合预期");
        }

        [Test]
        public void TestContainsKeyPerformance()
        {
            // 首先写入数据
            for (int i = 0; i < EntryCount; i++)
            {
                _storage.Set($"key{i}", $"value{i}");
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int existCount = 0;
            int notExistCount = 0;

            for (int i = 0; i < EntryCount * 2; i++)
            {
                if (_storage.ContainsKey($"key{i}"))
                {
                    existCount++;
                }
                else
                {
                    notExistCount++;
                }
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"检查 {EntryCount * 2} 个键是否存在耗时: {stopwatch.ElapsedMilliseconds} ms");
            UnityEngine.Debug.Log($"存在的键: {existCount}, 不存在的键: {notExistCount}");
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, "ContainsKey 性能不符合预期");
            Assert.AreEqual(EntryCount, existCount, "存在的键数量不正确");
            Assert.AreEqual(EntryCount, notExistCount, "不存在的键数量不正确");
        }

        [Test]
        public void TestMixedOperationsWithContainsKeyPerformance()
        {
            Random random = new Random();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < 10000; i++)
            {
                int operation = random.Next(4);
                int key = random.Next(1000);

                switch (operation)
                {
                    case 0: // 写入
                        _storage.Set($"key{key}", $"value{i}");
                        break;
                    case 1: // 读取
                        _storage.GetString($"key{key}");
                        break;
                    case 2: // 更新
                        _storage.Set($"key{key}", $"updatedValue{i}");
                        break;
                    case 3: // 检查键是否存在
                        _storage.ContainsKey($"key{key}");
                        break;
                }
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"混合操作（包含 ContainsKey）10000 次耗时: {stopwatch.ElapsedMilliseconds} ms");
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 2000, "混合操作（包含 ContainsKey）性能不符合预期");
        }
    }
}
