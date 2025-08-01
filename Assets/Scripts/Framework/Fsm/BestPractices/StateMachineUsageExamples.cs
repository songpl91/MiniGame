using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// 状态机使用示例集合 - 展示不同场景下的最佳实践
/// </summary>
namespace StateMachineUsageExamples
{
    /// <summary>
    /// 示例1：角色AI状态机
    /// 展示如何使用状态机控制NPC行为
    /// </summary>
    public class CharacterAIExample : MonoBehaviour
    {
        private StateMachine aiStateMachine;
        private Transform player;
        private float detectionRange = 10f;
        private float attackRange = 2f;
        
        void Start()
        {
            // 创建AI状态机
            var config = new StateMachineConfig
            {
                // EnableDebugLog = true,
                // EnableStateHistory = true,
                // MaxHistoryCount = 20,
                // EnablePerformanceMonitoring = true
            };
            
            // aiStateMachine = StateMachineFactory.CreateAIStateMachine(this, config);
            
            // 设置AI参数
            aiStateMachine.SetBlackboardValue("DetectionRange", detectionRange);
            aiStateMachine.SetBlackboardValue("AttackRange", attackRange);
            aiStateMachine.SetBlackboardValue("Health", 100f);
            aiStateMachine.SetBlackboardValue("MaxHealth", 100f);
            
            // 添加AI状态
            aiStateMachine.AddNode<IdleState>();
            aiStateMachine.AddNode<PatrolState>();
            aiStateMachine.AddNode<ChaseState>();
            aiStateMachine.AddNode<AttackState>();
            aiStateMachine.AddNode<DeadState>();
            
            // 设置状态转换规则
            aiStateMachine.SetStateTransition(new AIStateTransition());
            
            // 开始AI
            aiStateMachine.Run<IdleState>();
            
            Debug.Log("[AI示例] 角色AI状态机启动");
        }
        
        void Update()
        {
            if (aiStateMachine != null)
            {
                // 更新玩家位置
                if (player != null)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                    aiStateMachine.SetBlackboardValue("DistanceToPlayer", distanceToPlayer);
                    aiStateMachine.SetBlackboardValue("PlayerPosition", player.position);
                }
                
                aiStateMachine.Update();
            }
        }
        
        void OnDestroy()
        {
            // aiStateMachine?.Dispose();
        }
        
        // AI状态定义
        public class IdleState : EnhancedBaseStateNode
        {
            public override int Priority => 1;
            private float idleTime = 0f;
            
            public override void OnEnter()
            {
                base.OnEnter();
                idleTime = 0f;
                Debug.Log("[AI] 进入待机状态");
            }
            
            public override void OnUpdate()
            {
                idleTime += Time.deltaTime;
                
                float distanceToPlayer = GetBlackboardValue<float>("DistanceToPlayer", float.MaxValue);
                float detectionRange = GetBlackboardValue<float>("DetectionRange");
                
                // 发现玩家
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState<ChaseState>();
                    return;
                }
                
