using UnityEngine;
using Framework.CombatSystem.Core;
using Framework.CombatSystem.Entities;
using Framework.CombatSystem.Components;
using Framework.CombatSystem.Presentation;

namespace Framework.CombatSystem
{
    /// <summary>
    /// 战斗系统演示类
    /// 体现：门面模式 - 为复杂的战斗系统提供简单的接口
    /// 体现：工厂模式 - 创建不同类型的角色
    /// 体现：组合模式 - 整合逻辑层和表现层
    /// 这是一个完整的演示类，展示如何使用整个战斗框架
    /// 目的：教学演示抽象类、接口、继承、组合等面向对象设计原则
    /// </summary>
    public class CombatSystemDemo : MonoBehaviour
    {
        #region 字段
        
        /// <summary>
        /// 战斗管理器
        /// </summary>
        private CombatManager _combatManager;
        
        /// <summary>
        /// 战斗UI管理器
        /// </summary>
        private CombatUIManager _combatUIManager;
        
        /// <summary>
        /// 演示玩家逻辑
        /// </summary>
        private PlayerLogic _demoPlayerLogic;
        
        /// <summary>
        /// 演示玩家视图
        /// </summary>
        private PlayerView _demoPlayerView;
        
        /// <summary>
        /// 演示敌人列表
        /// </summary>
        private System.Collections.Generic.List<EnemyLogic> _demoEnemies;
        
        /// <summary>
        /// 演示怪物列表
        /// </summary>
        private System.Collections.Generic.List<MonsterLogic> _demoMonsters;
        
        /// <summary>
        /// 玩家预制体
        /// </summary>
        [Header("预制体配置")]
        [SerializeField] private GameObject _playerPrefab;
        
        /// <summary>
        /// 敌人预制体
        /// </summary>
        [SerializeField] private GameObject _enemyPrefab;
        
        /// <summary>
        /// 怪物预制体
        /// </summary>
        [SerializeField] private GameObject _monsterPrefab;
        
        /// <summary>
        /// 战斗UI管理器预制体
        /// </summary>
        [SerializeField] private GameObject _combatUIManagerPrefab;
        
        /// <summary>
        /// 演示配置
        /// </summary>
        [Header("演示配置")]
        [SerializeField] private bool _autoStartDemo = true;
        
        /// <summary>
        /// 敌人数量
        /// </summary>
        [SerializeField] private int _enemyCount = 3;
        
        /// <summary>
        /// 怪物数量
        /// </summary>
        [SerializeField] private int _monsterCount = 2;
        
        /// <summary>
        /// 生成范围
        /// </summary>
        [SerializeField] private float _spawnRange = 10f;
        
        /// <summary>
        /// 是否显示调试信息
        /// </summary>
        [SerializeField] private bool _showDebugInfo = true;
        
        /// <summary>
        /// 演示状态
        /// </summary>
        private DemoState _currentState = DemoState.NotStarted;
        
        /// <summary>
        /// 演示开始时间
        /// </summary>
        private float _demoStartTime;
        
        #endregion
        
        #region 枚举
        
        /// <summary>
        /// 演示状态枚举
        /// </summary>
        public enum DemoState
        {
            NotStarted,     // 未开始
            Initializing,   // 初始化中
            Running,        // 运行中
            Paused,         // 暂停
            Completed       // 完成
        }
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 当前演示状态
        /// </summary>
        public DemoState CurrentState => _currentState;
        
        /// <summary>
        /// 战斗管理器
        /// </summary>
        public CombatManager CombatManager => _combatManager;
        
        /// <summary>
        /// 演示玩家逻辑
        /// </summary>
        public PlayerLogic DemoPlayerLogic => _demoPlayerLogic;
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Start方法
        /// </summary>
        private void Start()
        {
            if (_autoStartDemo)
            {
                StartDemo();
            }
        }
        
        /// <summary>
        /// Unity Update方法
        /// </summary>
        private void Update()
        {
            // 处理输入
            HandleInput();
            
            // 更新演示状态
            UpdateDemoState();
            
            // 显示调试信息
            if (_showDebugInfo)
            {
                DisplayDebugInfo();
            }
        }
        
        /// <summary>
        /// Unity OnGUI方法（用于显示调试信息）
        /// </summary>
        private void OnGUI()
        {
            if (!_showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 600));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== 战斗系统演示 ===", GUI.skin.label);
            GUILayout.Label($"演示状态: {_currentState}");
            
