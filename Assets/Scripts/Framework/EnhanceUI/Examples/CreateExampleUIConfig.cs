using UnityEngine;
using Framework.EnhanceUI.Config;

namespace Framework.EnhanceUI.Examples
{
    /// <summary>
    /// 创建示例UI配置的工具类
    /// 用于在编辑器中快速创建示例配置
    /// </summary>
    public static class CreateExampleUIConfig
    {
        /// <summary>
        /// 创建示例UI配置
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void CreateExampleConfig()
        {
            // 这个方法在运行时会被调用，用于创建示例配置
            // 实际项目中应该通过编辑器工具或手动创建配置文件
        }
        
        /// <summary>
        /// 获取示例UI配置数据
        /// </summary>
        /// <returns>示例配置数据数组</returns>
        public static UIConfigData[] GetExampleConfigData()
        {
            return new UIConfigData[]
            {
                // 主菜单面板
                new UIConfigData
                {
                    UIName = "ExampleMainMenuPanel",
                    PrefabPath = "UI/Panels/ExampleMainMenuPanel",
                    LayerType = UILayerType.Normal,
                    OpenStrategy = UIOpenStrategy.Single,
                    AnimationType = UIAnimationType.Fade,
                    AnimationDuration = 0.3f,
                    LoadMode = UILoadMode.Sync,
                    ClickBackgroundToClose = false,
                    PlaySound = true,
                    IsModal = false
                },
                
                // 设置面板
                new UIConfigData
                {
                    UIName = "SettingsPanel",
                    PrefabPath = "UI/Panels/SettingsPanel",
                    LayerType = UILayerType.Popup,
                    OpenStrategy = UIOpenStrategy.Single,
                    AnimationType = UIAnimationType.Scale,
                    AnimationDuration = 0.25f,
                    LoadMode = UILoadMode.Async,
                    ClickBackgroundToClose = true,
                    PlaySound = true,
                    IsModal = true
                },
                
                // 确认对话框
                new UIConfigData
                {
                    UIName = "ConfirmDialog",
                    PrefabPath = "UI/Dialogs/ConfirmDialog",
                    LayerType = UILayerType.Popup,
                    OpenStrategy = UIOpenStrategy.Queue,
                    AnimationType = UIAnimationType.Scale,
                    AnimationDuration = 0.2f,
                    LoadMode = UILoadMode.Async,
                    ClickBackgroundToClose = false,
                    PlaySound = true,
                    IsModal = true
                },
                
                // 提示面板
                new UIConfigData
                {
                    UIName = "TipPanel",
                    PrefabPath = "UI/Panels/TipPanel",
                    LayerType = UILayerType.Top,
                    OpenStrategy = UIOpenStrategy.Multiple,
                    AnimationType = UIAnimationType.Slide,
                    AnimationDuration = 0.3f,
                    LoadMode = UILoadMode.Async,
                    ClickBackgroundToClose = false,
                    PlaySound = false,
                    IsModal = false
                },
                
                // 加载面板
                new UIConfigData
                {
                    UIName = "LoadingPanel",
                    PrefabPath = "UI/Panels/LoadingPanel",
                    LayerType = UILayerType.System,
                    OpenStrategy = UIOpenStrategy.Single,
                    AnimationType = UIAnimationType.Fade,
                    AnimationDuration = 0.2f,
                    LoadMode = UILoadMode.Sync,
                    ClickBackgroundToClose = false,
                    PlaySound = false,
                    IsModal = true
                },
                
                // 背景面板
                new UIConfigData
                {
                    UIName = "BackgroundPanel",
                    PrefabPath = "UI/Panels/BackgroundPanel",
                    LayerType = UILayerType.Background,
                    OpenStrategy = UIOpenStrategy.Single,
                    AnimationType = UIAnimationType.None,
                    AnimationDuration = 0f,
                    LoadMode = UILoadMode.Sync,
                    ClickBackgroundToClose = false,
                    PlaySound = false,
                    IsModal = false
                },
                
                // 测试多开面板
                new UIConfigData
                {
                    UIName = "TestMultiplePanel",
                    PrefabPath = "UI/Panels/TestMultiplePanel",
                    LayerType = UILayerType.Normal,
                    OpenStrategy = UIOpenStrategy.Multiple,
                    AnimationType = UIAnimationType.Fade,
                    AnimationDuration = 0.3f,
                    LoadMode = UILoadMode.Async,
                    ClickBackgroundToClose = true,
                    PlaySound = true,
                    IsModal = false
                },
                
                // 栈式面板
                new UIConfigData
                {
                    UIName = "StackPanel",
                    PrefabPath = "UI/Panels/StackPanel",
                    LayerType = UILayerType.Normal,
                    OpenStrategy = UIOpenStrategy.Stack,
                    AnimationType = UIAnimationType.Slide,
                    AnimationDuration = 0.3f,
                    LoadMode = UILoadMode.Async,
                    ClickBackgroundToClose = false,
                    PlaySound = true,
                    IsModal = false
                },
                
                // 限制数量面板
                new UIConfigData
                {
                    UIName = "LimitedPanel",
                    PrefabPath = "UI/Panels/LimitedPanel",
                    LayerType = UILayerType.Normal,
                    OpenStrategy = UIOpenStrategy.Limited,
                    AnimationType = UIAnimationType.Scale,
                    AnimationDuration = 0.25f,
                    LoadMode = UILoadMode.Async,
                    ClickBackgroundToClose = true,
                    PlaySound = true,
                    IsModal = false,
                    MaxInstanceCount = 3
                },
                
                // 调试面板
                new UIConfigData
                {
                    UIName = "DebugPanel",
                    PrefabPath = "UI/Panels/DebugPanel",
                    LayerType = UILayerType.Debug,
                    OpenStrategy = UIOpenStrategy.Single,
                    AnimationType = UIAnimationType.Slide,
                    AnimationDuration = 0.2f,
                    LoadMode = UILoadMode.Async,
                    ClickBackgroundToClose = false,
                    PlaySound = false,
                    IsModal = false
                }
            };
        }
        
