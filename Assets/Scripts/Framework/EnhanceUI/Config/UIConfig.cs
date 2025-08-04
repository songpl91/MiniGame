using UnityEngine;
using System;
using System.Collections.Generic;
using Framework.EnhanceUI.Core;

namespace Framework.EnhanceUI.Config
{
    /// <summary>
    /// UI配置数据
    /// 定义单个UI面板的配置信息
    /// </summary>
    [Serializable]
    public class UIConfigData
    {
        [Header("基础配置")]
        /// <summary>
        /// UI名称（唯一标识）
        /// </summary>
        public string uiName;
        
        /// <summary>
        /// UI预制体路径
        /// </summary>
        public string prefabPath;
        
        /// <summary>
        /// UI层级类型
        /// </summary>
        public UILayerType layerType = UILayerType.Normal;
        
        [Header("打开策略")]
        /// <summary>
        /// UI打开策略
        /// </summary>
        public UIOpenStrategy openStrategy = UIOpenStrategy.Single;
        
        /// <summary>
        /// 限制多开时的最大实例数（仅在Limited策略下有效）
        /// </summary>
        public int maxInstances = 1;
        
        /// <summary>
        /// 是否在打开时关闭同层级的其他UI
        /// </summary>
        public bool closeOthersInSameLayer = false;
        
        [Header("动画配置")]
        /// <summary>
        /// 显示动画类型
        /// </summary>
        public UIAnimationType showAnimation = UIAnimationType.Fade;
        
        /// <summary>
        /// 隐藏动画类型
        /// </summary>
        public UIAnimationType hideAnimation = UIAnimationType.Fade;
        
        /// <summary>
        /// 动画持续时间
        /// </summary>
        public float animationDuration = 0.3f;
        
        [Header("交互配置")]
        /// <summary>
        /// 是否可以通过点击背景关闭
        /// </summary>
        public bool closeOnBackgroundClick = false;
        
        /// <summary>
        /// 是否可以通过ESC键关闭
        /// </summary>
        public bool closeOnEscapeKey = true;
        
        /// <summary>
        /// 是否播放音效
        /// </summary>
        public bool playSound = true;
        
        [Header("加载配置")]
        /// <summary>
        /// 默认加载模式
        /// </summary>
        public UILoadMode defaultLoadMode = UILoadMode.Sync;
        
        /// <summary>
        /// 是否预加载
        /// </summary>
        public bool preload = false;
        
        /// <summary>
        /// 是否缓存实例
        /// </summary>
        public bool cacheInstance = true;
        
        [Header("高级配置")]
        /// <summary>
        /// 自定义数据（JSON格式）
        /// </summary>
        [TextArea(3, 5)]
        public string customData;
        
