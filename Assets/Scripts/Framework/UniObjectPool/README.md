# UniObjectPool

通用轻量级对象池系统

## 特性

- **通用性**: 支持任意类型的对象池化，不仅限于 GameObject
- **轻量级**: 最小化内存占用和性能开销
- **类型安全**: 强类型支持，编译时类型检查
- **自动管理**: 自动扩容、缩容和清理机制
- **线程安全**: 支持多线程环境下的安全操作
- **灵活配置**: 可配置初始容量、最大容量、清理策略等
- **生命周期管理**: 完整的对象生命周期管理
- **性能监控**: 内置性能统计和监控功能

## 核心组件

- `UniObjectPool<T>`: 泛型对象池核心类
- `IPoolable`: 可池化对象接口
- `PoolManager`: 对象池管理器
- `PoolConfig`: 对象池配置类
- `PoolStatistics`: 对象池统计信息

## 使用示例

```csharp
// 创建对象池
var pool = PoolManager.CreatePool<MyObject>(
    factory: () => new MyObject(),
    resetAction: obj => obj.Reset(),
    maxCapacity: 100
);

// 获取对象
var obj = pool.Get();

// 使用对象
obj.DoSomething();

// 归还对象
pool.Return(obj);
```

## 性能优势

- 减少 GC 压力
- 提高对象创建和销毁性能
- 内存复用，降低内存碎片
- 支持预热机制