                // 待机超时，开始巡逻
                if (idleTime > 3f)
                {
                    ChangeState<PatrolState>();
                }
            }
        }
        
        public class PatrolState : EnhancedBaseStateNode
        {
            public override int Priority => 2;
            private Vector3 patrolTarget;
            private float patrolSpeed = 2f;
            
            public override void OnEnter()
            {
                base.OnEnter();
                SetRandomPatrolTarget();
                Debug.Log("[AI] 开始巡逻");
            }
            
            public override void OnUpdate()
            {
                // 移动到巡逻点
                MoveToTarget(patrolTarget, patrolSpeed);
                
                // 检查是否到达巡逻点
                // if (Vector3.Distance(stateMachine.Owner.transform.position, patrolTarget) < 0.5f)
                {
                    ChangeState<IdleState>();
                }
                
                // 检查玩家
                float distanceToPlayer = GetBlackboardValue<float>("DistanceToPlayer", float.MaxValue);
                float detectionRange = GetBlackboardValue<float>("DetectionRange");
                
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState<ChaseState>();
                }
            }
            
            private void SetRandomPatrolTarget()
            {
                // Vector3 currentPos = stateMachine.Owner.transform.position;
                Vector3 randomDirection = Random.insideUnitSphere * 5f;
                randomDirection.y = 0;
                // patrolTarget = currentPos + randomDirection;
            }
            
            private void MoveToTarget(Vector3 target, float speed)
            {
                // Vector3 direction = (target - stateMachine.Owner.transform.position).normalized;
                // stateMachine.Owner.transform.position += direction * speed * Time.deltaTime;
            }
        }
        
        public class ChaseState : EnhancedBaseStateNode
        {
            public override int Priority => 5;
            private float chaseSpeed = 4f;
            
            public override void OnEnter()
            {
                base.OnEnter();
                Debug.Log("[AI] 开始追击玩家");
            }
            
            public override void OnUpdate()
            {
                Vector3 playerPos = GetBlackboardValue<Vector3>("PlayerPosition");
                float distanceToPlayer = GetBlackboardValue<float>("DistanceToPlayer");
                float attackRange = GetBlackboardValue<float>("AttackRange");
                float detectionRange = GetBlackboardValue<float>("DetectionRange");
                
                // 进入攻击范围
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState<AttackState>();
                    return;
                }
                
                // 玩家逃离检测范围
                if (distanceToPlayer > detectionRange * 1.5f)
                {
                    ChangeState<IdleState>();
                    return;
                }
                
                // 追击玩家
                // Vector3 direction = (playerPos - stateMachine.Owner.transform.position).normalized;
                // stateMachine.Owner.transform.position += direction * chaseSpeed * Time.deltaTime;
            }
        }
        
        public class AttackState : EnhancedBaseStateNode
        {
            public override int Priority => 8;
            private float attackCooldown = 1f;
            private float lastAttackTime = 0f;
            
            public override void OnEnter()
            {
                base.OnEnter();
                Debug.Log("[AI] 开始攻击");
            }
            
            public override void OnUpdate()
            {
                float distanceToPlayer = GetBlackboardValue<float>("DistanceToPlayer");
                float attackRange = GetBlackboardValue<float>("AttackRange");
                
                // 玩家离开攻击范围
                if (distanceToPlayer > attackRange)
                {
                    ChangeState<ChaseState>();
                    return;
                }
                
                // 执行攻击
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    PerformAttack();
                    lastAttackTime = Time.time;
                }
            }
            
            private void PerformAttack()
            {
                Debug.Log("[AI] 执行攻击！");
                // 这里添加攻击逻辑
            }
        }
        
        public class DeadState : EnhancedBaseStateNode
        {
            public override int Priority => 10;
            
            public override void OnEnter()
            {
                base.OnEnter();
                Debug.Log("[AI] 角色死亡");
            }
            
            public override bool CanExit()
            {
                return false; // 死亡状态不能退出
            }
        }
        
        // AI状态转换规则
        public class AIStateTransition : DefaultStateTransition
        {
            public override bool CanTransition(System.Type fromState, System.Type toState)
            {
                // 死亡状态不能转换到其他状态
                if (fromState == typeof(DeadState))
                    return false;
                
                return base.CanTransition(fromState, toState);
            }
        }
    }
    
    /// <summary>
    /// 示例2：UI界面状态机
    /// 展示如何使用状态机管理复杂的UI流程
    /// </summary>
    public class UIFlowExample : MonoBehaviour
    {
        private StateMachine uiStateMachine;
        
        [Header("UI面板")]
        public GameObject loginPanel;
        public GameObject mainMenuPanel;
        public GameObject inventoryPanel;
        public GameObject shopPanel;
        public GameObject settingsPanel;
        
        void Start()
        {
            // 创建UI状态机
            var config = new StateMachineConfig
            {
                // EnableDebugLog = true,
                // EnableStateHistory = true,
                // MaxHistoryCount = 10
            };
            
            // uiStateMachine = StateMachineFactory.CreateUIStateMachine(this, config);
            
            // 设置UI引用
            uiStateMachine.SetBlackboardValue("LoginPanel", loginPanel);
            uiStateMachine.SetBlackboardValue("MainMenuPanel", mainMenuPanel);
            uiStateMachine.SetBlackboardValue("InventoryPanel", inventoryPanel);
            uiStateMachine.SetBlackboardValue("ShopPanel", shopPanel);
            uiStateMachine.SetBlackboardValue("SettingsPanel", settingsPanel);
            
            // 添加UI状态
            uiStateMachine.AddNode<LoginUIState>();
            uiStateMachine.AddNode<MainMenuUIState>();
            uiStateMachine.AddNode<InventoryUIState>();
            uiStateMachine.AddNode<ShopUIState>();
            uiStateMachine.AddNode<SettingsUIState>();
            
            // 监听UI事件
            // uiStateMachine.OnStateChanged += OnUIStateChanged;
            
            // 开始UI流程
            uiStateMachine.Run<LoginUIState>();
            
            Debug.Log("[UI示例] UI状态机启动");
        }
        
        void Update()
        {
            uiStateMachine?.Update();
            
            // 全局UI输入处理
            HandleGlobalUIInput();
        }
        
        void OnDestroy()
        {
            if (uiStateMachine != null)
            {
                // uiStateMachine.OnStateChanged -= OnUIStateChanged;
                // uiStateMachine.Dispose();
            }
        }
        
        private void HandleGlobalUIInput()
        {
            // ESC键返回上一界面
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (uiStateMachine.GetStateStackDepth() > 0)
                {
                    uiStateMachine.PopState();
                }
            }
        }
        
        private void OnUIStateChanged(System.Type fromState, System.Type toState)
        {
            Debug.Log($"[UI示例] UI状态变化: {fromState?.Name ?? "None"} -> {toState.Name}");
        }
        
        // UI状态定义
        public class LoginUIState : EnhancedBaseStateNode
        {
            public override int Priority => 1;
            
            public override void OnEnter()
            {
                base.OnEnter();
                // ShowPanel("LoginPanel");
                Debug.Log("[UI] 显示登录界面");
            }
            
            public override void OnUpdate()
            {
                // 模拟登录成功
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    Debug.Log("[UI] 登录成功");
                    ChangeState<MainMenuUIState>();
                }
            }
            
            public override void OnExit()
            {
                base.OnExit();
                // HidePanel("LoginPanel");
            }
        }
        
        public class MainMenuUIState : EnhancedBaseStateNode
        {
            public override int Priority => 2;
            
            public override void OnEnter()
            {
                base.OnEnter();
                // ShowPanel("MainMenuPanel");
                Debug.Log("[UI] 显示主菜单");
            }
            
            public override void OnUpdate()
            {
                if (Input.GetKeyDown(KeyCode.I))
                {
                    Debug.Log("[UI] 打开背包");
                    stateMachine.PushState<InventoryUIState>();
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    Debug.Log("[UI] 打开商店");
                    stateMachine.PushState<ShopUIState>();
                }
                else if (Input.GetKeyDown(KeyCode.O))
                {
                    Debug.Log("[UI] 打开设置");
                    stateMachine.PushState<SettingsUIState>();
                }
            }
            
            public override void OnExit()
            {
                base.OnExit();
                // HidePanel("MainMenuPanel");
            }
            
            public override void OnPause()
            {
                base.OnPause();
                // 主菜单被暂停时可以保持显示
                Debug.Log("[UI] 主菜单暂停");
            }
            
            public override void OnResume()
            {
                base.OnResume();
                // ShowPanel("MainMenuPanel");
                Debug.Log("[UI] 主菜单恢复");
            }
        }
        
        public class InventoryUIState : EnhancedBaseStateNode
        {
            public override int Priority => 5;
            
            public override void OnEnter()
            {
                base.OnEnter();
                // ShowPanel("InventoryPanel");
                Debug.Log("[UI] 显示背包界面");
            }
            
            public override void OnUpdate()
            {
                if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Escape))
                {
                    stateMachine.PopState();
                }
            }
            
            public override void OnExit()
            {
                base.OnExit();
                // HidePanel("InventoryPanel");
            }
        }
        
        public class ShopUIState : EnhancedBaseStateNode
        {
            public override int Priority => 5;
            
            public override void OnEnter()
            {
                base.OnEnter();
                // ShowPanel("ShopPanel");
                Debug.Log("[UI] 显示商店界面");
            }
            
            public override void OnUpdate()
            {
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.Escape))
                {
                    stateMachine.PopState();
                }
            }
            
            public override void OnExit()
            {
                base.OnExit();
                // HidePanel("ShopPanel");
            }
        }
        
        public class SettingsUIState : EnhancedBaseStateNode
        {
            public override int Priority => 8;
            
            public override void OnEnter()
            {
                base.OnEnter();
                // ShowPanel("SettingsPanel");
                Debug.Log("[UI] 显示设置界面");
            }
            
            public override void OnUpdate()
            {
                if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.Escape))
                {
                    stateMachine.PopState();
                }
            }
            
            public override void OnExit()
            {
                base.OnExit();
                // HidePanel("SettingsPanel");
            }
        }
    }
    
    /// <summary>
    /// 示例3：关卡状态机
    /// 展示如何使用状态机管理关卡流程
    /// </summary>
    public class LevelFlowExample : MonoBehaviour
    {
        private StateMachine levelStateMachine;
        
        [Header("关卡配置")]
        public int totalWaves = 5;
        public float waveDuration = 30f;
        public float restDuration = 10f;
        
        void Start()
        {
            // 创建关卡状态机
            var config = new StateMachineConfig
            {
                // EnableDebugLog = true,
                // EnablePerformanceMonitoring = true,
                // MaxStateCapacity = 20
            };
            
            // levelStateMachine = StateMachineFactory.CreateGameStateMachine(this, config);
            
            // 设置关卡参数
            levelStateMachine.SetBlackboardValue("TotalWaves", totalWaves);
            levelStateMachine.SetBlackboardValue("WaveDuration", waveDuration);
            levelStateMachine.SetBlackboardValue("RestDuration", restDuration);
            levelStateMachine.SetBlackboardValue("CurrentWave", 0);
            levelStateMachine.SetBlackboardValue("EnemiesRemaining", 0);
            
            // 添加关卡状态
            levelStateMachine.AddNode<LevelStartState>();
            levelStateMachine.AddNode<WavePreparationState>();
            levelStateMachine.AddNode<WaveActiveState>();
            levelStateMachine.AddNode<WaveCompleteState>();
            levelStateMachine.AddNode<LevelCompleteState>();
            levelStateMachine.AddNode<LevelFailedState>();
            
            // 开始关卡
            levelStateMachine.Run<LevelStartState>();
            
            Debug.Log("[关卡示例] 关卡状态机启动");
        }
        
        void Update()
        {
            levelStateMachine?.Update();
        }
        
        void OnDestroy()
        {
            // levelStateMachine?.Dispose();
        }
        
        // 关卡状态定义
        public class LevelStartState : EnhancedBaseStateNode
        {
            public override int Priority => 1;
            
            public override void OnEnter()
            {
                base.OnEnter();
                Debug.Log("[关卡] 关卡开始");
                
                // 初始化关卡
                SetBlackboardValue("CurrentWave", 1);
                SetBlackboardValue("PlayerHealth", 100f);
                SetBlackboardValue("LevelStartTime", Time.time);
                
                // 延迟后开始第一波
                // StartCoroutine(DelayedStart());
            }
            
            private IEnumerator DelayedStart()
            {
                yield return new WaitForSeconds(2f);
                ChangeState<WavePreparationState>();
            }
        }
        
        public class WavePreparationState : EnhancedBaseStateNode
        {
            public override int Priority => 3;
            private float preparationTime = 3f;
            private float timer = 0f;
            
            public override void OnEnter()
            {
                base.OnEnter();
                timer = 0f;
                
                int currentWave = GetBlackboardValue<int>("CurrentWave");
                Debug.Log($"[关卡] 准备第 {currentWave} 波");
                
                SetBlackboardValue("WavePreparationTime", preparationTime);
            }
            
            public override void OnUpdate()
            {
                timer += Time.deltaTime;
                SetBlackboardValue("PreparationTimeRemaining", preparationTime - timer);
                
                if (timer >= preparationTime)
                {
                    ChangeState<WaveActiveState>();
                }
            }
        }
        
        public class WaveActiveState : EnhancedBaseStateNode
        {
            public override int Priority => 5;
            private float waveTimer = 0f;
            
            public override void OnEnter()
            {
                base.OnEnter();
                waveTimer = 0f;
                
                int currentWave = GetBlackboardValue<int>("CurrentWave");
                float waveDuration = GetBlackboardValue<float>("WaveDuration");
                
                Debug.Log($"[关卡] 第 {currentWave} 波开始");
                
                // 生成敌人
                SpawnEnemies(currentWave);
                
                SetBlackboardValue("WaveStartTime", Time.time);
                SetBlackboardValue("WaveActive", true);
            }
            
            public override void OnUpdate()
            {
                waveTimer += Time.deltaTime;
                float waveDuration = GetBlackboardValue<float>("WaveDuration");
                
                SetBlackboardValue("WaveTimeRemaining", waveDuration - waveTimer);
                
                // 检查波次完成条件
                int enemiesRemaining = GetBlackboardValue<int>("EnemiesRemaining");
                float playerHealth = GetBlackboardValue<float>("PlayerHealth");
                
                // 玩家死亡
                if (playerHealth <= 0)
                {
                    ChangeState<LevelFailedState>();
                    return;
                }
                
                // 敌人全部消灭或时间到
                if (enemiesRemaining <= 0 || waveTimer >= waveDuration)
                {
                    ChangeState<WaveCompleteState>();
                    return;
                }
                
                // 模拟敌人减少
                if (Random.Range(0f, 1f) < 0.01f && enemiesRemaining > 0)
                {
                    SetBlackboardValue("EnemiesRemaining", enemiesRemaining - 1);
                }
            }
            
            public override void OnExit()
            {
                base.OnExit();
                SetBlackboardValue("WaveActive", false);
            }
            
            private void SpawnEnemies(int waveNumber)
            {
                int enemyCount = waveNumber * 3; // 每波敌人数量递增
                SetBlackboardValue("EnemiesRemaining", enemyCount);
                Debug.Log($"[关卡] 生成 {enemyCount} 个敌人");
            }
        }
        
        public class WaveCompleteState : EnhancedBaseStateNode
        {
            public override int Priority => 4;
            
            public override void OnEnter()
            {
                base.OnEnter();
                
                int currentWave = GetBlackboardValue<int>("CurrentWave");
                int totalWaves = GetBlackboardValue<int>("TotalWaves");
                
                Debug.Log($"[关卡] 第 {currentWave} 波完成");
                
                // 给予奖励
                GiveWaveReward(currentWave);
                
                // 检查是否完成所有波次
                if (currentWave >= totalWaves)
                {
                    ChangeState<LevelCompleteState>();
                }
                else
                {
                    // 准备下一波
                    SetBlackboardValue("CurrentWave", currentWave + 1);
                    // StartCoroutine(RestPeriod());
                }
            }
            
            private IEnumerator RestPeriod()
            {
                float restDuration = GetBlackboardValue<float>("RestDuration");
                yield return new WaitForSeconds(restDuration);
                ChangeState<WavePreparationState>();
            }
            
            private void GiveWaveReward(int waveNumber)
            {
                int reward = waveNumber * 100;
                Debug.Log($"[关卡] 获得奖励: {reward} 分");
            }
        }
        
        public class LevelCompleteState : EnhancedBaseStateNode
        {
            public override int Priority => 8;
            
            public override void OnEnter()
            {
                base.OnEnter();
                
                float levelStartTime = GetBlackboardValue<float>("LevelStartTime");
                float completionTime = Time.time - levelStartTime;
                
                Debug.Log($"[关卡] 关卡完成！用时: {completionTime:F2} 秒");
                
                SetBlackboardValue("LevelCompleted", true);
                SetBlackboardValue("CompletionTime", completionTime);
                
                // 计算最终分数
                CalculateFinalScore();
            }
            
            private void CalculateFinalScore()
            {
                float completionTime = GetBlackboardValue<float>("CompletionTime");
                float playerHealth = GetBlackboardValue<float>("PlayerHealth");
                
                int timeBonus = Mathf.Max(0, (int)(1000 - completionTime * 10));
                int healthBonus = (int)(playerHealth * 10);
                int finalScore = timeBonus + healthBonus;
                
                SetBlackboardValue("FinalScore", finalScore);
                Debug.Log($"[关卡] 最终分数: {finalScore}");
            }
        }
        
        public class LevelFailedState : EnhancedBaseStateNode
        {
            public override int Priority => 8;
            
            public override void OnEnter()
            {
                base.OnEnter();
                
                int currentWave = GetBlackboardValue<int>("CurrentWave");
                Debug.Log($"[关卡] 关卡失败！在第 {currentWave} 波");
                
                SetBlackboardValue("LevelFailed", true);
                SetBlackboardValue("FailedWave", currentWave);
            }
        }
    }
}

