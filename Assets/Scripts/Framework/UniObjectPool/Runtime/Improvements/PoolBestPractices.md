# UniObjectPool 最佳实践指南

## 🎯 性能优化建议

### 1. 选择合适的对象池版本

```csharp
// 高频使用场景：选择高性能版本
var bulletPool = SmartPoolManager.CreatePool<Bullet>(
    createFunc: () => new Bullet(),
    useHighPerformanceVersion: true  // 启用高性能版本
);

// 内存敏感场景：选择内存优化版本
var memoryPool = new MemoryOptimizedPool<LargeObject>(
    createFunc: () => new LargeObject(),
    enableObjectTracking: false  // 关闭对象追踪以节省内存
);
```

### 2. 合理配置对象池参数

```csharp
// 根据使用场景配置参数
var config = new PoolConfig
{
    InitialCapacity = 20,        // 预创建数量：根据平均使用量设置
    MaxCapacity = 100,           // 最大容量：根据峰值使用量设置
    EnableAutoCleanup = true,    // 启用自动清理
    CleanupInterval = 30f,       // 清理间隔：根据内存压力调整
    CleanupThreshold = 50,       // 清理阈值：通常设为最大容量的一半
    EnableStatistics = false,    // 生产环境建议关闭统计以提升性能
    ValidateOnReturn = false     // 高性能场景建议关闭验证
};
```

### 3. 避免常见性能陷阱

```csharp
// ❌ 错误：频繁创建小对象池
for (int i = 0; i < 1000; i++)
{
    var pool = PoolManager.CreatePool<SmallObject>(() => new SmallObject());
    // 这会创建1000个对象池！
}

// ✅ 正确：复用对象池
var sharedPool = PoolManager.CreatePool<SmallObject>(() => new SmallObject());
for (int i = 0; i < 1000; i++)
{
    var obj = sharedPool.Get();
    // 使用对象...
    sharedPool.Return(obj);
}
```

## 🧠 内存管理最佳实践

### 1. 实现正确的IPoolable接口

```csharp
public class GameEntity : IPoolable
{
    private List<Component> components = new List<Component>();
    private Dictionary<string, object> properties = new Dictionary<string, object>();
    
    public void OnSpawn()
    {
        // 初始化对象状态
        isActive = true;
        health = maxHealth;
    }
    
    public void OnDespawn()
    {
        // ✅ 正确：清空集合，不要设为null
        components.Clear();
        properties.Clear();
        
        // ✅ 正确：重置基本类型字段
        isActive = false;
        health = 0;
        
        // ❌ 错误：不要设置引用为null
        // components = null;  // 这会导致下次使用时出错
    }
}
```

### 2. 智能清理策略

```csharp
// 设置智能清理策略
SmartPoolManager.SetCleanupStrategy("BulletPool", new PoolCleanupStrategy
{
    MemoryPressureThreshold = 0.7f,  // 内存压力超过70%时清理
    MinIdleTime = 180,               // 对象空闲3分钟后可清理
    IdleRatio = 0.6f,               // 空闲对象超过60%时清理
    EnableAdaptiveCleanup = true     // 启用自适应清理
});
```

### 3. 监控内存使用

```csharp
// 定期检查内存使用情况
private void CheckMemoryUsage()
{
    if (memoryPool is MemoryOptimizedPool<LargeObject> optimizedPool)
    {
        string memoryInfo = optimizedPool.GetMemoryInfo();
        Debug.Log(memoryInfo);
        
        // 在内存压力大时主动清理
        if (IsMemoryPressureHigh())
        {
            optimizedPool.SmartCleanup();
        }
    }
}
```

## 🔧 使用模式建议

### 1. 游戏对象池化

```csharp
// 为GameObject创建对象池
public class BulletManager : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletParent;
    
    private UniObjectPool<GameObject> bulletPool;
    
    private void Start()
    {
        // 使用扩展方法创建GameObject对象池
        bulletPool = bulletPrefab.CreateGameObjectPool(
            poolName: "BulletPool",
            parent: bulletParent,
            config: PoolConfig.CreateHighPerformance()
        );
    }
    
    public void FireBullet(Vector3 position, Vector3 direction)
    {
        // 从对象池获取子弹
        var bullet = bulletPool.Get();
        bullet.transform.position = position;
        bullet.GetComponent<Bullet>().Initialize(direction);
        
        // 子弹会在生命周期结束时自动归还到池
    }
}
```

