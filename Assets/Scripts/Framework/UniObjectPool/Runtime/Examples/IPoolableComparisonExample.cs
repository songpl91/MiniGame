using UnityEngine;
using UniFramework.ObjectPool;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// IPoolable 接口对比示例
    /// 展示实现和不实现 IPoolable 接口的区别
    /// </summary>
    public class IPoolableComparisonExample : MonoBehaviour
    {
        private void Start()
        {
            PoolManager.Initialize();
            
            Debug.Log("=== IPoolable 接口对比示例 ===");
            
            // 创建对象池
            CreatePools();
            
            // 演示不同的使用方式
            DemonstrateWithoutIPoolable();
            DemonstrateWithIPoolable();
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        private void CreatePools()
        {
            // 不实现 IPoolable 的数据类对象池
            PoolManager.CreatePool<DataWithoutIPoolable>("DataWithoutIPoolablePool",
                createFunc: () => new DataWithoutIPoolable(),
                resetAction: data => data.Reset(), // 🎯 必须手动指定重置方法
                destroyAction: null,
                config: new PoolConfig
                {
                    ValidateOnReturn = true,
                    InitialCapacity = 5,
                    MaxCapacity = 20
                });

            // 实现 IPoolable 的数据类对象池
            PoolManager.CreatePool<DataWithIPoolable>("DataWithIPoolablePool",
                createFunc: () => new DataWithIPoolable(),
                resetAction: null, // 🎯 不需要指定，会自动调用 OnDespawn()
                destroyAction: null,
                config: new PoolConfig
                {
                    ValidateOnReturn = true,
                    InitialCapacity = 5,
                    MaxCapacity = 20
                });
        }

        /// <summary>
        /// 演示不实现 IPoolable 接口的使用方式
        /// </summary>
        private void DemonstrateWithoutIPoolable()
        {
            Debug.Log("\n=== 不实现 IPoolable 接口 ===");
            
            // 获取对象
            var data = PoolManager.Get<DataWithoutIPoolable>("DataWithoutIPoolablePool");
            
            // 设置数据
            data.id = 123;
            data.name = "TestData";
            
            Debug.Log($"✓ 设置数据：ID={data.id}, Name={data.name}");
            
            // 手动归还到池
            PoolManager.Return("DataWithoutIPoolablePool", data);
            Debug.Log("✓ 手动归还到池（需要在创建池时指定 resetAction）");
            
            // 再次获取，验证是否重置
            var data2 = PoolManager.Get<DataWithoutIPoolable>("DataWithoutIPoolablePool");
            Debug.Log($"✓ 重新获取数据：ID={data2.id}, Name={data2.name} (应该已重置)");
            
            PoolManager.Return("DataWithoutIPoolablePool", data2);
        }

        /// <summary>
        /// 演示实现 IPoolable 接口的使用方式
        /// </summary>
        private void DemonstrateWithIPoolable()
        {
            Debug.Log("\n=== 实现 IPoolable 接口 ===");
            
            // 获取对象
            var data = PoolManager.Get<DataWithIPoolable>("DataWithIPoolablePool");
            
            // 设置数据
            data.id = 456;
            data.name = "TestDataWithIPoolable";
            
            Debug.Log($"✓ 设置数据：ID={data.id}, Name={data.name}");
            
            // 归还到池（会自动调用 OnDespawn）
            PoolManager.Return("DataWithIPoolablePool", data);
            Debug.Log("✓ 归还到池（Return 时自动调用 OnDespawn）");
            
            // 再次获取，验证是否重置
            var data2 = PoolManager.Get<DataWithIPoolable>("DataWithIPoolablePool");
            Debug.Log($"✓ 重新获取数据：ID={data2.id}, Name={data2.name} (应该已重置)");
            
            PoolManager.Return("DataWithIPoolablePool", data2);
        }
    }

    #region 数据类定义

    /// <summary>
    /// 不实现 IPoolable 接口的数据类
    /// ❌ 缺点：需要手动管理重置逻辑
    /// </summary>
    public class DataWithoutIPoolable
    {
        public int id;
        public string name;

        /// <summary>
        /// 手动重置方法
        /// 🎯 必须在创建池时通过 resetAction 参数指定调用
        /// </summary>
        public void Reset()
        {
            id = 0;
            name = string.Empty;
            Debug.Log("🔄 DataWithoutIPoolable.Reset() 被手动调用");
        }
    }

    /// <summary>
    /// 实现 IPoolable 接口的数据类
    /// ✅ 优点：自动管理重置逻辑
    /// </summary>
    public class DataWithIPoolable : IPoolable
    {
        public int id;
        public string name;

        /// <summary>
        /// 从池中取出时自动调用
        /// </summary>
        public void OnSpawn()
        {
            Debug.Log("🚀 DataWithIPoolable.OnSpawn() 被自动调用");
        }

        /// <summary>
        /// 归还到池时自动调用
        /// 🎯 这就是自动重置的关键！
        /// </summary>
        public void OnDespawn()
        {
            id = 0;
            name = string.Empty;
            Debug.Log("🔄 DataWithIPoolable.OnDespawn() 被自动调用");
        }

        /// <summary>
        /// 便捷的重置方法（可选）
        /// </summary>
        public void Reset()
        {
            OnDespawn();
        }
    }

    #endregion
}