            if (_combatManager != null)
            {
                GUILayout.Label($"战斗状态: {(_combatManager.IsInCombat ? "进行中" : "未开始")}");
                GUILayout.Label($"总角色数: {_combatManager.GetAllCharacterCount()}");
                GUILayout.Label($"玩家数: {_combatManager.GetPlayerCount()}");
                GUILayout.Label($"敌人数: {_combatManager.GetAliveEnemyCount()}");
                GUILayout.Label($"怪物数: {_combatManager.GetAliveMonsterCount()}");
            }
            
            GUILayout.Space(10);
            GUILayout.Label("=== 控制说明 ===");
            GUILayout.Label("WASD: 移动");
            GUILayout.Label("鼠标左键: 攻击");
            GUILayout.Label("Q/E/R/F: 技能");
            GUILayout.Label("Space: 交互");
            GUILayout.Label("Tab: 切换目标");
            GUILayout.Label("1-4: 快捷技能");
            
            GUILayout.Space(10);
            GUILayout.Label("=== 演示控制 ===");
            
            if (GUILayout.Button("开始演示"))
            {
                StartDemo();
            }
            
            if (GUILayout.Button("暂停/继续"))
            {
                TogglePause();
            }
            
            if (GUILayout.Button("重置演示"))
            {
                ResetDemo();
            }
            
            if (GUILayout.Button("添加敌人"))
            {
                CreateRandomEnemy();
            }
            
            if (GUILayout.Button("添加怪物"))
            {
                CreateRandomMonster();
            }
            
            GUILayout.Space(10);
            GUILayout.Label("=== 面向对象设计原则演示 ===");
            GUILayout.Label("✓ 抽象类: GameCharacterLogic");
            GUILayout.Label("✓ 接口: IMovable, IAttacker, IAIBehavior");
            GUILayout.Label("✓ 继承: PlayerLogic, EnemyLogic, MonsterLogic");
            GUILayout.Label("✓ 组合: 角色包含多个组件");
            GUILayout.Label("✓ 多态: 不同角色类型的统一处理");
            GUILayout.Label("✓ 封装: 逻辑与表现分离");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        #endregion
        
        #region 演示控制方法
        
        /// <summary>
        /// 开始演示
        /// </summary>
        public void StartDemo()
        {
            if (_currentState == DemoState.Running) return;
            
            Debug.Log("[CombatSystemDemo] 开始战斗系统演示");
            
            _currentState = DemoState.Initializing;
            _demoStartTime = Time.time;
            
            // 初始化战斗系统
            InitializeCombatSystem();
            
            // 创建演示角色
            CreateDemoCharacters();
            
            // 开始战斗
            StartCombat();
            
            _currentState = DemoState.Running;
            
            Debug.Log("[CombatSystemDemo] 演示初始化完成");
        }
        
        /// <summary>
        /// 暂停/继续演示
        /// </summary>
        public void TogglePause()
        {
            switch (_currentState)
            {
                case DemoState.Running:
                    _currentState = DemoState.Paused;
                    Time.timeScale = 0f;
                    Debug.Log("[CombatSystemDemo] 演示已暂停");
                    break;
                    
                case DemoState.Paused:
                    _currentState = DemoState.Running;
                    Time.timeScale = 1f;
                    Debug.Log("[CombatSystemDemo] 演示已继续");
                    break;
            }
        }
        
        /// <summary>
        /// 重置演示
        /// </summary>
        public void ResetDemo()
        {
            Debug.Log("[CombatSystemDemo] 重置演示");
            
            _currentState = DemoState.NotStarted;
            Time.timeScale = 1f;
            
            // 清理现有角色
            CleanupDemo();
            
            // 重新开始
            if (_autoStartDemo)
            {
                StartDemo();
            }
        }
        
        /// <summary>
        /// 清理演示
        /// </summary>
        private void CleanupDemo()
        {
            // 清理战斗管理器
            if (_combatManager != null)
            {
                _combatManager.EndCombat();
                _combatManager.ClearAllCharacters();
            }
            
            // 清理角色列表
            _demoEnemies?.Clear();
            _demoMonsters?.Clear();
            
            // 清理场景中的角色对象
            CleanupSceneObjects();
        }
        
