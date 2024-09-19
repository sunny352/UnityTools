using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public interface ISerializable
{
    byte[] Serialize();
    void Deserialize(byte[] data);
}

public class KeyValueStorage : IDisposable
{
    private readonly object _lock = new object();

    public KeyValueStorage(string directory, string name)
    {
        _dataPath = Path.Combine(directory, $"{name}.bin");
        _indexPath = Path.Combine(directory, $"{name}.idx");
        _index = new Dictionary<ulong, (long position, int length, int allocatedLength, long indexFilePosition)>();
        _md5 = MD5.Create();
        _cache = new Dictionary<ulong, object>();
    }

    private void Init()
    {
        if (_isInit)
        {
            return;
        }

        if (File.Exists(_indexPath))
        {
            LoadIndex();
        }
        else
        {
            CreateFiles();
        }

        _isInit = true;
    }

    private bool _isInit;

    private readonly string _dataPath;
    private readonly string _indexPath;
    private readonly Dictionary<ulong, (long position, int length, int allocatedLength, long indexFilePosition)> _index;
    private readonly MD5 _md5;
    private readonly Dictionary<ulong, object> _cache;

    private const int IndexEntrySize = 8 + 8 + 4 + 4; // hashKey (ulong) + position (long) + length (int) + allocatedLength (int)

    public void Dispose()
    {
        _md5.Dispose();
    }

    private void CreateFiles()
    {
        File.Create(_dataPath).Close();
        File.Create(_indexPath).Close();
    }

