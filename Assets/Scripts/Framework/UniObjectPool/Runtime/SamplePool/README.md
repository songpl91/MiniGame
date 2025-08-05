# SamplePool - 极简版对象池系统

## 概述

SamplePool 是基于 UniFramework.ObjectPool 设计的极简版对象池系统，专为Demo和快速原型开发设计。它保留了对象池的核心功能，同时大幅简化了复杂性，并为后期扩展预留了接口。

## 设计理念

- **当前简单够用**：提供最基础的对象池功能，满足Demo需求
- **中期可扩展维护**：保留扩展接口，便于功能增强
- **后期方便添加复杂功能**：可以平滑升级到完整版本

## 核心组件

### 1. ISamplePoolable
```csharp
public interface ISamplePoolable
{
    void OnSpawn();   // 对象从池中取出时调用
    void OnDespawn(); // 对象归还到池中时调用
}
```

### 2. SamplePool<T>
泛型对象池核心类，提供基础的Get/Return功能。

**主要特性：**
- 简化的构造函数
- 基础的对象获取和归还
- 预热功能
- 扩展接口预留

### 3. SamplePoolManager
全局对象池管理器，支持类型池和命名池。

**主要功能：**
- 类型对象池管理
- 命名对象池管理
- 便捷的Get/Return方法

### 4. SampleGameObjectPool
专门用于GameObject的对象池，提供Unity特有的便捷方法。

**主要功能：**
- Spawn/Despawn方法
- 自动位置和状态管理
- GameObject特有的重置逻辑

### 5. SamplePoolExtensions
扩展方法类，提供链式调用和便捷操作。

## 使用示例

### 基础使用

```csharp
// 初始化管理器
SamplePoolManager.Initialize();

// 创建对象池
var stringPool = SamplePoolManager.CreatePool(
    () => new StringBuilder(),
    sb => sb.Clear(),
    maxCapacity: 20
);

// 获取对象
var sb = stringPool.Get();
sb.Append("Hello World");

// 归还对象
stringPool.Return(sb);
```

### GameObject对象池

```csharp
// 创建GameObject对象池
var bulletPool = new SampleGameObjectPool(bulletPrefab, bulletParent, 50);
bulletPool.Prewarm(10); // 预热10个对象

// 生成对象
var bullet = bulletPool.Spawn(Vector3.zero, Quaternion.identity);

// 回收对象
bulletPool.Despawn(bullet);
```

### 使用扩展方法

```csharp
// 创建命名对象池
bulletPrefab.CreateSamplePool("BulletPool", bulletParent, 30);

// 生成对象
var bullet = bulletPrefab.SpawnFromSamplePool("BulletPool", position);

// 归还对象
bullet.ReturnToSamplePool("BulletPool");
```

## 与完整版本的对比

| 功能 | SamplePool | UniObjectPool |
|------|------------|---------------|
| 基础Get/Return | ✅ | ✅ |
| 线程安全 | ❌ | ✅ |
| 统计信息 | ❌ | ✅ |
| 自动清理 | ❌ | ✅ |
| 对象验证 | ❌ | ✅ |
| 配置系统 | ❌ | ✅ |
| 扩展接口 | ✅ | ✅ |

## 扩展路径

### 阶段1：当前Demo版本
- 基础对象池功能
- 简单的GameObject支持
- 基础的管理器

### 阶段2：功能增强版本
```csharp
// 可以通过继承扩展
public class EnhancedSamplePool<T> : SamplePool<T>
{
    protected override void ApplyConfig(object config)
    {
        // 添加配置支持
    }
}
```

### 阶段3：完整功能版本
- 平滑迁移到UniObjectPool
- 保持API兼容性
- 添加高级功能

## 迁移指南

当Demo成熟需要更多功能时，可以按以下步骤迁移：

1. **保持接口兼容**：ISamplePoolable可以直接实现IPoolable
2. **逐步替换**：SamplePool -> UniObjectPool
3. **添加配置**：引入PoolConfig系统
4. **启用高级功能**：统计、清理、验证等

## 最佳实践

1. **命名规范**：使用常量定义池名称
2. **预热策略**：根据使用频率预热适量对象
3. **生命周期管理**：及时归还不再使用的对象
4. **内存控制**：合理设置maxCapacity
5. **扩展准备**：使用虚方法和接口为扩展做准备

## 注意事项

- SamplePool不是线程安全的，仅适用于主线程
- 没有自动清理功能，需要手动管理内存
- 对象验证功能简化，需要开发者自行保证正确性
- 统计功能缺失，调试时需要其他方式监控

## 总结

SamplePool为快速Demo开发提供了一个轻量级的对象池解决方案。它在保持简单易用的同时，为后期扩展预留了充分的空间，是从原型到产品的理想过渡方案。