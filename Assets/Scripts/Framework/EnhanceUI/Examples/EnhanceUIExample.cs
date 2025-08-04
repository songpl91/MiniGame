using UnityEngine;
using Framework.EnhanceUI.Core;
using Framework.EnhanceUI.Config;

namespace Framework.EnhanceUI.Examples
{
    /// <summary>
    /// EnhanceUI框架使用示例
    /// 演示如何使用增强型UI框架的各种功能
    /// </summary>
    public class EnhanceUIExample : MonoBehaviour
    {
        #region 字段和属性
        
        [Header("示例配置")]
        [SerializeField] private UIConfig exampleUIConfig;
        [SerializeField] private bool autoRunExample = true;
        [SerializeField] private float delayBetweenOperations = 2f;
        
        #endregion
        
        #region Unity生命周期
        
        private void Start()
        {
            if (autoRunExample)
            {
                // 延迟执行示例，确保UI管理器已初始化
                Invoke(nameof(RunExample), 1f);
            }
        }
        
        private void Update()
        {
            // 键盘快捷键示例
            HandleKeyboardInput();
        }
        
        #endregion
        
        #region 示例方法
        
        /// <summary>
        /// 运行示例
        /// </summary>
        public void RunExample()
        {
            Debug.Log("=== EnhanceUI框架使用示例开始 ===");
            
            // 检查UI管理器是否已初始化
            if (!EnhanceUIManager.Instance.IsInitialized)
            {
                Debug.LogError("UI管理器未初始化，请先初始化UI管理器");
                return;
            }
            
            // 设置UI配置
            if (exampleUIConfig != null)
            {
                // 这里可以动态设置UI配置
                Debug.Log("使用自定义UI配置");
            }
            
            // 开始示例流程
            StartCoroutine(ExampleCoroutine());
        }
        
        /// <summary>
        /// 示例协程
        /// </summary>
        private System.Collections.IEnumerator ExampleCoroutine()
        {
            // 1. 同步打开主菜单
            Debug.Log("1. 同步打开主菜单");
            var mainMenu = EnhanceUIManager.Instance.OpenUI("ExampleMainMenuPanel");
            if (mainMenu != null)
            {
                Debug.Log("主菜单打开成功");
            }
            else
            {
                Debug.LogError("主菜单打开失败");
            }
            
            yield return new WaitForSeconds(delayBetweenOperations);
            
            // 2. 异步打开设置面板
            Debug.Log("2. 异步打开设置面板");
            EnhanceUIManager.Instance.OpenUIAsync("SettingsPanel", (panel) =>
            {
                if (panel != null)
                {
                    Debug.Log("设置面板打开成功");
                }
                else
                {
                    Debug.LogError("设置面板打开失败");
                }
            });
            
            yield return new WaitForSeconds(delayBetweenOperations);
            
            // 3. 测试多开策略
            Debug.Log("3. 测试多开策略 - 尝试打开多个相同面板");
            for (int i = 0; i < 3; i++)
            {
                EnhanceUIManager.Instance.OpenUIAsync("TestMultiplePanel", (panel) =>
                {
                    if (panel != null)
                    {
                        Debug.Log($"多开面板 {panel.InstanceId} 打开成功");
                    }
                });
                yield return new WaitForSeconds(0.5f);
            }
            
            yield return new WaitForSeconds(delayBetweenOperations);
            
            // 4. 测试不同层级
            Debug.Log("4. 测试不同层级的UI");
            
            // 打开底层UI
            var loadOptions1 = new UILoadOptions
            {
                LoadMode = UILoadMode.Async,
                CustomLayer = UILayerType.Bottom
            };
            EnhanceUIManager.Instance.OpenUI("BottomLayerPanel", loadOptions1);
            
            yield return new WaitForSeconds(0.5f);
            
            // 打开顶层UI
            var loadOptions2 = new UILoadOptions
            {
                LoadMode = UILoadMode.Async,
                CustomLayer = UILayerType.Top
            };
            EnhanceUIManager.Instance.OpenUI("TopLayerPanel", loadOptions2);
            
            yield return new WaitForSeconds(delayBetweenOperations);
            
            // 5. 测试加载队列
            Debug.Log("5. 测试加载队列 - 快速连续打开多个UI");
            for (int i = 0; i < 5; i++)
            {
                EnhanceUIManager.Instance.OpenUIAsync($"QueueTestPanel_{i}", (panel) =>
                {
                    if (panel != null)
                    {
                        Debug.Log($"队列测试面板 {panel.InstanceId} 打开成功");
                    }
                });
            }
            
            yield return new WaitForSeconds(delayBetweenOperations);
            
            // 6. 显示管理器状态
            Debug.Log("6. 显示管理器状态");
            ShowManagerStatus();
            
            yield return new WaitForSeconds(delayBetweenOperations);
            
            // 7. 清理所有UI
            Debug.Log("7. 清理所有UI");
            EnhanceUIManager.Instance.CloseAllUI();
            
            Debug.Log("=== EnhanceUI框架使用示例结束 ===");
        }
        
