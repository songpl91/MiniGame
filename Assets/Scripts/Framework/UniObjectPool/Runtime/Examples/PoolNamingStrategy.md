# UniObjectPool 对象池命名策略

## 设计原则

基于您的反馈，我们采用了以下设计原则：

1. **明确性优于便利性** - 用户应该明确知道对象池的名称
2. **避免自动生成复杂名称** - 不使用系统自动生成的复杂唯一名称
3. **提供简单的默认选项** - 为简单场景提供可预测的默认命名规则
4. **保持向后兼容** - 不破坏现有代码

## 核心API设计

### 1. 明确命名（推荐方式）

```csharp
// 创建对象池 - 明确指定名称
bulletPrefab.CreateGameObjectPool("BulletPool", bulletParent);

// 从对象池获取对象 - 明确指定名称
var bullet = bulletPrefab.SpawnFromPool("BulletPool");

// 归还对象到对象池 - 明确指定名称
bullet.ReturnToPool("BulletPool");
```

**优势：**
- 用户完全控制对象池名称
- 代码清晰易懂
- 便于调试和维护
- 避免命名冲突

### 2. 默认命名（便捷方式）

```csharp
// 创建对象池 - 使用默认命名规则：{预制体名称}Pool
bulletPrefab.CreateGameObjectPoolWithDefaultName(bulletParent);

// 从对象池获取对象 - 使用默认命名规则
var bullet = bulletPrefab.SpawnFromPoolWithDefaultName();

// 归还对象到对象池 - 使用默认命名规则
bullet.ReturnToPoolWithDefaultName();
```

**默认命名规则：**
- GameObject池：`{预制体名称}Pool`
- Component池：`{组件类型}_{预制体名称}Pool`

**优势：**
- 简单易用
- 命名规则可预测
- 适合简单场景

## 错误处理策略

### 1. 参数验证
- 所有核心方法都要求明确指定对象池名称
- 空值或无效参数会抛出异常
- 提供清晰的错误信息

### 2. 对象池不存在的处理
```csharp
// SpawnFromPool - 抛出异常
try 
{
    var obj = prefab.SpawnFromPool("NonExistentPool");
}
catch (InvalidOperationException ex)
{
    Debug.LogError($"对象池不存在: {ex.Message}");
}

// ReturnToPool - 销毁对象并警告
obj.ReturnToPool("NonExistentPool"); // 对象被销毁，输出警告日志
```

## PoolRegistry 注册器

### 功能
- 记录对象池的注册信息（名称、预制体、父对象、类型、时间、标签）
- 提供对象池查找功能
- 支持标签管理
- 自动生命周期管理

### 主要方法
```csharp
// 注册对象池
PoolRegistry.RegisterPool(poolName, prefab, parent, typeof(GameObject));

// 查找对象池
var poolName = PoolRegistry.FindPoolNameByPrefab(prefab);
var poolNames = PoolRegistry.FindPossiblePoolNames(prefab);

// 标签管理
var taggedPools = PoolRegistry.FindPoolsByTag("Bullet");

// 获取注册信息
var registration = PoolRegistry.GetRegistration(poolName);
var allRegistrations = PoolRegistry.GetAllRegistrations();
```

## 最佳实践

### 1. 使用常量定义对象池名称
```csharp
public class PoolNames
{
    public const string BULLET_POOL = "BulletPool";
    public const string ENEMY_POOL = "EnemyPool";
    public const string EFFECT_POOL = "EffectPool";
}

// 使用
bulletPrefab.CreateGameObjectPool(PoolNames.BULLET_POOL);
```

### 2. 集中管理对象池创建
```csharp
public class PoolInitializer : MonoBehaviour
{
    private void Start()
    {
        // 集中创建所有对象池
        CreateBulletPool();
        CreateEnemyPool();
        CreateEffectPool();
    }
    
    private void CreateBulletPool()
    {
        bulletPrefab.CreateGameObjectPool(
            PoolNames.BULLET_POOL,
            bulletParent,
            PoolConfig.CreateHighPerformance()
        );
    }
}
```

### 3. 错误处理
```csharp
public static class SafePoolExtensions
{
    public static GameObject TrySpawnFromPool(this GameObject prefab, string poolName)
    {
        try
        {
            return prefab.SpawnFromPool(poolName);
        }
        catch (InvalidOperationException)
        {
            Debug.LogWarning($"对象池 {poolName} 不存在，返回null");
            return null;
        }
    }
}
```

## 迁移指南

### 从旧版本迁移
如果您之前使用了自动命名的方式，可以按以下步骤迁移：

1. **识别现有对象池名称**
```csharp
// 查看当前注册的对象池
var registrations = PoolRegistry.GetAllRegistrations();
foreach (var reg in registrations)
{
    Debug.Log($"对象池: {reg.PoolName}, 预制体: {reg.Prefab.name}");
}
```

2. **定义明确的名称常量**
```csharp
// 替换自动生成的名称
// 旧方式：系统自动生成 "Bullet_BulletParent_12345_Pool"
// 新方式：明确定义
public const string BULLET_POOL = "BulletPool";
```

3. **更新代码调用**
```csharp
// 旧方式
var bullet = bulletPrefab.SpawnFromPool(); // 自动查找

// 新方式
var bullet = bulletPrefab.SpawnFromPool(PoolNames.BULLET_POOL); // 明确指定
```

## 总结

这个解决方案平衡了以下需求：
- **明确性** - 用户知道对象池的确切名称
- **简单性** - 提供简单的默认命名选项
- **可维护性** - 代码清晰，便于调试
- **灵活性** - 支持复杂场景的自定义需求
- **安全性** - 完善的错误处理和参数验证

通过这种设计，用户可以根据项目需求选择合适的命名策略，既保证了生产环境的可控性，又提供了开发阶段的便利性。