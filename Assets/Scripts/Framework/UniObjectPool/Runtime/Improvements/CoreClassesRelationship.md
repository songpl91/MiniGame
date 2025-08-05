# UniObjectPool 核心类关系详解

## 概述

UniObjectPool 系统由三个核心类组成，它们各司其职，形成了一个完整的对象池化解决方案：

1. **UniObjectPool<T>** - 对象池核心实现
2. **PoolManager** - 对象池管理器
3. **PooledObject<T>** - 池化对象包装器

## 架构关系图

```
┌─────────────────────────────────────────────────────────────┐
│                    PoolManager (静态管理器)                    │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │           Dictionary<Type, object> _pools               │ │
│  │        Dictionary<string, object> _namedPools           │ │
│  └─────────────────────────────────────────────────────────┘ │
│                            │                                │
│                            ▼                                │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │              UniObjectPool<T> 实例                       │ │
│  │  ┌─────────────────────────────────────────────────────┐ │ │
│  │  │           Stack<T> _pool                            │ │ │
│  │  │         HashSet<T> _activeObjects                   │ │ │
│  │  │           PoolConfig _config                        │ │ │
│  │  │         PoolStatistics _statistics                  │ │ │
│  │  └─────────────────────────────────────────────────────┘ │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                PooledObject<T> (包装器)                      │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │              T _value                                   │ │
│  │        UniObjectPool<T> _pool                           │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## 详细关系分析

### 1. UniObjectPool<T> - 对象池核心实现

**职责：**
- 实际的对象池化逻辑
- 对象的创建、获取、归还、销毁
- 池容量管理和自动清理
- 统计信息收集

**核心特性：**
```csharp
public class UniObjectPool<T> : IDisposable where T : class
{
    private readonly Stack<T> _pool;           // 可用对象栈
    private readonly HashSet<T> _activeObjects; // 活跃对象追踪
    private readonly Func<T> _createFunc;      // 对象创建函数
    private readonly Action<T> _resetAction;   // 对象重置动作
    private readonly Action<T> _destroyAction; // 对象销毁动作
    
    public T Get()      // 获取对象
    public bool Return(T item)  // 归还对象
    public void Cleanup()       // 清理对象
}
```

### 2. PoolManager - 对象池管理器

**职责：**
- 全局对象池注册和管理
- 提供统一的对象池访问接口
- 支持类型池和命名池两种模式
- 生命周期管理

**管理模式：**
```csharp
public static class PoolManager
{
    // 类型池：一个类型对应一个池
    private static readonly Dictionary<Type, object> _pools;
    
    // 命名池：支持同一类型的多个池实例
    private static readonly Dictionary<string, object> _namedPools;
    
    // 创建池
    public static UniObjectPool<T> CreatePool<T>(...)
    public static UniObjectPool<T> CreatePool<T>(string poolName, ...)
    
    // 获取池
    public static UniObjectPool<T> GetPool<T>()
    public static UniObjectPool<T> GetPool<T>(string poolName)
    
    // 便捷方法
    public static T Get<T>()
    public static bool Return<T>(T item)
}
```

### 3. PooledObject<T> - 池化对象包装器

**职责：**
- 包装池化对象，提供 RAII 语义
- 自动归还对象到池中
- 防止对象泄漏

**使用模式：**
```csharp
public sealed class PooledObject<T> : IDisposable where T : class
{
    public T Value { get; }  // 包装的实际对象
    
    public void Dispose()    // 自动归还到池中
    
    // 支持隐式转换
    public static implicit operator T(PooledObject<T> pooledObject)
}
```

## 三者协作流程

### 创建阶段
```csharp
// 1. 通过 PoolManager 创建对象池
var pool = PoolManager.CreatePool<MyClass>(
    () => new MyClass(),           // 创建函数
    obj => obj.Reset(),           // 重置函数
    obj => obj.Dispose()          // 销毁函数
);

// 2. PoolManager 内部创建 UniObjectPool<MyClass> 实例
// 3. 将池实例存储在 _pools 字典中
```

### 使用阶段
```csharp
// 方式1：直接使用 PoolManager
MyClass obj1 = PoolManager.Get<MyClass>();
// ... 使用对象
PoolManager.Return(obj1);

// 方式2：使用 PooledObject 包装器
using (var pooledObj = new PooledObject<MyClass>(PoolManager.Get<MyClass>(), pool))
{
    MyClass obj2 = pooledObj.Value;
    // ... 使用对象
    // 自动归还（通过 using 语句）
}

// 方式3：直接操作具体池实例
var pool = PoolManager.GetPool<MyClass>();
MyClass obj3 = pool.Get();
// ... 使用对象
pool.Return(obj3);
```

## 设计模式分析

### 1. 工厂模式
- **UniObjectPool<T>** 作为对象工厂，负责对象的创建和管理

### 2. 单例模式
- **PoolManager** 采用静态类设计，全局唯一的管理入口

### 3. 包装器模式
- **PooledObject<T>** 包装实际对象，添加自动归还功能

### 4. RAII 模式
- **PooledObject<T>** 实现 IDisposable，确保资源自动释放

## 使用场景对比

| 使用方式 | 适用场景 | 优点 | 缺点 |
|---------|---------|------|------|
| **PoolManager.Get/Return** | 简单快速的对象获取 | API简洁，性能好 | 需要手动管理归还 |
| **PooledObject<T>** | 需要自动归还的场景 | 防止泄漏，RAII语义 | 额外的包装开销 |
| **直接操作池实例** | 高性能场景，批量操作 | 最高性能，完全控制 | API复杂，需要缓存池引用 |

## 最佳实践建议

### 1. 选择合适的使用方式
```csharp
// 简单场景：使用 PoolManager
var obj = PoolManager.Get<MyClass>();
try 
{
    // 使用对象
}
finally 
{
    PoolManager.Return(obj);
}

// 复杂场景：使用 PooledObject
using (var pooled = new PooledObject<MyClass>(PoolManager.Get<MyClass>(), pool))
{
    var obj = pooled.Value;
    // 使用对象，自动归还
}

// 高性能场景：直接操作池
var pool = PoolManager.GetPool<MyClass>();
var obj = pool.Get();
// 使用对象
pool.Return(obj);
```

### 2. 命名池的使用
```csharp
// 为不同用途创建不同的池
PoolManager.CreatePool<Bullet>("PlayerBullets", ...);
PoolManager.CreatePool<Bullet>("EnemyBullets", ...);

// 使用时指定池名称
var playerBullet = PoolManager.Get<Bullet>("PlayerBullets");
var enemyBullet = PoolManager.Get<Bullet>("EnemyBullets");
```

## 总结

这三个类形成了一个层次化的对象池系统：

1. **UniObjectPool<T>** 是底层实现，提供核心的池化逻辑
2. **PoolManager** 是中间层，提供统一的管理和访问接口
3. **PooledObject<T>** 是上层包装，提供更安全的使用方式

这种设计既保证了性能（直接使用池），又提供了便利性（PoolManager），还增加了安全性（PooledObject），满足了不同场景的需求。