/// <summary>
/// 状态机最佳实践指南
/// </summary>
public static class StateMachineBestPractices
{
    /// <summary>
    /// 性能优化建议
    /// </summary>
    public static class PerformanceOptimization
    {
        // 1. 使用对象池避免频繁的内存分配
        public static void UseObjectPooling()
        {
            // 示例：为状态转换事件使用对象池
            // 在StateMachine中实现事件对象的复用
        }
        
        // 2. 缓存频繁访问的黑板数据
        public static void CacheBlackboardData()
        {
            // 示例：在状态中缓存经常使用的数据
            // private float cachedPlayerHealth;
            // public override void OnEnter() {
            //     cachedPlayerHealth = GetBlackboardValue<float>("PlayerHealth");
            // }
        }
        
        // 3. 避免在Update中进行复杂计算
        public static void OptimizeUpdateLogic()
        {
            // 示例：使用定时器减少计算频率
            // private float updateInterval = 0.1f;
            // private float lastUpdateTime = 0f;
            // 
            // public override void OnUpdate() {
            //     if (Time.time - lastUpdateTime >= updateInterval) {
            //         // 执行复杂逻辑
            //         lastUpdateTime = Time.time;
            //     }
            // }
        }
    }
    
    /// <summary>
    /// 状态设计原则
    /// </summary>
    public static class StateDesignPrinciples
    {
        // 1. 单一职责原则 - 每个状态只负责一个明确的行为
        // 2. 状态转换清晰 - 明确定义何时可以从一个状态转换到另一个状态
        // 3. 数据封装 - 使用黑板系统共享数据，避免状态间直接依赖
        // 4. 优先级设计 - 合理设置状态优先级，确保重要状态不被意外打断
        // 5. 错误处理 - 在状态转换和执行中添加适当的错误处理
    }
    
