# UnityTools
Unity的一些工具代码

## KeyValueStorage

KeyValueStorage是一个用于Unity的键值存储系统,提供了高效的数据持久化解决方案。适用于保存游戏进度、用户设置等场景。

### 主要特性:
- 支持多种基本数据类型(int, long, float, double, bool, byte, short, uint, ulong, ushort)
- 支持字符串类型
- 支持自定义类型(实现ISerializable接口)
- 高效的文件存储和索引机制
- 线程安全
- 内存缓存以提高读取性能
- 支持数据的增删改查操作

### 使用示例:

```csharp
// 创建KeyValueStorage实例
var storage = new KeyValueStorage(Application.persistentDataPath, "GameData");
// 存储数据
storage.Set("PlayerScore", 1000);
storage.Set("PlayerName", "Alice");
// 读取数据
int score = storage.GetInt("PlayerScore");
string name = storage.GetString("PlayerName");
// 检查键是否存在
bool hasKey = storage.ContainsKey("PlayerScore");
// 删除数据
storage.Remove("PlayerScore");
// 清空所有数据
storage.Clear();
```