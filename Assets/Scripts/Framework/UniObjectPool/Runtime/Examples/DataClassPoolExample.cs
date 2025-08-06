using System.Collections.Generic;
using UnityEngine;
using UniFramework.ObjectPool;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 数据类对象池示例
    /// 展示如何为纯数据类使用对象池
    /// 
    /// 🎯 重要说明：
    /// 1. 数据类必须实现 IPoolable 接口才能自动重置
    /// 2. OnSpawn() - 从池中取出时调用，用于初始化
    /// 3. OnDespawn() - 归还到池时调用，用于重置数据（相当于 Reset）
    /// 4. 使用 PooledObject<T>.Dispose() 时会自动调用 OnDespawn()
    /// 5. 集合类型字段应该 Clear() 而不是设为 null
    /// </summary>
    public class DataClassPoolExample : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private int testDataCount = 1000;
        [SerializeField] private int testIterations = 100;

        private void Start()
        {
            PoolManager.Initialize();
            
            Debug.Log("=== 数据类对象池示例 ===");
            
            // 演示不同类型的数据类对象池
            DemonstrateSimpleDataPool();
            DemonstrateComplexDataPool();
            DemonstratePerformanceComparison();
        }

        #region 简单数据类示例

        /// <summary>
        /// 演示简单数据类的对象池
        /// </summary>
        private void DemonstrateSimpleDataPool()
        {
            Debug.Log("\n=== 简单数据类对象池 ===");
            
            // 创建数据类对象池
            CreateSimpleDataPools();
            
            // 使用数据类对象池
            UseSimpleDataPools();
        }

        /// <summary>
        /// 创建简单数据类的对象池
        /// </summary>
        private void CreateSimpleDataPools()
        {
            Debug.Log("创建简单数据类对象池");
            
            // 创建不同类型的数据池
            // 🎯 注意：由于数据类实现了 IPoolable 接口，
            // 对象池会自动调用 OnDespawn() 方法进行重置，
            // 所以不需要手动指定 resetAction
            PoolManager.CreatePool<PlayerData>("PlayerDataPool", 
                createFunc: () => new PlayerData(),
                resetAction: null,        // 🎯 不需要手动指定，会自动调用 OnDespawn()
                destroyAction: null,      // 销毁时不需要特殊处理
                config: new PoolConfig
                {
                    ValidateOnReturn = true,
                    InitialCapacity = 10,
                    MaxCapacity = 100
                });
            
            PoolManager.CreatePool<DamageData>("DamageDataPool",
                createFunc: () => new DamageData(),
                resetAction: null,        // 🎯 自动调用 OnDespawn()
                destroyAction: null,
                config: new PoolConfig
                {
                    ValidateOnReturn = true,
                    InitialCapacity = 50,
                    MaxCapacity = 200
                });
            
            PoolManager.CreatePool<EventData>("EventDataPool",
                createFunc: () => new EventData(),
                resetAction: null,        // 🎯 自动调用 OnDespawn()
                destroyAction: null,
                config: new PoolConfig
                {
                    ValidateOnReturn = true,
                    InitialCapacity = 20,
                    MaxCapacity = 100
                });
            
            Debug.Log("✓ 简单数据类对象池创建完成");
        }

        /// <summary>
        /// 使用简单数据类对象池
        /// </summary>
        private void UseSimpleDataPools()
        {
            Debug.Log("使用简单数据类对象池");
            
            // 使用玩家数据池
            UsePlayerDataPool();
            
            // 使用伤害数据池
            UseDamageDataPool();
            
            // 使用事件数据池
            UseEventDataPool();
        }

        private void UsePlayerDataPool()
        {
            Debug.Log("【玩家系统】使用玩家数据池");
            
            // 从池中获取数据对象
            var playerData = PoolManager.Get<PlayerData>("PlayerDataPool");
            
            // 设置数据
            playerData.playerId = 12345;
            playerData.playerName = "TestPlayer";
            playerData.level = 10;
            playerData.experience = 2500;
            playerData.health = 100;
            playerData.mana = 50;
            
            Debug.Log($"✓ 玩家数据：{playerData.playerName}, 等级:{playerData.level}, 血量:{playerData.health}");
            
            // 使用完毕后归还到池
            PoolManager.Return(playerData);
            Debug.Log("✓ 玩家数据已归还到池");
        }

        private void UseDamageDataPool()
        {
            Debug.Log("【战斗系统】使用伤害数据池");
            
            // 模拟多次伤害计算
            for (int i = 0; i < 5; i++)
            {
                var damageData = PoolManager.Get<DamageData>("DamageDataPool");
                
                // 设置伤害数据
                damageData.attackerId = 1001;
                damageData.targetId = 2001;
                damageData.damageAmount = Random.Range(10, 50);
                damageData.damageType = (DamageType)(i % 3);
                damageData.isCritical = Random.value > 0.8f;
                damageData.timestamp = System.DateTime.Now;
                
                Debug.Log($"✓ 伤害数据：{damageData.damageAmount} ({damageData.damageType}), 暴击:{damageData.isCritical}");
                
                // 归还到池
                PoolManager.Return(damageData);
            }
            
            Debug.Log("✓ 所有伤害数据已处理完毕");
        }

        private void UseEventDataPool()
        {
            Debug.Log("【事件系统】使用事件数据池");
            
            var eventData = PoolManager.Get<EventData>("EventDataPool");
            
            // 设置事件数据
            eventData.eventId = "PLAYER_LEVEL_UP";
            eventData.eventType = EventType.PlayerAction;
            eventData.parameters.Clear();
            eventData.parameters["playerId"] = "12345";
            eventData.parameters["newLevel"] = "11";
            eventData.parameters["oldLevel"] = "10";
            eventData.timestamp = System.DateTime.Now;
            
            Debug.Log($"✓ 事件数据：{eventData.eventId}, 参数数量:{eventData.parameters.Count}");
            
            // 归还到池
            PoolManager.Return(eventData);
            Debug.Log("✓ 事件数据已归还到池");
        }

        #endregion

        #region 复杂数据类示例

        /// <summary>
        /// 演示复杂数据类的对象池
        /// </summary>
        private void DemonstrateComplexDataPool()
        {
            Debug.Log("\n=== 复杂数据类对象池 ===");
            
            // 创建复杂数据类对象池
            CreateComplexDataPool();
            
            // 使用复杂数据类对象池
            UseComplexDataPool();
        }

        /// <summary>
        /// 创建复杂数据类对象池
        /// </summary>
        private void CreateComplexDataPool()
        {
            Debug.Log("创建复杂数据类对象池");
            
            PoolManager.CreatePool<GameStateData>("GameStateDataPool",
                createFunc: () => new GameStateData(),
                resetAction: data => data.Reset(),
                destroyAction: data => data.Cleanup(),
                config: new PoolConfig
                {
                    ValidateOnReturn = true,
                    InitialCapacity = 5,
                    MaxCapacity = 20
                });
            
            Debug.Log("✓ 复杂数据类对象池创建完成");
        }

        /// <summary>
        /// 使用复杂数据类对象池
        /// </summary>
        private void UseComplexDataPool()
        {
            Debug.Log("使用复杂数据类对象池");
            
            var gameState = PoolManager.Get<GameStateData>("GameStateDataPool");
            
            // 设置复杂数据
            gameState.gameId = "GAME_001";
            gameState.currentLevel = 5;
            gameState.gameMode = GameMode.Campaign;
            gameState.startTime = System.DateTime.Now;
            
            // 添加玩家数据
            gameState.players.Add(new PlayerInfo { id = 1, name = "Player1", score = 1000 });
            gameState.players.Add(new PlayerInfo { id = 2, name = "Player2", score = 1200 });
            
            // 添加游戏事件
            gameState.gameEvents.Add("GAME_START");
            gameState.gameEvents.Add("LEVEL_COMPLETE");
            
            // 设置游戏设置
            gameState.gameSettings["difficulty"] = "Normal";
            gameState.gameSettings["soundEnabled"] = "true";
            gameState.gameSettings["musicVolume"] = "0.8";
            
            Debug.Log($"✓ 游戏状态：{gameState.gameId}, 关卡:{gameState.currentLevel}, 玩家数:{gameState.players.Count}");
            Debug.Log($"✓ 事件数:{gameState.gameEvents.Count}, 设置数:{gameState.gameSettings.Count}");
            
            // 归还到池
            PoolManager.Return(gameState);
            Debug.Log("✓ 游戏状态数据已归还到池");
        }

        #endregion

        #region 性能对比

        /// <summary>
        /// 性能对比：对象池 vs 直接创建
        /// </summary>
        private void DemonstratePerformanceComparison()
        {
            Debug.Log("\n=== 性能对比测试 ===");
            
            // 测试直接创建
            TestDirectCreation();
            
            // 测试对象池
            TestPooledCreation();
        }

        /// <summary>
        /// 测试直接创建性能
        /// </summary>
        private void TestDirectCreation()
        {
            Debug.Log("测试直接创建性能");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < testIterations; i++)
            {
                var dataList = new List<DamageData>();
                
                for (int j = 0; j < testDataCount; j++)
                {
                    var data = new DamageData();
                    data.attackerId = j;
                    data.targetId = j + 1000;
                    data.damageAmount = j % 100;
                    data.damageType = (DamageType)(j % 3);
                    data.isCritical = (j % 10) == 0;
                    data.timestamp = System.DateTime.Now;
                    
                    dataList.Add(data);
                }
                
                // 模拟使用数据
                foreach (var data in dataList)
                {
                    // 简单的数据访问
                    var total = data.damageAmount + (data.isCritical ? 50 : 0);
                }
                
                dataList.Clear();
            }
            
            stopwatch.Stop();
            Debug.Log($"✓ 直接创建耗时：{stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// 测试对象池性能
        /// </summary>
        private void TestPooledCreation()
        {
            Debug.Log("测试对象池性能");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < testIterations; i++)
            {
                var dataList = new List<DamageData>();
                
                for (int j = 0; j < testDataCount; j++)
                {
                    var data = PoolManager.Get<DamageData>("DamageDataPool");
                    
                    data.attackerId = j;
                    data.targetId = j + 1000;
                    data.damageAmount = j % 100;
                    data.damageType = (DamageType)(j % 3);
                    data.isCritical = (j % 10) == 0;
                    data.timestamp = System.DateTime.Now;
                    
                    dataList.Add(data);
                }
                
                // 模拟使用数据
                foreach (var data in dataList)
                {
                    var total = data.damageAmount + (data.isCritical ? 50 : 0);
                }
                
                // 归还所有对象到池
                foreach (var data in dataList)
                {
                    PoolManager.Return(data);
                }
                
                dataList.Clear();
            }
            
            stopwatch.Stop();
            Debug.Log($"✓ 对象池耗时：{stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion

        /// <summary>
        /// 显示池状态信息
        /// </summary>
        [ContextMenu("显示池状态")]
        public void ShowPoolStatus()
        {
            Debug.Log("=== 数据类对象池状态 ===");
            
            var pools = new string[] { "PlayerDataPool", "DamageDataPool", "EventDataPool", "GameStateDataPool" };
            
            foreach (string poolName in pools)
            {
                if (PoolManager.HasPool(poolName))
                {
                    Debug.Log($"池 {poolName}：活跃对象数量未知（需要扩展PoolManager接口）");
                }
                else
                {
                    Debug.Log($"池 {poolName}：不存在");
                }
            }
        }

        private void OnDestroy()
        {
            PoolManager.Destroy();
        }
    }

    #region 数据类定义

    /// <summary>
    /// 简单玩家数据类
    /// </summary>
    public class PlayerData : IPoolable
    {
        public int playerId;
        public string playerName;
        public int level;
        public int experience;
        public int health;
        public int mana;
        
        /// <summary>
        /// 对象从池中取出时调用
        /// 用于初始化对象状态
        /// </summary>
        public void OnSpawn()
        {
            // 从池中取出时可以进行一些初始化
            // 对于数据类通常不需要特殊处理
        }
        
        /// <summary>
        /// 对象归还到池中时调用
        /// 用于重置对象状态，准备回收
        /// 🎯 这就是自动调用的 Reset 方法！
        /// </summary>
        public void OnDespawn()
        {
            playerId = 0;
            playerName = string.Empty;
            level = 1;
            experience = 0;
            health = 100;
            mana = 100;
        }
        
        /// <summary>
        /// 便捷的重置方法（可选）
        /// 可以手动调用，也可以在 OnDespawn 中调用
        /// </summary>
        public void Reset()
        {
            OnDespawn(); // 直接调用 OnDespawn 保持一致性
        }
    }

    /// <summary>
    /// 伤害数据类
    /// </summary>
    public class DamageData : IPoolable
    {
        public int attackerId;
        public int targetId;
        public int damageAmount;
        public DamageType damageType;
        public bool isCritical;
        public System.DateTime timestamp;
        
        /// <summary>
        /// 对象从池中取出时调用
        /// </summary>
        public void OnSpawn()
        {
            // 伤害数据通常不需要特殊的初始化
        }
        
        /// <summary>
        /// 对象归还到池中时调用
        /// 🎯 自动重置数据
        /// </summary>
        public void OnDespawn()
        {
            attackerId = 0;
            targetId = 0;
            damageAmount = 0;
            damageType = DamageType.Physical;
            isCritical = false;
            timestamp = default;
        }
        
        /// <summary>
        /// 便捷的重置方法（可选）
        /// </summary>
        public void Reset()
        {
            OnDespawn();
        }
    }

    /// <summary>
    /// 事件数据类
    /// </summary>
    public class EventData : IPoolable
    {
        public string eventId;
        public EventType eventType;
        public Dictionary<string, string> parameters;
        public System.DateTime timestamp;
        
        public EventData()
        {
            parameters = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// 对象从池中取出时调用
        /// </summary>
        public void OnSpawn()
        {
            // 确保字典已初始化
            if (parameters == null)
                parameters = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// 对象归还到池中时调用
        /// 🎯 自动重置数据
        /// </summary>
        public void OnDespawn()
        {
            eventId = string.Empty;
            eventType = EventType.System;
            parameters.Clear(); // 清空字典，不要设为 null
            timestamp = default;
        }
        
        /// <summary>
        /// 便捷的重置方法（可选）
        /// </summary>
        public void Reset()
        {
            OnDespawn();
        }
    }

    /// <summary>
    /// 复杂游戏状态数据类
    /// </summary>
    public class GameStateData : IPoolable
    {
        public string gameId;
        public int currentLevel;
        public GameMode gameMode;
        public System.DateTime startTime;
        public List<PlayerInfo> players;
        public List<string> gameEvents;
        public Dictionary<string, string> gameSettings;
        
        public GameStateData()
        {
            players = new List<PlayerInfo>();
            gameEvents = new List<string>();
            gameSettings = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// 对象从池中取出时调用
        /// 🎯 初始化集合，确保对象可用
        /// </summary>
        public void OnSpawn()
        {
            // 确保集合已初始化
            if (players == null) players = new List<PlayerInfo>();
            if (gameEvents == null) gameEvents = new List<string>();
            if (gameSettings == null) gameSettings = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// 对象归还到池中时调用
        /// 🎯 自动重置数据，清空集合
        /// </summary>
        public void OnDespawn()
        {
            gameId = string.Empty;
            currentLevel = 1;
            gameMode = GameMode.Tutorial;
            startTime = default;
            players.Clear();      // 清空列表，不要设为 null
            gameEvents.Clear();   // 清空列表，不要设为 null
            gameSettings.Clear(); // 清空字典，不要设为 null
        }
        
        /// <summary>
        /// 初始化（从池中获取时调用）
        /// 🎯 已被 OnSpawn 替代，保留用于向后兼容
        /// </summary>
        public void Initialize()
        {
            OnSpawn();
        }
        
        /// <summary>
        /// 重置数据（归还到池时调用）
        /// 🎯 已被 OnDespawn 替代，保留用于向后兼容
        /// </summary>
        public void Reset()
        {
            OnDespawn();
        }
        
        /// <summary>
        /// 清理资源（销毁时调用）
        /// 注意：对象池中的对象通常不会被销毁，此方法仅在特殊情况下使用
        /// </summary>
        public void Cleanup()
        {
            players = null;
            gameEvents = null;
            gameSettings = null;
        }
    }

    /// <summary>
    /// 玩家信息
    /// </summary>
    public class PlayerInfo
    {
        public int id;
        public string name;
        public int score;
    }

    /// <summary>
    /// 伤害类型枚举
    /// </summary>
    public enum DamageType
    {
        Physical,
        Magical,
        True
    }

    /// <summary>
    /// 事件类型枚举
    /// </summary>
    public enum EventType
    {
        System,
        PlayerAction,
        GameLogic
    }

    /// <summary>
    /// 游戏模式枚举
    /// </summary>
    public enum GameMode
    {
        Tutorial,
        Campaign,
        Multiplayer,
        Sandbox
    }

    #endregion
}