        /// <summary>
        /// 获取自定义数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>自定义数据对象</returns>
        public T GetCustomData<T>() where T : class
        {
            if (string.IsNullOrEmpty(customData))
                return null;
                
            try
            {
                return JsonUtility.FromJson<T>(customData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIConfig] 解析自定义数据失败: {uiName}, 错误: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 设置自定义数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">数据对象</param>
        public void SetCustomData<T>(T data) where T : class
        {
            if (data == null)
            {
                customData = string.Empty;
                return;
            }
            
            try
            {
                customData = JsonUtility.ToJson(data, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIConfig] 序列化自定义数据失败: {uiName}, 错误: {e.Message}");
                customData = string.Empty;
            }
        }
    }
    
    /// <summary>
    /// UI配置ScriptableObject
    /// 管理所有UI面板的配置信息
    /// </summary>
    [CreateAssetMenu(fileName = "UIConfig", menuName = "EnhanceUI/UI Config", order = 1)]
    public class UIConfig : ScriptableObject
    {
        [Header("全局配置")]
        /// <summary>
        /// 是否启用UI音效
        /// </summary>
        public bool enableUISound = true;
        
        /// <summary>
        /// 是否启用UI动画
        /// </summary>
        public bool enableUIAnimation = true;
        
        /// <summary>
        /// 默认动画持续时间
        /// </summary>
        public float defaultAnimationDuration = 0.3f;
        
        /// <summary>
        /// UI根节点预制体路径
        /// </summary>
        public string uiRootPrefabPath = "UI/UIRoot";
        
        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        public bool enableDebugMode = false;
        
        [Header("性能配置")]
        /// <summary>
        /// UI实例池初始大小
        /// </summary>
        public int instancePoolInitialSize = 10;
        
        /// <summary>
        /// UI实例池最大大小
        /// </summary>
        public int instancePoolMaxSize = 50;
        
        /// <summary>
        /// 异步加载超时时间（秒）
        /// </summary>
        public float asyncLoadTimeout = 30f;
        
        [Header("UI配置列表")]
        /// <summary>
        /// UI配置数据列表
        /// </summary>
        public List<UIConfigData> uiConfigs = new List<UIConfigData>();
        
        // 运行时缓存
        private Dictionary<string, UIConfigData> _configCache;
        
        /// <summary>
        /// 初始化配置缓存
        /// </summary>
        private void InitializeCache()
        {
            if (_configCache != null)
                return;
                
            _configCache = new Dictionary<string, UIConfigData>();
            
            foreach (var config in uiConfigs)
            {
                if (string.IsNullOrEmpty(config.uiName))
                {
                    Debug.LogWarning("[UIConfig] 发现空的UI名称，跳过该配置");
                    continue;
                }
                
                if (_configCache.ContainsKey(config.uiName))
                {
                    Debug.LogWarning($"[UIConfig] 发现重复的UI名称: {config.uiName}，使用最后一个配置");
                }
                
                _configCache[config.uiName] = config;
            }
            
            Debug.Log($"[UIConfig] 配置缓存初始化完成，共加载 {_configCache.Count} 个UI配置");
        }
        
        /// <summary>
        /// 获取UI配置
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>UI配置数据，如果不存在返回null</returns>
        public UIConfigData GetUIConfig(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
                return null;
                
            InitializeCache();
            
            _configCache.TryGetValue(uiName, out UIConfigData config);
            return config;
        }
        
        /// <summary>
        /// 判断UI是否存在配置
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>是否存在配置</returns>
        public bool HasUIConfig(string uiName)
        {
            return GetUIConfig(uiName) != null;
        }
        
        /// <summary>
        /// 获取所有UI配置
        /// </summary>
        /// <returns>所有UI配置的字典</returns>
        public Dictionary<string, UIConfigData> GetAllConfigs()
        {
            InitializeCache();
            return new Dictionary<string, UIConfigData>(_configCache);
        }
        
        /// <summary>
        /// 添加UI配置
        /// </summary>
        /// <param name="config">UI配置数据</param>
        public void AddUIConfig(UIConfigData config)
        {
            if (config == null || string.IsNullOrEmpty(config.uiName))
            {
                Debug.LogWarning("[UIConfig] 尝试添加无效的UI配置");
                return;
            }
            
            // 检查是否已存在
            var existingConfig = uiConfigs.Find(c => c.uiName == config.uiName);
            if (existingConfig != null)
            {
                Debug.LogWarning($"[UIConfig] UI配置已存在: {config.uiName}，将替换现有配置");
                uiConfigs.Remove(existingConfig);
            }
            
            uiConfigs.Add(config);
            
            // 清除缓存以便重新构建
            _configCache = null;
            
            Debug.Log($"[UIConfig] 添加UI配置: {config.uiName}");
        }
        
        /// <summary>
        /// 移除UI配置
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveUIConfig(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
                return false;
                
            var config = uiConfigs.Find(c => c.uiName == uiName);
            if (config == null)
                return false;
                
            uiConfigs.Remove(config);
            
            // 清除缓存以便重新构建
            _configCache = null;
            
            Debug.Log($"[UIConfig] 移除UI配置: {uiName}");
            return true;
        }
        
        /// <summary>
        /// 验证配置的有效性
        /// </summary>
        /// <returns>验证结果和错误信息</returns>
        public (bool isValid, List<string> errors) ValidateConfig()
        {
            var errors = new List<string>();
            var uiNames = new HashSet<string>();
            
            foreach (var config in uiConfigs)
            {
                // 检查UI名称
                if (string.IsNullOrEmpty(config.uiName))
                {
                    errors.Add("发现空的UI名称");
                    continue;
                }
                
                // 检查重复名称
                if (uiNames.Contains(config.uiName))
                {
                    errors.Add($"发现重复的UI名称: {config.uiName}");
                }
                else
                {
                    uiNames.Add(config.uiName);
                }
                
                // 检查预制体路径
                if (string.IsNullOrEmpty(config.prefabPath))
                {
                    errors.Add($"UI '{config.uiName}' 的预制体路径为空");
                }
                
                // 检查限制多开配置
                if (config.openStrategy == UIOpenStrategy.Limited && config.maxInstances <= 0)
                {
                    errors.Add($"UI '{config.uiName}' 使用限制多开策略但最大实例数无效");
                }
                
                // 检查动画时间
                if (config.animationDuration < 0)
                {
                    errors.Add($"UI '{config.uiName}' 的动画持续时间不能为负数");
                }
            }
            
            return (errors.Count == 0, errors);
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// 编辑器下验证配置
        /// </summary>
        private void OnValidate()
        {
            var (isValid, errors) = ValidateConfig();
            if (!isValid)
            {
                foreach (var error in errors)
                {
                    Debug.LogWarning($"[UIConfig] 配置验证失败: {error}");
                }
            }
        }
        #endif
    }
}