    /// <summary>
    /// 调试和监控建议
    /// </summary>
    public static class DebuggingAndMonitoring
    {
        // 1. 启用状态历史记录
        public static StateMachineConfig GetDebugConfig()
        {
            return new StateMachineConfig
            {
                // EnableDebugLog = true,
                // EnableStateHistory = true,
                // MaxHistoryCount = 50,
                // EnablePerformanceMonitoring = true,
                // EnableSafetyChecks = true
            };
        }
        
        // 2. 实现自定义调试信息
        public static void ImplementCustomDebugInfo()
        {
            // 在状态中重写GetDebugInfo方法
            // public override string GetDebugInfo() {
            //     return base.GetDebugInfo() + 
            //            $"\n自定义数据: {customValue}";
            // }
        }
        
        // 3. 使用性能监控
        public static void MonitorPerformance(StateMachine stateMachine)
        {
            // var profiler = stateMachine.GetComponent<StateMachineProfiler>();
            // if (profiler != null)
            // {
            //     Debug.Log($"状态机性能报告:\n{profiler.GetPerformanceReport()}");
            // }
        }
    }
    
    /// <summary>
    /// 常见错误和解决方案
    /// </summary>
    public static class CommonMistakesAndSolutions
    {
        // 错误1：在状态间直接引用
        // 解决：使用黑板系统进行数据共享
        
        // 错误2：状态转换逻辑过于复杂
        // 解决：使用状态转换管理器，集中处理转换逻辑
        
        // 错误3：忘记清理资源
        // 解决：在OnExit和Dispose中正确清理资源
        
        // 错误4：状态优先级设置不当
        // 解决：仔细设计优先级层次，确保重要状态不被打断
        
        // 错误5：过度使用状态栈
        // 解决：只在真正需要暂停/恢复的场景使用状态栈
    }
}