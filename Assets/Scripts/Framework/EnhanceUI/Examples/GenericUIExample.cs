using UnityEngine;
using Framework.EnhanceUI.Core;
using Framework.EnhanceUI.Config;

namespace Framework.EnhanceUI.Examples
{
    /// <summary>
    /// 泛型UI接口使用示例
    /// 演示如何使用泛型接口避免装箱操作
    /// </summary>
    public class GenericUIExample : MonoBehaviour
    {
        [Header("测试数据")]
        public int testIntValue = 100;
        public float testFloatValue = 3.14f;
        public string testStringValue = "Hello World";
        public bool testBoolValue = true;
        
        [Header("UI配置")]
        public UIConfig uiConfig;
        
        private void Start()
        {
            // 初始化UI管理器
            if (uiConfig != null)
            {
                EnhanceUIManager.Instance.SetUIConfig(uiConfig);
            }
        }
        
        private void Update()
        {
            // 键盘快捷键测试
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestGenericOpenUI();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestNonGenericOpenUI();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestStructData();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestClassData();
            }
        }
        
        /// <summary>
        /// 测试泛型接口（推荐方式，避免装箱）
        /// </summary>
        private void TestGenericOpenUI()
        {
            Debug.Log("=== 测试泛型接口（避免装箱）===");
            
            // 传递int值类型 - 使用泛型接口避免装箱
            EnhanceUIManager.Instance.OpenUI<int>("TestPanel", testIntValue);
            
            // 传递float值类型 - 使用泛型接口避免装箱
            EnhanceUIManager.Instance.OpenUI<float>("TestPanel", testFloatValue);
            
            // 传递bool值类型 - 使用泛型接口避免装箱
            EnhanceUIManager.Instance.OpenUI<bool>("TestPanel", testBoolValue);
            
            // 传递string引用类型 - 不会装箱
            EnhanceUIManager.Instance.OpenUI<string>("TestPanel", testStringValue);
            
            // 无参数打开
            EnhanceUIManager.Instance.OpenUI("TestPanel");
        }
        
        /// <summary>
        /// 测试非泛型接口（会产生装箱）
        /// </summary>
        private void TestNonGenericOpenUI()
        {
            Debug.Log("=== 测试非泛型接口（会产生装箱）===");
            
            // 传递int值类型 - 会发生装箱
            EnhanceUIManager.Instance.OpenUI("TestPanel", testIntValue);
            
            // 传递float值类型 - 会发生装箱
            EnhanceUIManager.Instance.OpenUI("TestPanel", testFloatValue);
            
            // 传递bool值类型 - 会发生装箱
            EnhanceUIManager.Instance.OpenUI("TestPanel", testBoolValue);
            
            // 传递string引用类型 - 不会装箱
            EnhanceUIManager.Instance.OpenUI("TestPanel", testStringValue);
        }
        
        /// <summary>
        /// 测试结构体数据
        /// </summary>
        private void TestStructData()
        {
            Debug.Log("=== 测试结构体数据 ===");
            
            // 创建结构体数据
            var playerData = new PlayerData
            {
                playerId = 12345,
                playerName = "TestPlayer",
                level = 50,
                experience = 75000,
                isVip = true
            };
            
            // 使用泛型接口传递结构体 - 避免装箱
            EnhanceUIManager.Instance.OpenUI<PlayerData>("PlayerInfoPanel", playerData);
            
            // 异步打开
            EnhanceUIManager.Instance.OpenUIAsync<PlayerData>("PlayerInfoPanel", playerData, (panel) =>
            {
                Debug.Log($"异步打开成功: {panel.PanelName}");
            });
        }
        