    private void LoadIndex()
    {
        using var reader = new BinaryReader(File.Open(_indexPath, FileMode.Open));
        long indexFilePosition = 0;
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var hashKey = reader.ReadUInt64();
            var position = reader.ReadInt64();
            var length = reader.ReadInt32();
            var allocatedLength = reader.ReadInt32();
            _index[hashKey] = (position, length, allocatedLength, indexFilePosition);
            indexFilePosition += IndexEntrySize;
        }
    }

    private ulong CalculateMD5Hash(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = _md5.ComputeHash(inputBytes);
        return BitConverter.ToUInt64(hashBytes, 0);
    }

    // 基本类型的 Set 方法重载
    public void Set(string key, int value) => SetValue(key, value, BitConverter.GetBytes, DataType.Int);
    public void Set(string key, long value) => SetValue(key, value, BitConverter.GetBytes, DataType.Long);
    public void Set(string key, float value) => SetValue(key, value, BitConverter.GetBytes, DataType.Float);
    public void Set(string key, double value) => SetValue(key, value, BitConverter.GetBytes, DataType.Double);
    public void Set(string key, bool value) => SetValue(key, value, BitConverter.GetBytes, DataType.Bool);
    public void Set(string key, byte value) => SetValue(key, value, v => new[] { v }, DataType.Byte);
    public void Set(string key, short value) => SetValue(key, value, BitConverter.GetBytes, DataType.Short);
    public void Set(string key, uint value) => SetValue(key, value, BitConverter.GetBytes, DataType.UInt);
    public void Set(string key, ulong value) => SetValue(key, value, BitConverter.GetBytes, DataType.ULong);
    public void Set(string key, ushort value) => SetValue(key, value, BitConverter.GetBytes, DataType.UShort);

    // 字符串类型的 Set 方法
    public void Set(string key, string value)
    {
        lock (_lock)
        {
            try
            {
                Init();
                var hashKey = CalculateMD5Hash(key);
                var valueBytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
                WriteValueToFile(hashKey, valueBytes, DataType.String);
                _cache[hashKey] = value ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.LogError(FormatErrorMessage(key, "设置失败", ex.Message));
            }
        }
    }

    // 实现了 ISerializable 接口的类型的 Set 方法
    public void Set<T>(string key, T value) where T : ISerializable
    {
        lock (_lock)
        {
            try
            {
                Init();
                var hashKey = CalculateMD5Hash(key);
                if (value == null)
                {
                    RemoveEntry(hashKey);
                }
                else
                {
                    var valueBytes = value.Serialize();
                    WriteValueToFile(hashKey, valueBytes, DataType.Custom);
                    _cache[hashKey] = value;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(FormatErrorMessage(key, "设置失败", ex.Message));
            }
        }
    }

    // 基本类型的通用 Set 方法
    private void SetValue<T>(string key, T value, Func<T, byte[]> converter, DataType dataType) where T : struct
    {
        lock (_lock)
        {
            try
            {
                Init();
                var hashKey = CalculateMD5Hash(key);
                var valueBytes = converter(value);
                WriteValueToFile(hashKey, valueBytes, dataType);
                _cache[hashKey] = value; // 直接缓存值
            }
            catch (Exception ex)
            {
                Debug.LogError(FormatErrorMessage(key, "设置失败", ex.Message));
            }
        }
    }

    private void WriteValueToFile(ulong hashKey, byte[] valueBytes, DataType dataType)
    {
        var newLength = valueBytes.Length;
        var allocatedLength = AllocateSpace(newLength);

        long position;
        long indexFilePosition;
        if (_index.TryGetValue(hashKey, out var existingEntry))
        {
            var (existingPosition, _, existingAllocatedLength, existingIndexFilePosition) = existingEntry;
            indexFilePosition = existingIndexFilePosition;

            if (newLength <= existingAllocatedLength)
            {
                // 覆盖原位置
                position = existingPosition;
                using var writer = new BinaryWriter(File.Open(_dataPath, FileMode.Open));
                writer.Seek((int)existingPosition, SeekOrigin.Begin);
                writer.Write((byte)dataType);  // 写入类型信息
                writer.Write(valueBytes);
            }
            else
            {
                // 在文件末尾追加新数据
                using var writer = new BinaryWriter(File.Open(_dataPath, FileMode.Append));
                position = writer.BaseStream.Position;
                writer.Write((byte)dataType);  // 写入类型信息
                writer.Write(valueBytes);
            }
        }
        else
        {
            // 新键，在文件末尾追加
            using var writer = new BinaryWriter(File.Open(_dataPath, FileMode.Append));
            position = writer.BaseStream.Position;
            writer.Write((byte)dataType);  // 写入类型信息
            writer.Write(valueBytes);
            indexFilePosition = -1; // 标记为新条目
        }

        _index[hashKey] = (position, newLength, allocatedLength, indexFilePosition);
        UpdateIndexFileEntry(hashKey, position, newLength, allocatedLength, indexFilePosition);
    }

    private void UpdateIndexFileEntry(ulong hashKey, long position, int length, int allocatedLength, long indexFilePosition)
    {
        using var stream = new FileStream(_indexPath, FileMode.Open, FileAccess.ReadWrite);
        using var writer = new BinaryWriter(stream);
        if (indexFilePosition >= 0)
        {
            // 更新现有条目
            stream.Seek(indexFilePosition + 8, SeekOrigin.Begin); // 跳过 hashKey
            writer.Write(position);
            writer.Write(length);
            writer.Write(allocatedLength);
        }
        else
        {
            // 添加新条目
            stream.Seek(0, SeekOrigin.End);
            writer.Write(hashKey);
            writer.Write(position);
            writer.Write(length);
            writer.Write(allocatedLength);
            _index[hashKey] = (position, length, allocatedLength, stream.Position - IndexEntrySize);
        }
    }

    private int AllocateSpace(int requiredLength)
    {
        // 向上取整到最接近的32字节
        return ((requiredLength + 31) / 32) * 32;
    }

    // 修改基本类型的 Get 方法
    public int GetInt(string key, int defaultValue = 0) => GetValue(key, BitConverter.ToInt32, sizeof(int), DataType.Int, defaultValue);
    public long GetLong(string key, long defaultValue = 0L) => GetValue(key, BitConverter.ToInt64, sizeof(long), DataType.Long, defaultValue);
    public float GetFloat(string key, float defaultValue = 0f) => GetValue(key, BitConverter.ToSingle, sizeof(float), DataType.Float, defaultValue);
    public double GetDouble(string key, double defaultValue = 0d) => GetValue(key, BitConverter.ToDouble, sizeof(double), DataType.Double, defaultValue);
    public bool GetBool(string key, bool defaultValue = false) => GetValue(key, BitConverter.ToBoolean, sizeof(bool), DataType.Bool, defaultValue);
    public byte GetByte(string key, byte defaultValue = 0) => GetValue(key, (bytes, _) => bytes[0], sizeof(byte), DataType.Byte, defaultValue);
    public short GetShort(string key, short defaultValue = 0) => GetValue(key, BitConverter.ToInt16, sizeof(short), DataType.Short, defaultValue);
    public uint GetUInt(string key, uint defaultValue = 0U) => GetValue(key, BitConverter.ToUInt32, sizeof(uint), DataType.UInt, defaultValue);
    public ulong GetULong(string key, ulong defaultValue = 0UL) => GetValue(key, BitConverter.ToUInt64, sizeof(ulong), DataType.ULong, defaultValue);
    public ushort GetUShort(string key, ushort defaultValue = 0) => GetValue(key, BitConverter.ToUInt16, sizeof(ushort), DataType.UShort, defaultValue);

    // 字符串类型的 Get 方法
    public string GetString(string key, string defaultValue = "")
    {
        lock (_lock)
        {
            try
            {
                Init();
                var hashKey = CalculateMD5Hash(key);

                // 首先检查缓存
                if (_cache.TryGetValue(hashKey, out var cachedValue))
                {
                    if (cachedValue is string stringValue)
                    {
                        return stringValue;
                    }
                }

                if (!_index.TryGetValue(hashKey, out var entry))
                {
                    return defaultValue;
                }

                var (position, length, _, _) = entry;
                using var reader = new BinaryReader(File.Open(_dataPath, FileMode.Open, FileAccess.Read, FileShare.Read));
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
                var storedType = (DataType)reader.ReadByte();
                if (storedType != DataType.String)
                {
                    Debug.LogError(GetTypeMismatchMessage(key, storedType, DataType.String));
                    return defaultValue;
                }
                var valueBytes = reader.ReadBytes(length);
                var result = Encoding.UTF8.GetString(valueBytes);
                _cache[hashKey] = result; // 缓存结果
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError(FormatErrorMessage(key, "获取字符串失败", ex.Message));
                return defaultValue;
            }
        }
    }

    // 实现了 ISerializable 接口的类型的 Get 方法
    public T Get<T>(string key, T defaultValue = default) where T : ISerializable, new()
    {
        lock (_lock)
        {
            try
            {
                Init();
                var hashKey = CalculateMD5Hash(key);

                // 首先检查缓存
                if (_cache.TryGetValue(hashKey, out var cachedValue))
                {
                    if (cachedValue is T typedValue)
                    {
                        return typedValue;
                    }
                }

                if (!_index.TryGetValue(hashKey, out var entry))
                {
                    return defaultValue;
                }

                var (position, length, _, _) = entry;
                using var reader = new BinaryReader(File.Open(_dataPath, FileMode.Open));
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
                var storedType = (DataType)reader.ReadByte();
                if (storedType != DataType.Custom)
                {
                    Debug.LogError(GetTypeMismatchMessage(key, storedType, DataType.Custom));
                    return defaultValue;
                }
                var valueBytes = reader.ReadBytes(length);
                var instance = new T();
                instance.Deserialize(valueBytes);
                _cache[hashKey] = instance; // 缓存结果
                return instance;
            }
            catch (Exception ex)
            {
                Debug.LogError(FormatErrorMessage(key, "获取自定义类型失败", ex.Message));
                return defaultValue;
            }
        }
    }

    // 修改后的 GetValue 方法，增加了数据长度检查
    private T GetValue<T>(string key, Func<byte[], int, T> converter, int expectedLength, DataType expectedType, T defaultValue)
    {
        lock (_lock)
        {
            try
            {
                Init();
                var hashKey = CalculateMD5Hash(key);

                // 首先检查缓存
                if (_cache.TryGetValue(hashKey, out var cachedValue))
                {
                    if (cachedValue is T typedValue)
                    {
                        return typedValue;
                    }
                }

                if (!_index.TryGetValue(hashKey, out var entry))
                {
                    return defaultValue;
                }

                var (position, length, _, _) = entry;

                // 数据长度检查
                if (length != expectedLength)
                {
                    Debug.LogError(GetLengthMismatchMessage(key, length, expectedLength));
                    return defaultValue;
                }

                using var reader = new BinaryReader(File.Open(_dataPath, FileMode.Open));
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
                var storedType = (DataType)reader.ReadByte();
                if (storedType == DataType.Unknown)
                {
                    Debug.LogError(GetUnknownTypeMessage(key));
                    return defaultValue;
                }
                if (storedType != expectedType)
                {
                    Debug.LogError(GetTypeMismatchMessage(key, storedType, expectedType));
                    return defaultValue;
                }
                var valueBytes = reader.ReadBytes(expectedLength);
                if (valueBytes.Length != expectedLength)
                {
                    Debug.LogError(GetLengthMismatchMessage(key, valueBytes.Length, expectedLength));
                    return defaultValue;
                }
                var result = converter(valueBytes, 0);
                _cache[hashKey] = result; // 缓存结果
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError(FormatErrorMessage(key, "获取值失败", ex.Message));
                return defaultValue;
            }
        }
    }

    private enum DataType : byte
    {
        Unknown = 0,
        Int = 1,
        Long = 2,
        Float = 3,
        Double = 4,
        Bool = 5,
        Byte = 6,
        Short = 7,
        UInt = 8,
        ULong = 9,
        UShort = 10,
        String = 11,
        Custom = 12
    }

    private string GetTypeMismatchMessage(string key, DataType storedType, DataType expectedType)
    {
        return FormatErrorMessage(key, "类型不匹配", $"存储的是 {storedType}，但尝试读取为 {expectedType}");
    }

    private string GetLengthMismatchMessage(string key, int storedLength, int expectedLength)
    {
        return FormatErrorMessage(key, "数据长度不匹配", $"存储的长度为 {storedLength}，但预期长度为 {expectedLength}");
    }

    private string GetUnknownTypeMessage(string key)
    {
        return FormatErrorMessage(key, "未知类型", "存储的数据类型未知或无效");
    }

    private string FormatErrorMessage(string key, string errorType, string details)
    {
        return $"键 '{key}' {errorType}：{details}";
    }

    private void RemoveEntry(ulong hashKey)
    {
        if (_index.TryGetValue(hashKey, out var entry))
        {
            var (_, _, _, indexFilePosition) = entry;
            _index.Remove(hashKey);
            _cache.Remove(hashKey);
            RemoveIndexFileEntry(indexFilePosition);
        }
    }

    private void RemoveIndexFileEntry(long indexFilePosition)
    {
        using var stream = new FileStream(_indexPath, FileMode.Open, FileAccess.ReadWrite);
        var lastEntryPosition = stream.Length - IndexEntrySize;
        if (indexFilePosition != lastEntryPosition)
        {
            // 不是最后一个条目，用最后一个条目覆盖它
            stream.Seek(lastEntryPosition, SeekOrigin.Begin);
            var lastEntry = new byte[IndexEntrySize];
            _ = stream.Read(lastEntry, 0, IndexEntrySize);

            stream.Seek(indexFilePosition, SeekOrigin.Begin);
            stream.Write(lastEntry, 0, IndexEntrySize);

            // 更新被移动的最后一个条目在内存中的索引位置
            using var reader = new BinaryReader(new MemoryStream(lastEntry));
            var movedHashKey = reader.ReadUInt64();
            if (_index.TryGetValue(movedHashKey, out var movedEntry))
            {
                _index[movedHashKey] = (movedEntry.position, movedEntry.length, movedEntry.allocatedLength, indexFilePosition);
            }
        }
        // 无论如何都要截断文件
        stream.SetLength(stream.Length - IndexEntrySize);
    }

    public bool ContainsKey(string key)
    {
        lock (_lock)
        {
            try
            {
                Init();
                var hashKey = CalculateMD5Hash(key);
                return _index.ContainsKey(hashKey);
            }
            catch (Exception ex)
            {
                Debug.LogError(FormatErrorMessage(key, "检查键是否存在失败", ex.Message));
                return false;
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            try
            {
                Init();

                // 清空内存中的索引和缓存
                _index.Clear();
                _cache.Clear();

                // 清空或删除数据文件和索引文件
                File.WriteAllBytes(_dataPath, Array.Empty<byte>());
                File.WriteAllBytes(_indexPath, Array.Empty<byte>());

                Debug.Log("KeyValueStorage 已清空");
            }
            catch (Exception ex)
            {
                Debug.LogError($"清空 KeyValueStorage 失败：{ex.Message}");
            }
        }
    }

    public bool Remove(string key)
    {
        lock (_lock)
        {
            try
            {
                Init();
                var hashKey = CalculateMD5Hash(key);
                if (_index.TryGetValue(hashKey, out var entry))
                {
                    var (_, _, _, indexFilePosition) = entry;
                    _index.Remove(hashKey);
                    _cache.Remove(hashKey);
                    RemoveIndexFileEntry(indexFilePosition);
                    Debug.Log($"键 '{key}' 已被移除");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(FormatErrorMessage(key, "移除失败", ex.Message));
                return false;
            }
        }
    }
}
