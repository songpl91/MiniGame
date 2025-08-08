using UnityEngine;
using Framework.CombatSystem.Core;

namespace Framework.CombatSystem
{
    /// <summary>
    /// 战斗系统管理器
    /// 体现：单例模式 - 全局唯一的战斗系统管理器
    /// 体现：门面模式 - 为整个战斗系统提供统一的访问接口
    /// 体现：组合模式 - 管理多个子系统
    /// 这是战斗系统的顶层管理器，负责协调各个子系统
    /// 目的：提供统一的战斗系统访问入口，简化外部调用
    /// </summary>
    public class CombatSystemManager : MonoBehaviour
    {
        #region 单例模式实现
        
        /// <summary>
        /// 单例实例
        /// </summary>
        private static CombatSystemManager _instance;
        
        /// <summary>
        /// 单例属性
        /// </summary>
        public static CombatSystemManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CombatSystemManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CombatSystemManager");
                        _instance = go.AddComponent<CombatSystemManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 字段
        
        /// <summary>
        /// 核心战斗管理器
        /// </summary>
        private CombatManager _combatManager;
        
        /// <summary>
        /// 战斗系统演示
        /// </summary>
        private CombatSystemDemo _combatDemo;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized = false;
        
        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        [Header("系统配置")]
        [SerializeField] private bool _enableDebugMode = true;
        
        /// <summary>
        /// 是否自动启动演示
        /// </summary>
        [SerializeField] private bool _autoStartDemo = false;
        
        /// <summary>
        /// 演示预制体
        /// </summary>
        [Header("演示配置")]
        [SerializeField] private GameObject _demoPrefab;
        
        /// <summary>
        /// 系统状态
        /// </summary>
        private SystemState _currentState = SystemState.Uninitialized;
        
        #endregion
        
        #region 枚举
        
        /// <summary>
        /// 系统状态枚举
        /// </summary>
        public enum SystemState
        {
            Uninitialized,  // 未初始化
            Initializing,   // 初始化中
            Ready,          // 就绪
            Running,        // 运行中
            Paused,         // 暂停
            Error           // 错误
        }
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 当前系统状态
        /// </summary>
        public SystemState CurrentState => _currentState;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// 核心战斗管理器
        /// </summary>
        public CombatManager CombatManager => _combatManager;
        