        /// <summary>
        /// 清理场景对象
        /// </summary>
        private void CleanupSceneObjects()
        {
            // 清理玩家
            if (_demoPlayerView != null)
            {
                DestroyImmediate(_demoPlayerView.gameObject);
                _demoPlayerView = null;
            }
            
            // 清理敌人
            var enemyViews = FindObjectsOfType<EnemyView>();
            foreach (var enemyView in enemyViews)
            {
                DestroyImmediate(enemyView.gameObject);
            }
            
            // 清理怪物
            var monsterViews = FindObjectsOfType<MonsterView>();
            foreach (var monsterView in monsterViews)
            {
                DestroyImmediate(monsterView.gameObject);
            }
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化战斗系统
        /// </summary>
        private void InitializeCombatSystem()
        {
            // 创建战斗管理器
            if (_combatManager == null)
            {
                GameObject combatManagerObj = new GameObject("CombatManager");
                combatManagerObj.transform.SetParent(transform);
                _combatManager = combatManagerObj.AddComponent<CombatManager>();
                _combatManager.Initialize();
            }
            
            // 创建战斗UI管理器
            if (_combatUIManager == null && _combatUIManagerPrefab != null)
            {
                GameObject uiManagerObj = Instantiate(_combatUIManagerPrefab, transform);
                _combatUIManager = uiManagerObj.GetComponent<CombatUIManager>();
                if (_combatUIManager != null)
                {
                    _combatUIManager.Initialize(_combatManager);
                }
            }
            
            // 初始化角色列表
            _demoEnemies = new System.Collections.Generic.List<EnemyLogic>();
            _demoMonsters = new System.Collections.Generic.List<MonsterLogic>();
        }
        
        #endregion
        
        #region 角色创建方法
        
        /// <summary>
        /// 创建演示角色
        /// </summary>
        private void CreateDemoCharacters()
        {
            // 创建玩家
            CreateDemoPlayer();
            
            // 创建敌人
            for (int i = 0; i < _enemyCount; i++)
            {
                CreateRandomEnemy();
            }
            
            // 创建怪物
            for (int i = 0; i < _monsterCount; i++)
            {
                CreateRandomMonster();
            }
        }
        
        /// <summary>
        /// 创建演示玩家
        /// </summary>
        private void CreateDemoPlayer()
        {
            // 创建玩家逻辑
            _demoPlayerLogic = new PlayerLogic("演示玩家", 100f, Vector3Logic.Zero);
            
            // 添加移动组件
            var groundMovement = new GroundMovementComponent(_demoPlayerLogic, 5f);
            _demoPlayerLogic.AddComponent(groundMovement);
            
            // 添加攻击组件
            var meleeAttack = new MeleeAttackComponent(_demoPlayerLogic, 20f, 2f, 1.5f);
            _demoPlayerLogic.AddComponent(meleeAttack);
            
            // 创建玩家视图
            if (_playerPrefab != null)
            {
                GameObject playerObj = Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
                _demoPlayerView = playerObj.GetComponent<PlayerView>();
                
                if (_demoPlayerView == null)
                {
                    _demoPlayerView = playerObj.AddComponent<PlayerView>();
                }
                
                _demoPlayerView.BindCharacterLogic(_demoPlayerLogic);
                
                // 添加输入控制器
                var inputController = playerObj.GetComponent<PlayerInputController>();
                if (inputController == null)
                {
                    inputController = playerObj.AddComponent<PlayerInputController>();
                }
                inputController.Initialize(_demoPlayerLogic);
            }
            
            // 添加到战斗管理器
            _combatManager.AddCharacter(_demoPlayerLogic);
            
            // 绑定到UI管理器
            if (_combatUIManager != null)
            {
                _combatUIManager.BindPlayerLogic(_demoPlayerLogic);
            }
            
            Debug.Log("[CombatSystemDemo] 创建演示玩家完成");
        }
        
        /// <summary>
        /// 创建随机敌人
        /// </summary>
        public void CreateRandomEnemy()
        {
            // 随机位置
            Vector3 randomPos = GetRandomSpawnPosition();
            Vector3Logic logicPos = new Vector3Logic(randomPos.x, randomPos.y, randomPos.z);
            
            // 创建敌人逻辑
            string enemyName = $"敌人_{_demoEnemies.Count + 1}";
            var enemyLogic = new EnemyLogic(enemyName, Random.Range(80f, 120f), logicPos);
            enemyLogic.SetLevel(Random.Range(1, 5));
            
            // 添加移动组件
            var groundMovement = new GroundMovementComponent(enemyLogic, 3f);
            enemyLogic.AddComponent(groundMovement);
            
            // 添加攻击组件
            var meleeAttack = new MeleeAttackComponent(enemyLogic, 15f, 1.8f, 2f);
            enemyLogic.AddComponent(meleeAttack);
            
            // 设置巡逻路径
            SetRandomPatrolPath(enemyLogic, randomPos);
            
            // 创建敌人视图
            if (_enemyPrefab != null)
            {
                GameObject enemyObj = Instantiate(_enemyPrefab, randomPos, Quaternion.identity);
                var enemyView = enemyObj.GetComponent<EnemyView>();
                
                if (enemyView == null)
                {
                    enemyView = enemyObj.AddComponent<EnemyView>();
                }
                
                enemyView.BindCharacterLogic(enemyLogic);
            }
            
            // 添加到列表和战斗管理器
            _demoEnemies.Add(enemyLogic);
            _combatManager.AddCharacter(enemyLogic);
            
            Debug.Log($"[CombatSystemDemo] 创建敌人: {enemyName}");
        }
        
        /// <summary>
        /// 创建随机怪物
        /// </summary>
        public void CreateRandomMonster()
        {
            // 随机位置
            Vector3 randomPos = GetRandomSpawnPosition();
            Vector3Logic logicPos = new Vector3Logic(randomPos.x, randomPos.y, randomPos.z);
            
            // 随机怪物类型
            MonsterType[] types = { MonsterType.Normal, MonsterType.Elite, MonsterType.Rare };
            MonsterType randomType = types[Random.Range(0, types.Length)];
            
            // 创建怪物逻辑
            string monsterName = $"怪物_{_demoMonsters.Count + 1}";
            var monsterLogic = new MonsterLogic(monsterName, Random.Range(150f, 200f), logicPos, randomType);
            monsterLogic.SetLevel(Random.Range(2, 6));
            
            // 添加移动组件
            var groundMovement = new GroundMovementComponent(monsterLogic, 4f);
            monsterLogic.AddComponent(groundMovement);
            
            // 添加攻击组件（根据类型选择不同攻击方式）
            if (randomType == MonsterType.Elite)
            {
                var rangedAttack = new RangedAttackComponent(monsterLogic, 25f, 6f, 2f, 10f);
                monsterLogic.AddComponent(rangedAttack);
            }
            else
            {
                var meleeAttack = new MeleeAttackComponent(monsterLogic, 20f, 2f, 1.8f);
                monsterLogic.AddComponent(meleeAttack);
            }
            
            // 创建怪物视图
            if (_monsterPrefab != null)
            {
                GameObject monsterObj = Instantiate(_monsterPrefab, randomPos, Quaternion.identity);
                var monsterView = monsterObj.GetComponent<MonsterView>();
                
                if (monsterView == null)
                {
                    monsterView = monsterObj.AddComponent<MonsterView>();
                }
                
                monsterView.BindCharacterLogic(monsterLogic);
            }
            
            // 添加到列表和战斗管理器
            _demoMonsters.Add(monsterLogic);
            _combatManager.AddCharacter(monsterLogic);
            
            Debug.Log($"[CombatSystemDemo] 创建怪物: {monsterName} (类型: {randomType})");
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取随机生成位置
        /// </summary>
        /// <returns>随机位置</returns>
        private Vector3 GetRandomSpawnPosition()
        {
            float x = Random.Range(-_spawnRange, _spawnRange);
            float z = Random.Range(-_spawnRange, _spawnRange);
            return new Vector3(x, 0f, z);
        }
        
        /// <summary>
        /// 设置随机巡逻路径
        /// </summary>
        /// <param name="enemyLogic">敌人逻辑</param>
        /// <param name="centerPos">中心位置</param>
        private void SetRandomPatrolPath(EnemyLogic enemyLogic, Vector3 centerPos)
        {
            var patrolPoints = new System.Collections.Generic.List<Vector3Logic>();
            int pointCount = Random.Range(2, 5);
            
            for (int i = 0; i < pointCount; i++)
            {
                float angle = (360f / pointCount) * i * Mathf.Deg2Rad;
                float radius = Random.Range(3f, 8f);
                
                float x = centerPos.x + Mathf.Cos(angle) * radius;
                float z = centerPos.z + Mathf.Sin(angle) * radius;
                
                patrolPoints.Add(new Vector3Logic(x, centerPos.y, z));
            }
            
            enemyLogic.SetPatrolPath(patrolPoints);
        }
        
        /// <summary>
        /// 开始战斗
        /// </summary>
        private void StartCombat()
        {
            if (_combatManager != null)
            {
                _combatManager.StartCombat();
            }
        }
        
        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            // F1 - 切换调试信息显示
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _showDebugInfo = !_showDebugInfo;
            }
            
            // F2 - 暂停/继续
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TogglePause();
            }
            