### 2. 数据对象池化

```csharp
// 为数据类创建对象池
public class EventSystem : MonoBehaviour
{
    private UniObjectPool<EventData> eventPool;
    
    private void Start()
    {
        eventPool = PoolManager.CreatePool<EventData>(
            createFunc: () => new EventData(),
            config: PoolConfig.CreateMemoryOptimized()
        );
    }
    
    public void TriggerEvent(string eventId, Dictionary<string, object> parameters)
    {
        // 使用using语句自动归还对象
        using (var pooledEvent = eventPool.Get().AsPooled(eventPool))
        {
            var eventData = pooledEvent.Value;
            eventData.eventId = eventId;
            eventData.parameters.Clear();
            
            foreach (var kvp in parameters)
            {
                eventData.parameters[kvp.Key] = kvp.Value;
            }
            
            ProcessEvent(eventData);
        } // 自动归还到池
    }
}
```

### 3. 临时对象池化

```csharp
// 为临时计算对象创建对象池
public class PathfindingSystem : MonoBehaviour
{
    private UniObjectPool<PathfindingRequest> requestPool;
    
    private void Start()
    {
        requestPool = PoolManager.CreatePool<PathfindingRequest>(
            createFunc: () => new PathfindingRequest(),
            resetAction: request => request.Reset(),  // 重置请求状态
            config: new PoolConfig
            {
                InitialCapacity = 10,
                MaxCapacity = 50,
                EnableStatistics = false  // 关闭统计以提升性能
            }
        );
    }
    
    public async Task<Path> FindPathAsync(Vector3 start, Vector3 end)
    {
        var request = requestPool.Get();
        try
        {
            request.Setup(start, end);
            var path = await CalculatePath(request);
            return path;
        }
        finally
        {
            requestPool.Return(request);  // 确保归还到池
        }
    }
}
```

## 📊 性能监控

### 1. 启用性能监控

```csharp
// 在开发阶段启用详细监控
#if UNITY_EDITOR
    var config = new PoolConfig
    {
        EnableStatistics = true,
        ValidateOnReturn = true
    };
#else
    var config = PoolConfig.CreateHighPerformance();
#endif
```

### 2. 定期生成性能报告

```csharp
// 定期输出性能报告
private void LogPerformanceReport()
{
    string report = SmartPoolManager.GetPerformanceReport();
    Debug.Log(report);
    
    // 可以将报告发送到分析服务
    AnalyticsService.SendReport("ObjectPoolPerformance", report);
}
```

## ⚠️ 常见陷阱和解决方案

### 1. 内存泄漏

```csharp
// ❌ 错误：忘记归还对象
var obj = pool.Get();
// 使用对象...
// 忘记调用 pool.Return(obj);

// ✅ 解决方案：使用using语句
using (var pooledObj = pool.Get().AsPooled(pool))
{
    var obj = pooledObj.Value;
    // 使用对象...
} // 自动归还
```

### 2. 对象状态污染

```csharp
// ❌ 错误：没有正确重置对象状态
public void OnDespawn()
{
    // 忘记清理某些状态
}

// ✅ 解决方案：完整的状态重置
public void OnDespawn()
{
    // 重置所有可变状态
    position = Vector3.zero;
    velocity = Vector3.zero;
    isActive = false;
    components.Clear();
    events.Clear();
}
```

### 3. 线程安全问题

```csharp
// ❌ 错误：在多线程环境中使用非线程安全的对象池
// 标准UniObjectPool在多线程环境中需要额外同步

// ✅ 解决方案：使用高性能版本或添加同步
var threadSafePool = new HighPerformanceObjectPool<MyObject>(
    createFunc: () => new MyObject()
);
```

## 🎯 总结

1. **根据场景选择合适的对象池版本**
2. **正确实现IPoolable接口**
3. **合理配置对象池参数**
4. **使用智能清理策略**
5. **定期监控性能和内存使用**
6. **避免常见的使用陷阱**

遵循这些最佳实践，可以最大化对象池的性能优势，同时避免常见的问题。