        /// <summary>
        /// 测试类数据
        /// </summary>
        private void TestClassData()
        {
            Debug.Log("=== 测试类数据 ===");
            
            // 创建类数据
            var gameSettings = new GameSettings
            {
                musicVolume = 0.8f,
                soundVolume = 0.6f,
                language = "Chinese",
                quality = GraphicsQuality.High,
                enableNotifications = true
            };
            
            // 使用泛型接口传递类数据
            EnhanceUIManager.Instance.OpenUI<GameSettings>("SettingsPanel", gameSettings);
            
            // 带选项的异步打开
            var options = new UILoadOptions
            {
                loadMode = UILoadMode.Async,
                priority = 1,
                skipAnimation = false,
                timeout = 5.0f
            };
            
            EnhanceUIManager.Instance.OpenUIAsync<GameSettings>("SettingsPanel", gameSettings, options, (panel) =>
            {
                Debug.Log($"带选项异步打开成功: {panel.PanelName}");
            });
        }
        
        #region 测试数据结构
        
        /// <summary>
        /// 玩家数据结构体
        /// </summary>
        [System.Serializable]
        public struct PlayerData
        {
            public int playerId;
            public string playerName;
            public int level;
            public long experience;
            public bool isVip;
            
            public override string ToString()
            {
                return $"Player[{playerId}]: {playerName}, Level: {level}, Exp: {experience}, VIP: {isVip}";
            }
        }
        
        /// <summary>
        /// 游戏设置类
        /// </summary>
        [System.Serializable]
        public class GameSettings
        {
            public float musicVolume;
            public float soundVolume;
            public string language;
            public GraphicsQuality quality;
            public bool enableNotifications;
            
            public override string ToString()
            {
                return $"Settings: Music: {musicVolume}, Sound: {soundVolume}, Lang: {language}, Quality: {quality}, Notifications: {enableNotifications}";
            }
        }
        
        /// <summary>
        /// 图形质量枚举
        /// </summary>
        public enum GraphicsQuality
        {
            Low,
            Medium,
            High,
            Ultra
        }
        
        #endregion
        
        #region Unity编辑器菜单
        
#if UNITY_EDITOR
        [UnityEditor.MenuItem("EnhanceUI/Test Generic Interface")]
        public static void TestGenericInterface()
        {
            var example = FindObjectOfType<GenericUIExample>();
            if (example != null)
            {
                example.TestGenericOpenUI();
            }
            else
            {
                Debug.LogWarning("未找到 GenericUIExample 组件");
            }
        }
        
        [UnityEditor.MenuItem("EnhanceUI/Test Non-Generic Interface")]
        public static void TestNonGenericInterface()
        {
            var example = FindObjectOfType<GenericUIExample>();
            if (example != null)
            {
                example.TestNonGenericOpenUI();
            }
            else
            {
                Debug.LogWarning("未找到 GenericUIExample 组件");
            }
        }
        
        [UnityEditor.MenuItem("EnhanceUI/Test Struct Data")]
        public static void TestStructData()
        {
            var example = FindObjectOfType<GenericUIExample>();
            if (example != null)
            {
                example.TestStructData();
            }
            else
            {
                Debug.LogWarning("未找到 GenericUIExample 组件");
            }
        }
        
        [UnityEditor.MenuItem("EnhanceUI/Test Class Data")]
        public static void TestClassData()
        {
            var example = FindObjectOfType<GenericUIExample>();
            if (example != null)
            {
                example.TestClassData();
            }
            else
            {
                Debug.LogWarning("未找到 GenericUIExample 组件");
            }
        }
#endif
        
        #endregion
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("泛型UI接口测试", GUI.skin.box);
            
            if (GUILayout.Button("1. 测试泛型接口（推荐）"))
            {
                TestGenericOpenUI();
            }
            
            if (GUILayout.Button("2. 测试非泛型接口（装箱）"))
            {
                TestNonGenericOpenUI();
            }
            
            if (GUILayout.Button("3. 测试结构体数据"))
            {
                TestStructData();
            }
            
            if (GUILayout.Button("4. 测试类数据"))
            {
                TestClassData();
            }
            
            GUILayout.Space(10);
            GUILayout.Label("说明：", GUI.skin.box);
            GUILayout.Label("• 泛型接口避免值类型装箱");
            GUILayout.Label("• 提高性能，减少GC压力");
            GUILayout.Label("• 支持结构体和类数据传递");
            
            GUILayout.EndArea();
        }
    }
}