        /// <summary>
        /// 验证配置数据
        /// </summary>
        /// <param name="configData">配置数据</param>
        /// <returns>是否有效</returns>
        public static bool ValidateConfigData(UIConfigData configData)
        {
            if (configData == null)
            {
                Debug.LogError("配置数据为空");
                return false;
            }
            
            if (string.IsNullOrEmpty(configData.UIName))
            {
                Debug.LogError("UI名称不能为空");
                return false;
            }
            
            if (string.IsNullOrEmpty(configData.PrefabPath))
            {
                Debug.LogError($"UI '{configData.UIName}' 的预制体路径不能为空");
                return false;
            }
            
            if (configData.AnimationDuration < 0)
            {
                Debug.LogWarning($"UI '{configData.UIName}' 的动画时长不能为负数，已重置为0");
                configData.AnimationDuration = 0;
            }
            
            if (configData.OpenStrategy == UIOpenStrategy.Limited && configData.MaxInstanceCount <= 0)
            {
                Debug.LogWarning($"UI '{configData.UIName}' 使用限制策略但最大实例数无效，已重置为1");
                configData.MaxInstanceCount = 1;
            }
            
            return true;
        }
        
        /// <summary>
        /// 打印配置信息
        /// </summary>
        /// <param name="configData">配置数据</param>
        public static void PrintConfigInfo(UIConfigData configData)
        {
            if (configData == null) return;
            
            Debug.Log($"=== UI配置信息: {configData.UIName} ===");
            Debug.Log($"预制体路径: {configData.PrefabPath}");
            Debug.Log($"层级类型: {configData.LayerType}");
            Debug.Log($"打开策略: {configData.OpenStrategy}");
            Debug.Log($"动画类型: {configData.AnimationType}");
            Debug.Log($"动画时长: {configData.AnimationDuration}");
            Debug.Log($"加载模式: {configData.LoadMode}");
            Debug.Log($"点击背景关闭: {configData.ClickBackgroundToClose}");
            Debug.Log($"播放音效: {configData.PlaySound}");
            Debug.Log($"模态窗口: {configData.IsModal}");
            if (configData.OpenStrategy == UIOpenStrategy.Limited)
            {
                Debug.Log($"最大实例数: {configData.MaxInstanceCount}");
            }
        }
    }
}