            // F3 - 重置演示
            if (Input.GetKeyDown(KeyCode.F3))
            {
                ResetDemo();
            }
            
            // F4 - 添加敌人
            if (Input.GetKeyDown(KeyCode.F4))
            {
                CreateRandomEnemy();
            }
            
            // F5 - 添加怪物
            if (Input.GetKeyDown(KeyCode.F5))
            {
                CreateRandomMonster();
            }
        }
        
        /// <summary>
        /// 更新演示状态
        /// </summary>
        private void UpdateDemoState()
        {
            if (_currentState != DemoState.Running) return;
            
            // 检查战斗是否结束
            if (_combatManager != null && _combatManager.IsInCombat)
            {
                // 检查胜利条件
                if (_combatManager.GetAliveEnemyCount() == 0 && _combatManager.GetAliveMonsterCount() == 0)
                {
                    // 玩家胜利
                    _combatManager.EndCombat();
                    _currentState = DemoState.Completed;
                    Debug.Log("[CombatSystemDemo] 演示完成 - 玩家胜利！");
                }
                else if (_combatManager.GetPlayerCount() == 0)
                {
                    // 玩家失败
                    _combatManager.EndCombat();
                    _currentState = DemoState.Completed;
                    Debug.Log("[CombatSystemDemo] 演示完成 - 玩家失败！");
                }
            }
        }
        