        /// <summary>
        /// 显示管理器状态
        /// </summary>
        private void ShowManagerStatus()
        {
            var status = EnhanceUIManager.Instance.GetManagerStatus();
            
            Debug.Log($"=== UI管理器状态 ===");
            Debug.Log($"已初始化: {status.IsInitialized}");
            Debug.Log($"活跃实例数: {status.InstanceStatus.ActiveInstanceCount}");
            Debug.Log($"总实例数: {status.InstanceStatus.TotalInstanceCount}");
            Debug.Log($"UI类型数: {status.InstanceStatus.UITypeCount}");
            Debug.Log($"等待队列数: {status.QueueStatus.WaitingCount}");
            Debug.Log($"加载中数: {status.QueueStatus.LoadingCount}");
            Debug.Log($"层级数: {status.LayerCount}");
            Debug.Log($"预制体缓存数: {status.PrefabCacheCount}");
            Debug.Log($"对象池数: {status.InstanceStatus.PoolCount}");
        }
        
        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void HandleKeyboardInput()
        {
            // F1 - 打开主菜单
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log("F1 - 打开主菜单");
                EnhanceUIManager.Instance.OpenUIAsync("ExampleMainMenuPanel");
            }
            
            // F2 - 打开设置面板
            if (Input.GetKeyDown(KeyCode.F2))
            {
                Debug.Log("F2 - 打开设置面板");
                EnhanceUIManager.Instance.OpenUIAsync("SettingsPanel");
            }
            
            // F3 - 显示管理器状态
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Debug.Log("F3 - 显示管理器状态");
                ShowManagerStatus();
            }
            
            // F4 - 关闭所有UI
            if (Input.GetKeyDown(KeyCode.F4))
            {
                Debug.Log("F4 - 关闭所有UI");
                EnhanceUIManager.Instance.CloseAllUI();
            }
            
            // F5 - 运行完整示例
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("F5 - 运行完整示例");
                RunExample();
            }
            
            // ESC - 关闭最顶层UI
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var activeInstances = EnhanceUIManager.Instance.InstanceManager.GetAllActiveInstances();
                if (activeInstances.Count > 0)
                {
                    // 关闭最后打开的UI
                    var lastInstance = activeInstances[activeInstances.Count - 1];
                    Debug.Log($"ESC - 关闭最顶层UI: {lastInstance.UIName}");
                    EnhanceUIManager.Instance.CloseUI(lastInstance.InstanceId);
                }
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 测试同步加载
        /// </summary>
        [ContextMenu("测试同步加载")]
        public void TestSyncLoad()
        {
            Debug.Log("测试同步加载");
            var panel = EnhanceUIManager.Instance.OpenUI("ExampleMainMenuPanel");
            if (panel != null)
            {
                Debug.Log("同步加载成功");
            }
            else
            {
                Debug.LogError("同步加载失败");
            }
        }
        
        /// <summary>
        /// 测试异步加载
        /// </summary>
        [ContextMenu("测试异步加载")]
        public void TestAsyncLoad()
        {
            Debug.Log("测试异步加载");
            EnhanceUIManager.Instance.OpenUIAsync("ExampleMainMenuPanel", (panel) =>
            {
                if (panel != null)
                {
                    Debug.Log("异步加载成功");
                }
                else
                {
                    Debug.LogError("异步加载失败");
                }
            });
        }
        
        /// <summary>
        /// 测试多开策略
        /// </summary>
        [ContextMenu("测试多开策略")]
        public void TestMultipleStrategy()
        {
            Debug.Log("测试多开策略");
            
            // 创建支持多开的加载选项
            var options = new UILoadOptions
            {
                LoadMode = UILoadMode.Async,
                Priority = 1
            };
            
            // 连续打开多个相同UI
            for (int i = 0; i < 3; i++)
            {
                EnhanceUIManager.Instance.OpenUI("TestMultiplePanel", options, (panel) =>
                {
                    if (panel != null)
                    {
                        Debug.Log($"多开面板 {panel.InstanceId} 创建成功");
                    }
                });
            }
        }
        
        /// <summary>
        /// 测试层级管理
        /// </summary>
        [ContextMenu("测试层级管理")]
        public void TestLayerManagement()
        {
            Debug.Log("测试层级管理");
            
            // 在不同层级打开UI
            var layers = new UILayerType[] 
            { 
                UILayerType.Background, 
                UILayerType.Normal, 
                UILayerType.Popup, 
                UILayerType.Top 
            };
            
            for (int i = 0; i < layers.Length; i++)
            {
                var options = new UILoadOptions
                {
                    LoadMode = UILoadMode.Async,
                    CustomLayer = layers[i]
                };
                
                EnhanceUIManager.Instance.OpenUI($"LayerTest_{layers[i]}", options, (panel) =>
                {
                    if (panel != null)
                    {
                        Debug.Log($"层级 {layers[i]} 的面板创建成功");
                    }
                });
            }
        }
        
        #endregion
    }
}