        /// <summary>
        /// 战斗系统演示
        /// </summary>
        public CombatSystemDemo CombatDemo => _combatDemo;
        
        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        public bool EnableDebugMode
        {
            get => _enableDebugMode;
            set => _enableDebugMode = value;
        }
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法
        /// </summary>
        private void Awake()
        {
            // 实现单例模式
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSystem();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Unity Start方法
        /// </summary>
        private void Start()
        {
            if (_autoStartDemo && _isInitialized)
            {
                StartDemo();
            }
        }
        
        /// <summary>
        /// Unity Update方法
        /// </summary>
        private void Update()
        {
            if (!_isInitialized) return;
            
            // 更新系统状态
            UpdateSystemState();
            
            // 处理全局输入
            HandleGlobalInput();
        }
        
        /// <summary>
        /// Unity OnDestroy方法
        /// </summary>
        private void OnDestroy()
        {
            if (_instance == this)
            {
                ShutdownSystem();
                _instance = null;
            }
        }
        
        #endregion
        
        #region 系统管理方法
        
        /// <summary>
        /// 初始化系统
        /// </summary>
        private void InitializeSystem()
        {
            if (_isInitialized) return;
            
            _currentState = SystemState.Initializing;
            
            try
            {
                Debug.Log("[CombatSystemManager] 开始初始化战斗系统...");
                
                // 创建核心战斗管理器
                InitializeCombatManager();
                
                // 初始化演示系统
                if (_autoStartDemo)
                {
                    InitializeDemoSystem();
                }
                
                _isInitialized = true;
                _currentState = SystemState.Ready;
                
                Debug.Log("[CombatSystemManager] 战斗系统初始化完成");
            }
            catch (System.Exception ex)
            {
                _currentState = SystemState.Error;
                Debug.LogError($"[CombatSystemManager] 系统初始化失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 初始化核心战斗管理器
        /// </summary>
        private void InitializeCombatManager()
        {
            if (_combatManager == null)
            {
                GameObject combatManagerObj = new GameObject("CoreCombatManager");
                combatManagerObj.transform.SetParent(transform);
                _combatManager = combatManagerObj.AddComponent<CombatManager>();
                _combatManager.Initialize();
                
                Debug.Log("[CombatSystemManager] 核心战斗管理器初始化完成");
            }
        }
        
        /// <summary>
        /// 初始化演示系统
        /// </summary>
        private void InitializeDemoSystem()
        {
            if (_combatDemo == null)
            {
                GameObject demoObj;
                
                if (_demoPrefab != null)
                {
                    demoObj = Instantiate(_demoPrefab, transform);
                }
                else
                {
                    demoObj = new GameObject("CombatSystemDemo");
                    demoObj.transform.SetParent(transform);
                    demoObj.AddComponent<CombatSystemDemo>();
                }
                
                _combatDemo = demoObj.GetComponent<CombatSystemDemo>();
                
                if (_combatDemo == null)
                {
                    Debug.LogWarning("[CombatSystemManager] 演示系统组件未找到");
                }
                else
                {
                    Debug.Log("[CombatSystemManager] 演示系统初始化完成");
                }
            }
        }
        
        /// <summary>
        /// 关闭系统
        /// </summary>
        private void ShutdownSystem()
        {
            Debug.Log("[CombatSystemManager] 关闭战斗系统...");
            
            // 停止演示
            if (_combatDemo != null)
            {
                _combatDemo.ResetDemo();
            }
            
            // 清理战斗管理器
            if (_combatManager != null)
            {
                _combatManager.EndCombat();
                _combatManager.ClearAllCharacters();
            }
            
            _isInitialized = false;
            _currentState = SystemState.Uninitialized;
            
            Debug.Log("[CombatSystemManager] 战斗系统已关闭");
        }
        
        #endregion
        
        #region 公共接口方法
        
        /// <summary>
        /// 启动演示
        /// </summary>
        public void StartDemo()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[CombatSystemManager] 系统未初始化，无法启动演示");
                return;
            }
            
            if (_combatDemo == null)
            {
                InitializeDemoSystem();
            }
            
            if (_combatDemo != null)
            {
                _combatDemo.StartDemo();
                _currentState = SystemState.Running;
                Debug.Log("[CombatSystemManager] 演示已启动");
            }
            else
            {
                Debug.LogError("[CombatSystemManager] 演示系统初始化失败");
            }
        }
        
        /// <summary>
        /// 停止演示
        /// </summary>
        public void StopDemo()
        {
            if (_combatDemo != null)
            {
                _combatDemo.ResetDemo();
                _currentState = SystemState.Ready;
                Debug.Log("[CombatSystemManager] 演示已停止");
            }
        }
        
        /// <summary>
        /// 暂停/继续演示
        /// </summary>
        public void TogglePauseDemo()
        {
            if (_combatDemo != null)
            {
                _combatDemo.TogglePause();
                
                if (_combatDemo.CurrentState == CombatSystemDemo.DemoState.Paused)
                {
                    _currentState = SystemState.Paused;
                }
                else if (_combatDemo.CurrentState == CombatSystemDemo.DemoState.Running)
                {
                    _currentState = SystemState.Running;
                }
            }
        }
        
        /// <summary>
        /// 重新初始化系统
        /// </summary>
        public void ReinitializeSystem()
        {
            Debug.Log("[CombatSystemManager] 重新初始化系统...");
            
            ShutdownSystem();
            InitializeSystem();
            
            if (_autoStartDemo)
            {
                StartDemo();
            }
        }
        
        /// <summary>
        /// 获取系统状态信息
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public string GetSystemStatusInfo()
        {
            string info = $"=== 战斗系统状态 ===\n";
            info += $"系统状态: {_currentState}\n";
            info += $"是否初始化: {_isInitialized}\n";
            info += $"调试模式: {_enableDebugMode}\n";
            info += $"自动启动演示: {_autoStartDemo}\n";
            
            if (_combatManager != null)
            {
                info += $"\n=== 战斗管理器状态 ===\n";
                info += $"是否在战斗中: {_combatManager.IsInCombat}\n";
                info += $"总角色数: {_combatManager.GetAllCharacterCount()}\n";
                info += $"玩家数: {_combatManager.GetPlayerCount()}\n";
                info += $"敌人数: {_combatManager.GetAliveEnemyCount()}\n";
                info += $"怪物数: {_combatManager.GetAliveMonsterCount()}\n";
            }
            
            if (_combatDemo != null)
            {
                info += $"\n=== 演示系统状态 ===\n";
                info += $"演示状态: {_combatDemo.CurrentState}\n";
                info += _combatDemo.GetDemoStatistics();
            }
            
            return info;
        }
        
        /// <summary>
        /// 获取面向对象设计原则说明
        /// </summary>
        /// <returns>设计原则说明</returns>
        public string GetOOPPrinciplesInfo()
        {
            if (_combatDemo != null)
            {
                return _combatDemo.GetOOPDemonstration();
            }
            
            return "演示系统未初始化，无法获取设计原则说明";
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 更新系统状态
        /// </summary>
        private void UpdateSystemState()
        {
            // 根据子系统状态更新总体状态
            if (_combatDemo != null)
            {
                switch (_combatDemo.CurrentState)
                {
                    case CombatSystemDemo.DemoState.Running:
                        if (_currentState != SystemState.Running)
                            _currentState = SystemState.Running;
                        break;
                        
                    case CombatSystemDemo.DemoState.Paused:
                        if (_currentState != SystemState.Paused)
                            _currentState = SystemState.Paused;
                        break;
                        
                    case CombatSystemDemo.DemoState.NotStarted:
                    case CombatSystemDemo.DemoState.Completed:
                        if (_currentState == SystemState.Running || _currentState == SystemState.Paused)
                            _currentState = SystemState.Ready;
                        break;
                }
            }
        }
        
        /// <summary>
        /// 处理全局输入
        /// </summary>
        private void HandleGlobalInput()
        {
            // F10 - 显示系统状态
            if (Input.GetKeyDown(KeyCode.F10))
            {
                Debug.Log(GetSystemStatusInfo());
            }
            
            // F11 - 显示设计原则说明
            if (Input.GetKeyDown(KeyCode.F11))
            {
                Debug.Log(GetOOPPrinciplesInfo());
            }
            
            // F12 - 重新初始化系统
            if (Input.GetKeyDown(KeyCode.F12))
            {
                ReinitializeSystem();
            }
        }
        
        #endregion
        
        #region 调试方法
        
        /// <summary>
        /// Unity OnGUI方法（用于显示系统信息）
        /// </summary>
        private void OnGUI()
        {
            if (!_enableDebugMode) return;
            
            // 显示系统状态
            GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 290, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== 战斗系统管理器 ===", GUI.skin.label);
            GUILayout.Label($"系统状态: {_currentState}");
            GUILayout.Label($"初始化: {(_isInitialized ? "是" : "否")}");
            
            if (_combatManager != null)
            {
                GUILayout.Label($"战斗中: {(_combatManager.IsInCombat ? "是" : "否")}");
            }
            
            if (_combatDemo != null)
            {
                GUILayout.Label($"演示状态: {_combatDemo.CurrentState}");
            }
            
            GUILayout.Space(10);
            GUILayout.Label("=== 全局控制 ===");
            GUILayout.Label("F10: 显示系统状态");
            GUILayout.Label("F11: 显示设计原则");
            GUILayout.Label("F12: 重新初始化");
            
            if (GUILayout.Button("启动演示"))
            {
                StartDemo();
            }
            
            if (GUILayout.Button("停止演示"))
            {
                StopDemo();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        #endregion
    }
}