        /// <summary>
        /// 显示调试信息
        /// </summary>
        private void DisplayDebugInfo()
        {
            // 这里可以添加更多的调试信息显示逻辑
            // 目前主要通过OnGUI显示
        }
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity OnDestroy方法
        /// </summary>
        private void OnDestroy()
        {
            // 清理演示
            CleanupDemo();
            
            // 恢复时间缩放
            Time.timeScale = 1f;
        }
        
        #endregion
        
        #region 公共接口方法
        
        /// <summary>
        /// 获取演示统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        public string GetDemoStatistics()
        {
            if (_combatManager == null) return "战斗管理器未初始化";
            
            float runTime = _currentState == DemoState.Running ? Time.time - _demoStartTime : 0f;
            
            return $"演示统计信息:\n" +
                   $"运行时间: {runTime:F1}秒\n" +
                   $"当前状态: {_currentState}\n" +
                   $"总角色数: {_combatManager.GetAllCharacterCount()}\n" +
                   $"存活敌人: {_combatManager.GetAliveEnemyCount()}\n" +
                   $"存活怪物: {_combatManager.GetAliveMonsterCount()}\n" +
                   $"玩家数量: {_combatManager.GetPlayerCount()}";
        }
        
        /// <summary>
        /// 获取面向对象设计原则演示说明
        /// </summary>
        /// <returns>设计原则说明</returns>
        public string GetOOPDemonstration()
        {
            return "=== 面向对象设计原则演示 ===\n\n" +
                   "1. 抽象类 (Abstract Class):\n" +
                   "   - GameCharacterLogic: 定义角色的通用行为和属性\n" +
                   "   - MovementComponent: 定义移动组件的基础结构\n" +
                   "   - AttackComponent: 定义攻击组件的基础结构\n\n" +
                   
                   "2. 接口 (Interface):\n" +
                   "   - IMovable: 定义移动能力契约\n" +
                   "   - IAttacker: 定义攻击能力契约\n" +
                   "   - IAIBehavior: 定义AI行为契约\n" +
                   "   - IControllable: 定义控制能力契约\n\n" +
                   
                   "3. 继承 (Inheritance):\n" +
                   "   - PlayerLogic继承GameCharacterLogic\n" +
                   "   - EnemyLogic继承GameCharacterLogic\n" +
                   "   - MonsterLogic继承GameCharacterLogic\n" +
                   "   - 各种具体组件继承抽象组件基类\n\n" +
                   
                   "4. 组合 (Composition):\n" +
                   "   - 角色包含多个功能组件\n" +
                   "   - 战斗管理器管理多个角色\n" +
                   "   - UI管理器组合多个UI组件\n\n" +
                   
                   "5. 多态 (Polymorphism):\n" +
                   "   - 不同角色类型统一处理\n" +
                   "   - 不同组件类型统一管理\n" +
                   "   - 虚方法重写实现不同行为\n\n" +
                   
                   "6. 封装 (Encapsulation):\n" +
                   "   - 逻辑层与表现层分离\n" +
                   "   - 私有字段和公共接口\n" +
                   "   - 组件内部实现隐藏\n\n" +
                   
                   "7. 设计模式:\n" +
                   "   - 观察者模式: 事件系统\n" +
                   "   - 组合模式: 角色组件系统\n" +
                   "   - 工厂模式: 角色创建\n" +
                   "   - 门面模式: 简化复杂接口";
        }
        
        #endregion
    }
}