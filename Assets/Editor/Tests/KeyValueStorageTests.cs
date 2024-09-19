using NUnit.Framework;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class KeyValueStorageTests
    {
        private KeyValueStorage _storage;
        private string _testDirectory;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine("Storages", "TestKeyValueStorage");
            Directory.CreateDirectory(_testDirectory);
            _storage = new KeyValueStorage(_testDirectory, "test");
        }

        [TearDown]
        public void TearDown()
        {
            _storage.Dispose();
            Directory.Delete(_testDirectory, true);
        }

        [Test]
        public void TestSetAndGetInt()
        {
            _storage.Set("intKey", 42);
            Assert.AreEqual(42, _storage.GetInt("intKey"));
        }

        [Test]
        public void TestSetAndGetLong()
        {
            _storage.Set("longKey", 9223372036854775807);
            Assert.AreEqual(9223372036854775807, _storage.GetLong("longKey"));
        }

        [Test]
        public void TestSetAndGetFloat()
        {
            _storage.Set("floatKey", 3.14f);
            Assert.AreEqual(3.14f, _storage.GetFloat("floatKey"));
        }

        [Test]
        public void TestSetAndGetDouble()
        {
            _storage.Set("doubleKey", 3.14159265359);
            Assert.AreEqual(3.14159265359, _storage.GetDouble("doubleKey"));
        }

        [Test]
        public void TestSetAndGetBool()
        {
            _storage.Set("boolKey", true);
            Assert.IsTrue(_storage.GetBool("boolKey"));
        }

        [Test]
        public void TestSetAndGetByte()
        {
            _storage.Set("byteKey", (byte)255);
            Assert.AreEqual(255, _storage.GetByte("byteKey"));
        }

        [Test]
        public void TestSetAndGetShort()
        {
            _storage.Set("shortKey", (short)32767);
            Assert.AreEqual(32767, _storage.GetShort("shortKey"));
        }

        [Test]
        public void TestSetAndGetUInt()
        {
            _storage.Set("uintKey", 4294967295);
            Assert.AreEqual(4294967295, _storage.GetUInt("uintKey"));
        }

        [Test]
        public void TestSetAndGetULong()
        {
            _storage.Set("ulongKey", 18446744073709551615);
            Assert.AreEqual(18446744073709551615, _storage.GetULong("ulongKey"));
        }

        [Test]
        public void TestSetAndGetUShort()
        {
            _storage.Set("ushortKey", (ushort)65535);
            Assert.AreEqual(65535, _storage.GetUShort("ushortKey"));
        }

        [Test]
        public void TestSetAndGetString()
        {
            _storage.Set("stringKey", "Hello, World!");
            Assert.AreEqual("Hello, World!", _storage.GetString("stringKey"));
        }

        [Test]
        public void TestSetAndGetCustomClass()
        {
            var myClass = new MyClass { Id = 1, Name = "TestClass" };
            _storage.Set("myClassKey", myClass);
            var retrievedClass = _storage.Get<MyClass>("myClassKey");
            Assert.AreEqual(1, retrievedClass.Id);
            Assert.AreEqual("TestClass", retrievedClass.Name);
        }

        [Test]
        public void TestOverwriteValue()
        {
            _storage.Set("overwriteKey", 10);
            Assert.AreEqual(10, _storage.GetInt("overwriteKey"));

            _storage.Set("overwriteKey", 20);
            Assert.AreEqual(20, _storage.GetInt("overwriteKey"));
        }

        [Test]
        public void TestNonExistentKey()
        {
            Assert.AreEqual(0, _storage.GetInt("nonExistentKey"));
            Assert.AreEqual(0L, _storage.GetLong("nonExistentKey"));
            Assert.AreEqual(0f, _storage.GetFloat("nonExistentKey"));
            Assert.AreEqual(0d, _storage.GetDouble("nonExistentKey"));
            Assert.IsFalse(_storage.GetBool("nonExistentKey"));
            Assert.AreEqual(0, _storage.GetByte("nonExistentKey"));
            Assert.AreEqual(0, _storage.GetShort("nonExistentKey"));
            Assert.AreEqual(0U, _storage.GetUInt("nonExistentKey"));
            Assert.AreEqual(0UL, _storage.GetULong("nonExistentKey"));
            Assert.AreEqual(0, _storage.GetUShort("nonExistentKey"));
            Assert.AreEqual(string.Empty, _storage.GetString("nonExistentKey"));
            Assert.IsNull(_storage.Get<MyClass>("nonExistentKey"));
        }

        [Test]
        public void TestLargeString()
        {
            var largeString = new string('a', 1000000);
            _storage.Set("largeStringKey", largeString);
            Assert.AreEqual(largeString, _storage.GetString("largeStringKey"));
        }

        [Test]
        public void TestMultipleOperations()
        {
            // 设置初始值
            _storage.Set("intKey", 1);
            _storage.Set("longKey", 1000000000000L);
            _storage.Set("floatKey", 3.14f);
            _storage.Set("doubleKey", 3.14159265359);
            _storage.Set("boolKey", true);
            _storage.Set("shortString", "abc");
            _storage.Set("mediumString", "Hello, World!");
            _storage.Set("longString", new string('a', 1000));
            _storage.Set("unicodeString", "你好世界");

            // 验证初始值
            Assert.AreEqual(1, _storage.GetInt("intKey"));
            Assert.AreEqual(1000000000000L, _storage.GetLong("longKey"));
            Assert.AreEqual(3.14f, _storage.GetFloat("floatKey"));
            Assert.AreEqual(3.14159265359, _storage.GetDouble("doubleKey"));
            Assert.IsTrue(_storage.GetBool("boolKey"));
            Assert.AreEqual("abc", _storage.GetString("shortString"));
            Assert.AreEqual("Hello, World!", _storage.GetString("mediumString"));
            Assert.AreEqual(new string('a', 1000), _storage.GetString("longString"));
            Assert.AreEqual("你好世界", _storage.GetString("unicodeString"));

            // 覆盖值
            _storage.Set("intKey", 2);
            _storage.Set("longKey", 2000000000000L);
            _storage.Set("floatKey", 2.718f);
            _storage.Set("doubleKey", 2.718281828459);
            _storage.Set("boolKey", false);
            _storage.Set("shortString", "xyz");
            _storage.Set("mediumString", "Goodbye, World!");
            _storage.Set("longString", new string('b', 2000));
            _storage.Set("unicodeString", "再见世界");

            // 验证覆盖后的值
            Assert.AreEqual(2, _storage.GetInt("intKey"));
            Assert.AreEqual(2000000000000L, _storage.GetLong("longKey"));
            Assert.AreEqual(2.718f, _storage.GetFloat("floatKey"));
            Assert.AreEqual(2.718281828459, _storage.GetDouble("doubleKey"));
            Assert.IsFalse(_storage.GetBool("boolKey"));
            Assert.AreEqual("xyz", _storage.GetString("shortString"));
            Assert.AreEqual("Goodbye, World!", _storage.GetString("mediumString"));
            Assert.AreEqual(new string('b', 2000), _storage.GetString("longString"));
            Assert.AreEqual("再见世界", _storage.GetString("unicodeString"));

            // 多次覆盖不同长度的字符串
            _storage.Set("shortString", "a very long string that is much longer than the original");
            _storage.Set("mediumString", "short");
            _storage.Set("longString", "medium length string");
            _storage.Set("unicodeString", "混合Unicode和ASCII字符");

            // 验证多次覆盖后的值
            Assert.AreEqual("a very long string that is much longer than the original", _storage.GetString("shortString"));
            Assert.AreEqual("short", _storage.GetString("mediumString"));
            Assert.AreEqual("medium length string", _storage.GetString("longString"));
            Assert.AreEqual("混合Unicode和ASCII字符", _storage.GetString("unicodeString"));

            // 再次覆盖，这次使用空字符串和非常长的字符串
            _storage.Set("shortString", "");
            _storage.Set("mediumString", new string('x', 10000));
            _storage.Set("longString", "🌟🌠✨"); // 使用 emoji
            _storage.Set("unicodeString", "Just ASCII now");

            // 验证最终的值
            Assert.AreEqual("", _storage.GetString("shortString"));
            Assert.AreEqual(new string('x', 10000), _storage.GetString("mediumString"));
            Assert.AreEqual("🌟🌠✨", _storage.GetString("longString"));
            Assert.AreEqual("Just ASCII now", _storage.GetString("unicodeString"));

            // 使用不同类型覆盖现有键
            _storage.Set("intKey", "This was an int");
            _storage.Set("longString", 42);

            // 验证类型变更后的值
            Assert.AreEqual("This was an int", _storage.GetString("intKey"));
            Assert.AreEqual(42, _storage.GetInt("longString"));

            // 验证原来的类型读取会失败
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 15，但预期长度为 4"));
            Assert.AreEqual(0, _storage.GetInt("intKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 类型不匹配：存储的是 Int，但尝试读取为 String"));
            Assert.AreEqual(string.Empty, _storage.GetString("longString"));
        }

        [Test]
        public void TestPersistence()
        {
            _storage.Set("persistenceKey", "persistenceValue");
            _storage.Dispose();

            _storage = new KeyValueStorage(_testDirectory, "test");
            Assert.AreEqual("persistenceValue", _storage.GetString("persistenceKey"));
        }

        [Test]
        public void TestTypeMismatch_IntToLong()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 4，但预期长度为 8"));
            _storage.Set("intKey", 42);
            Assert.AreEqual(0, _storage.GetLong("intKey"));
        }

        [Test]
        public void TestTypeMismatch_IntToDouble()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 4，但预期长度为 8"));
            _storage.Set("intKey", 42);
            Assert.AreEqual(0.0, _storage.GetDouble("intKey"));
        }

        [Test]
        public void TestTypeMismatch_FloatToDouble()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 4，但预期长度为 8"));
            _storage.Set("floatKey", 3.14f);
            Assert.AreEqual(0.0, _storage.GetDouble("floatKey"));
        }

        [Test]
        public void TestTypeMismatch_FloatToInt()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 类型不匹配：存储的是 Float，但尝试读取为 Int"));
            _storage.Set("floatKey", 3.14f);
            Assert.AreEqual(0, _storage.GetInt("floatKey"));
        }

        [Test]
        public void TestTypeMismatch_BoolToInt()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 1，但预期长度为 4"));
            _storage.Set("boolKey", true);
            Assert.AreEqual(0, _storage.GetInt("boolKey"));
        }

        [Test]
        public void TestTypeMismatch_BoolToString()
        {
            // 对于字符串，不会有长度不匹配的错误，因为字符串可以是任意长度
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 类型不匹配：存储的是 Bool，但尝试读取为 String"));
            _storage.Set("boolKey", true);
            Assert.AreEqual(string.Empty, _storage.GetString("boolKey"));
        }

        [Test]
        public void TestTypeMismatch_StringToInt_SameLength()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 类型不匹配：存储的是 String，但尝试读取为 Int"));
            _storage.Set("stringKey", "test");  // "test" 的长度恰好是 4 字节
            Assert.AreEqual(0, _storage.GetInt("stringKey"));
        }

        [Test]
        public void TestTypeMismatch_StringToInt_ShorterLength()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 2，但预期长度为 4"));
            _storage.Set("stringKey", "ab");  // 2 字节字符串
            Assert.AreEqual(0, _storage.GetInt("stringKey"));
        }

        [Test]
        public void TestTypeMismatch_StringToInt_LongerLength()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 8，但预期长度为 4"));
            _storage.Set("stringKey", "longtext");  // 8 字节字符串
            Assert.AreEqual(0, _storage.GetInt("stringKey"));
        }

        [Test]
        public void TestTypeMismatch_StringToInt_UnicodeLength()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 6，但预期长度为 4"));
            _storage.Set("stringKey", "你好");  // 中文字符，UTF-8 编码后为 6 字节
            Assert.AreEqual(0, _storage.GetInt("stringKey"));
        }

        [Test]
        public void TestTypeMismatch_StringToInt_SingleUnicodeChar()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 3，但预期长度为 4"));
            _storage.Set("stringKey", "你");  // 单个中文字符，UTF-8 编码后为 3 字节
            Assert.AreEqual(0, _storage.GetInt("stringKey"));
        }

        [Test]
        public void TestTypeMismatch_StringToCustom()
        {
            // 对于自定义类型，不会有长度不匹配的错误，因为自定义类型可以是任意长度
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 类型不匹配：存储的是 String，但尝试读取为 Custom"));
            _storage.Set("stringKey", "test");
            Assert.IsNull(_storage.Get<MyClass>("stringKey"));
        }

        [Test]
        public void TestTypeMismatch_CustomToInt()
        {
            // 自定义类型的长度可能不确定，所以我们不能预测具体的长度错误消息
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 \\d+，但预期长度为 4"));
            var myClass = new MyClass { Id = 1, Name = "TestClass" };
            _storage.Set("myClassKey", myClass);
            Assert.AreEqual(0, _storage.GetInt("myClassKey"));
        }

        [Test]
        public void TestTypeMismatch_CustomToString()
        {
            // 对于字符串，不会有长度不匹配的错误，因为字符串可以是任意长度
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 类型不匹配：存储的是 Custom，但尝试读取为 String"));
            var myClass = new MyClass { Id = 1, Name = "TestClass" };
            _storage.Set("myClassKey", myClass);
            Assert.AreEqual(string.Empty, _storage.GetString("myClassKey"));
        }

        [Test]
        public void TestLengthMismatch()
        {
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 4，但预期长度为 8"));
            _storage.Set("intKey", 42);
            Assert.AreEqual(0L, _storage.GetLong("intKey"));
        }

        [Test]
        public void TestSetAndGetNullString()
        {
            _storage.Set("nullStringKey", null);
            Assert.AreEqual(string.Empty, _storage.GetString("nullStringKey"));
        }

        [Test]
        public void TestSetAndGetEmptyString()
        {
            _storage.Set("emptyStringKey", string.Empty);
            Assert.AreEqual(string.Empty, _storage.GetString("emptyStringKey"));
        }

        [Test]
        public void TestSetNullStringAndGetOtherTypes()
        {
            _storage.Set("nullStringKey", null);
            
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 4"));
            Assert.AreEqual(0, _storage.GetInt("nullStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 8"));
            Assert.AreEqual(0L, _storage.GetLong("nullStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 4"));
            Assert.AreEqual(0f, _storage.GetFloat("nullStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 8"));
            Assert.AreEqual(0d, _storage.GetDouble("nullStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 1"));
            Assert.IsFalse(_storage.GetBool("nullStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 1"));
            Assert.AreEqual(0, _storage.GetByte("nullStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 2"));
            Assert.AreEqual(0, _storage.GetShort("nullStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 4"));
            Assert.AreEqual(0U, _storage.GetUInt("nullStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 8"));
            Assert.AreEqual(0UL, _storage.GetULong("nullStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 2"));
            Assert.AreEqual(0, _storage.GetUShort("nullStringKey"));

            // 对于字符串，不会有长度不匹配的错误，因为字符串可以是任意长度
            Assert.AreEqual(string.Empty, _storage.GetString("nullStringKey"));

            // 对于自定义类型，不会有长度不匹配的错误，因为自定义类型可以是任意长度
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 类型不匹配：存储的是 String，但尝试读取为 Custom"));
            Assert.IsNull(_storage.Get<MyClass>("nullStringKey"));
        }

        [Test]
        public void TestSetEmptyStringAndGetOtherTypes()
        {
            _storage.Set("emptyStringKey", string.Empty);
            
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 4"));
            Assert.AreEqual(0, _storage.GetInt("emptyStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 8"));
            Assert.AreEqual(0L, _storage.GetLong("emptyStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 4"));
            Assert.AreEqual(0f, _storage.GetFloat("emptyStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 8"));
            Assert.AreEqual(0d, _storage.GetDouble("emptyStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 1"));
            Assert.IsFalse(_storage.GetBool("emptyStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 1"));
            Assert.AreEqual(0, _storage.GetByte("emptyStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 2"));
            Assert.AreEqual(0, _storage.GetShort("emptyStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 4"));
            Assert.AreEqual(0U, _storage.GetUInt("emptyStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 8"));
            Assert.AreEqual(0UL, _storage.GetULong("emptyStringKey"));

            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 数据长度不匹配：存储的长度为 0，但预期长度为 2"));
            Assert.AreEqual(0, _storage.GetUShort("emptyStringKey"));

            // 对于字符串，不会有长度不匹配的错误，因为字符串可以是任意长度
            Assert.AreEqual(string.Empty, _storage.GetString("emptyStringKey"));

            // 对于自定义类型，不会有长度不匹配的错误，因为自定义类型可以是任意长度
            LogAssert.Expect(LogType.Error, new Regex("键 '.+' 类型不匹配：存储的是 String，但尝试读取为 Custom"));
            Assert.IsNull(_storage.Get<MyClass>("emptyStringKey"));
        }

        [Test]
        public void TestOverwriteWithNullAndEmptyString()
        {
            _storage.Set("overwriteKey", "Initial Value");
            Assert.AreEqual("Initial Value", _storage.GetString("overwriteKey"));

            // 用 null 覆盖
            _storage.Set("overwriteKey", null);
            Assert.AreEqual(string.Empty, _storage.GetString("overwriteKey"));

            _storage.Set("overwriteKey", "New Value");
            Assert.AreEqual("New Value", _storage.GetString("overwriteKey"));

            // 用空字符串覆盖
            _storage.Set("overwriteKey", string.Empty);
            Assert.AreEqual(string.Empty, _storage.GetString("overwriteKey"));

            // 再次用 null 覆盖
            _storage.Set("overwriteKey", null);
            Assert.AreEqual(string.Empty, _storage.GetString("overwriteKey"));

            // 用非空值覆盖
            _storage.Set("overwriteKey", "Final Value");
            Assert.AreEqual("Final Value", _storage.GetString("overwriteKey"));
        }

        [Test]
        public void TestSetNullCustomType()
        {
            var myClass = new MyClass { Id = 1, Name = "TestClass" };
            _storage.Set("customKey", myClass);
            Assert.IsNotNull(_storage.Get<MyClass>("customKey"));

            _storage.Set<MyClass>("customKey", null);
            Assert.IsNull(_storage.Get<MyClass>("customKey"));
        }

        [Test]
        public void TestSetNullThenSetValue()
        {
            var myClass = new MyClass { Id = 1, Name = "TestClass" };
            _storage.Set("customKey", myClass);
            Assert.IsNotNull(_storage.Get<MyClass>("customKey"));

            _storage.Set<MyClass>("customKey", null);
            Assert.IsNull(_storage.Get<MyClass>("customKey"));

            var newClass = new MyClass { Id = 2, Name = "NewClass" };
            _storage.Set("customKey", newClass);
            var retrievedClass = _storage.Get<MyClass>("customKey");
            Assert.IsNotNull(retrievedClass);
            Assert.AreEqual(2, retrievedClass.Id);
            Assert.AreEqual("NewClass", retrievedClass.Name);
        }

        [Test]
        public void TestSetNullThenReload()
        {
            var myClass = new MyClass { Id = 1, Name = "TestClass" };
            _storage.Set("customKey", myClass);
            Assert.IsNotNull(_storage.Get<MyClass>("customKey"));

            _storage.Set<MyClass>("customKey", null);
            Assert.IsNull(_storage.Get<MyClass>("customKey"));

            // 重新创建存储实例来模拟重新加载
            _storage.Dispose();
            _storage = new KeyValueStorage(_testDirectory, "test");

            // 验证null值在重新加载后仍然保持
            Assert.IsNull(_storage.Get<MyClass>("customKey"));
        }

        [Test]
        public void TestSetMultipleEntriesThenReload()
        {
            _storage.Set("intKey", 42);
            _storage.Set("stringKey", "Hello");
            var myClass = new MyClass { Id = 1, Name = "TestClass" };
            _storage.Set("customKey", myClass);

            // 重新创建存储实例来模拟重新加载
            _storage.Dispose();
            _storage = new KeyValueStorage(_testDirectory, "test");

            // 验证所有值在重新加载后仍然正确
            Assert.AreEqual(42, _storage.GetInt("intKey"));
            Assert.AreEqual("Hello", _storage.GetString("stringKey"));
            var retrievedClass = _storage.Get<MyClass>("customKey");
            Assert.IsNotNull(retrievedClass);
            Assert.AreEqual(1, retrievedClass.Id);
            Assert.AreEqual("TestClass", retrievedClass.Name);
        }

        [Test]
        public void TestLargeNumberOfEntries()
        {
            // 使用字符串
            for (int i = 0; i < 10000; i++)
            {
                _storage.Set($"stringKey{i}", $"Value{i}");
            }

            for (int i = 0; i < 10000; i++)
            {
                Assert.AreEqual($"Value{i}", _storage.GetString($"stringKey{i}"));
            }

            // 更新一些值
            for (int i = 0; i < 10000; i += 2)
            {
                _storage.Set($"stringKey{i}", $"UpdatedValue{i}");
            }

            for (int i = 0; i < 10000; i++)
            {
                if (i % 2 == 0)
                {
                    Assert.AreEqual($"UpdatedValue{i}", _storage.GetString($"stringKey{i}"));
                }
                else
                {
                    Assert.AreEqual($"Value{i}", _storage.GetString($"stringKey{i}"));
                }
            }

            // 删除一些值（设置为null）
            for (int i = 0; i < 2500; i++)
            {
                _storage.Set($"stringKey{i}", null);
            }

            for (int i = 0; i < 10000; i++)
            {
                if (i < 2500)
                {
                    Assert.AreEqual(string.Empty, _storage.GetString($"stringKey{i}"));
                }
                else if (i % 2 == 0)
                {
                    Assert.AreEqual($"UpdatedValue{i}", _storage.GetString($"stringKey{i}"));
                }
                else
                {
                    Assert.AreEqual($"Value{i}", _storage.GetString($"stringKey{i}"));
                }
            }

            // 使用自定义类型
            for (int i = 0; i < 10000; i++)
            {
                _storage.Set<MyClass>($"customKey{i}", new MyClass { Id = i, Name = $"Name{i}" });
            }

            for (int i = 0; i < 10000; i++)
            {
                var obj = _storage.Get<MyClass>($"customKey{i}");
                Assert.IsNotNull(obj);
                Assert.AreEqual(i, obj.Id);
                Assert.AreEqual($"Name{i}", obj.Name);
            }

            // 更新一些值
            for (int i = 0; i < 10000; i += 2)
            {
                _storage.Set<MyClass>($"customKey{i}", new MyClass { Id = i * 2, Name = $"UpdatedName{i}" });
            }

            for (int i = 0; i < 10000; i++)
            {
                var obj = _storage.Get<MyClass>($"customKey{i}");
                Assert.IsNotNull(obj);
                if (i % 2 == 0)
                {
                    Assert.AreEqual(i * 2, obj.Id);
                    Assert.AreEqual($"UpdatedName{i}", obj.Name);
                }
                else
                {
                    Assert.AreEqual(i, obj.Id);
                    Assert.AreEqual($"Name{i}", obj.Name);
                }
            }

            // 删除一些值
            for (int i = 0; i < 2500; i++)
            {
                _storage.Set<MyClass>($"customKey{i}", null);
            }

            for (int i = 0; i < 10000; i++)
            {
                var obj = _storage.Get<MyClass>($"customKey{i}");
                if (i < 2500)
                {
                    Assert.IsNull(obj);
                }
                else if (i % 2 == 0)
                {
                    Assert.IsNotNull(obj);
                    Assert.AreEqual(i * 2, obj.Id);
                    Assert.AreEqual($"UpdatedName{i}", obj.Name);
                }
                else
                {
                    Assert.IsNotNull(obj);
                    Assert.AreEqual(i, obj.Id);
                    Assert.AreEqual($"Name{i}", obj.Name);
                }
            }
        }

        [Test]
        public void TestGetIntWithDefaultValue()
        {
            Assert.AreEqual(42, _storage.GetInt("nonExistentKey", 42));
            _storage.Set("intKey", 10);
            Assert.AreEqual(10, _storage.GetInt("intKey", 42));
        }

        [Test]
        public void TestGetLongWithDefaultValue()
        {
            Assert.AreEqual(42L, _storage.GetLong("nonExistentKey", 42L));
            _storage.Set("longKey", 10L);
            Assert.AreEqual(10L, _storage.GetLong("longKey", 42L));
        }

        [Test]
        public void TestGetFloatWithDefaultValue()
        {
            Assert.AreEqual(4.2f, _storage.GetFloat("nonExistentKey", 4.2f));
            _storage.Set("floatKey", 1.0f);
            Assert.AreEqual(1.0f, _storage.GetFloat("floatKey", 4.2f));
        }

        [Test]
        public void TestGetDoubleWithDefaultValue()
        {
            Assert.AreEqual(4.2, _storage.GetDouble("nonExistentKey", 4.2));
            _storage.Set("doubleKey", 1.0);
            Assert.AreEqual(1.0, _storage.GetDouble("doubleKey", 4.2));
        }

        [Test]
        public void TestGetBoolWithDefaultValue()
        {
            Assert.IsTrue(_storage.GetBool("nonExistentKey", true));
            _storage.Set("boolKey", false);
            Assert.IsFalse(_storage.GetBool("boolKey", true));
        }

        [Test]
        public void TestGetByteWithDefaultValue()
        {
            Assert.AreEqual((byte)42, _storage.GetByte("nonExistentKey", 42));
            _storage.Set("byteKey", (byte)10);
            Assert.AreEqual((byte)10, _storage.GetByte("byteKey", 42));
        }

        [Test]
        public void TestGetShortWithDefaultValue()
        {
            Assert.AreEqual((short)42, _storage.GetShort("nonExistentKey", 42));
            _storage.Set("shortKey", (short)10);
            Assert.AreEqual((short)10, _storage.GetShort("shortKey", 42));
        }

        [Test]
        public void TestGetUIntWithDefaultValue()
        {
            Assert.AreEqual(42U, _storage.GetUInt("nonExistentKey", 42U));
            _storage.Set("uintKey", 10U);
            Assert.AreEqual(10U, _storage.GetUInt("uintKey", 42U));
        }

        [Test]
        public void TestGetULongWithDefaultValue()
        {
            Assert.AreEqual(42UL, _storage.GetULong("nonExistentKey", 42UL));
            _storage.Set("ulongKey", 10UL);
            Assert.AreEqual(10UL, _storage.GetULong("ulongKey", 42UL));
        }

        [Test]
        public void TestGetUShortWithDefaultValue()
        {
            Assert.AreEqual((ushort)42, _storage.GetUShort("nonExistentKey", 42));
            _storage.Set("ushortKey", (ushort)10);
            Assert.AreEqual((ushort)10, _storage.GetUShort("ushortKey", 42));
        }

        [Test]
        public void TestGetStringWithDefaultValue()
        {
            Assert.AreEqual("default", _storage.GetString("nonExistentKey", "default"));
            _storage.Set("stringKey", "value");
            Assert.AreEqual("value", _storage.GetString("stringKey", "default"));
        }

        [Test]
        public void TestGetCustomTypeWithDefaultValue()
        {
            var defaultValue = new MyClass { Id = 42, Name = "Default" };
            var result = _storage.Get("nonExistentKey", defaultValue);
            Assert.AreEqual(42, result.Id);
            Assert.AreEqual("Default", result.Name);

            var setValue = new MyClass { Id = 10, Name = "Test" };
            _storage.Set("customKey", setValue);
            result = _storage.Get("customKey", defaultValue);
            Assert.AreEqual(10, result.Id);
            Assert.AreEqual("Test", result.Name);
        }

        [Test]
        public void TestClear()
        {
            // 添加一些数据
            _storage.Set("intKey", 42);
            _storage.Set("stringKey", "Hello");
            _storage.Set("customKey", new MyClass { Id = 10, Name = "Test" });

            // 验证数据已经存储
            Assert.AreEqual(42, _storage.GetInt("intKey"));
            Assert.AreEqual("Hello", _storage.GetString("stringKey"));
            var customValue = _storage.Get<MyClass>("customKey");
            Assert.AreEqual(10, customValue.Id);
            Assert.AreEqual("Test", customValue.Name);

            // 清空存储
            _storage.Clear();

            // 验证所有数据都已被清除
            Assert.AreEqual(0, _storage.GetInt("intKey"));
            Assert.AreEqual(string.Empty, _storage.GetString("stringKey"));
            Assert.IsNull(_storage.Get<MyClass>("customKey"));

            // 验证可以在清空后添加新数据
            _storage.Set("newKey", "NewValue");
            Assert.AreEqual("NewValue", _storage.GetString("newKey"));
        }

        [Test]
        public void TestRemove()
        {
            // 添加一些数据
            _storage.Set("intKey", 42);
            _storage.Set("stringKey", "Hello");
            _storage.Set("customKey", new MyClass { Id = 10, Name = "Test" });

            // 验证数据已经存储
            Assert.AreEqual(42, _storage.GetInt("intKey"));
            Assert.AreEqual("Hello", _storage.GetString("stringKey"));
            var customValue = _storage.Get<MyClass>("customKey");
            Assert.AreEqual(10, customValue.Id);
            Assert.AreEqual("Test", customValue.Name);

            // 移除一个键
            Assert.IsTrue(_storage.Remove("stringKey"));

            // 验证键已被移除
            Assert.AreEqual(string.Empty, _storage.GetString("stringKey"));

            // 验证其他键未受影响
            Assert.AreEqual(42, _storage.GetInt("intKey"));
            customValue = _storage.Get<MyClass>("customKey");
            Assert.AreEqual(10, customValue.Id);
            Assert.AreEqual("Test", customValue.Name);

            // 尝试移除不存在的键
            Assert.IsFalse(_storage.Remove("nonExistentKey"));

            // 验证可以在移除后添加新数据
            _storage.Set("newKey", "NewValue");
            Assert.AreEqual("NewValue", _storage.GetString("newKey"));
        }
    }

    public class MyClass : ISerializable
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(Id);
                writer.Write(Name);
                return ms.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                Id = reader.ReadInt32();
                Name = reader.ReadString();
            }
        }
    }
}
