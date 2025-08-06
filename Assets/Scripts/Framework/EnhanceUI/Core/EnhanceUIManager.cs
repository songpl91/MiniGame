using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.EnhanceUI.Config;

namespace Framework.EnhanceUI.Core
{
    /// <summary>
    /// 增强型UI管理器
    /// 集成了层级管理、加载队列、实例管理等功能的核心UI管理器
    /// </summary>
    public class EnhanceUIManager : MonoBehaviour, IUILoader
    {
        #region 单例模式

        private static EnhanceUIManager instance;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static EnhanceUIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<EnhanceUIManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("EnhanceUIManager");
                        instance = go.AddComponent<EnhanceUIManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return instance;
            }
        }

        #endregion

        #region 字段和属性

        [Header("UI管理器配置")] [SerializeField] private UIConfig uiConfig;
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool enableDebugLog = true;

        [Header("UI根节点配置")] [SerializeField] private Canvas uiRootCanvas;
        [SerializeField] private string uiRootName = "UIRoot";
        [SerializeField] private int canvasSortOrder = 100;

        [Header("资源加载配置")] [SerializeField] private string uiPrefabPath = "UI/Prefabs/";
        [SerializeField] private bool useAddressableAssets = false;

        // 核心管理器组件
        private UILayerManager layerManager;
        private UILoadQueue loadQueue;
        private UIInstanceManager instanceManager;
        private UINavigationManager navigationManager;
        private UINavigationInputHandler navigationInputHandler;

        // UI根节点
        private Transform uiRoot;

        // 预制体缓存
        private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

        // 初始化状态
        private bool isInitialized = false;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// UI配置
        /// </summary>
        public UIConfig UIConfig => uiConfig;

        /// <summary>
        /// 层级管理器
        /// </summary>
        public UILayerManager LayerManager => layerManager;

        /// <summary>
        /// 加载队列
        /// </summary>
        public UILoadQueue LoadQueue => loadQueue;

        /// <summary>
        /// 实例管理器
        /// </summary>
        public UIInstanceManager InstanceManager => instanceManager;

        /// <summary>
        /// 导航管理器
        /// </summary>
        public UINavigationManager NavigationManager => navigationManager;

        /// <summary>
        /// 导航输入处理器
        /// </summary>
        public UINavigationInputHandler NavigationInputHandler => navigationInputHandler;

        #endregion

        #region 事件委托

        /// <summary>
        /// UI打开事件
        /// </summary>
        public event Action<string, EnhanceUIPanel> OnUIOpened;

        /// <summary>
        /// UI关闭事件
        /// </summary>
        public event Action<string, EnhanceUIPanel> OnUIClosed;

        /// <summary>
        /// UI加载开始事件
        /// </summary>
        public event Action<string> OnUILoadStart;

        /// <summary>
        /// UI加载完成事件
        /// </summary>
        public event Action<string, bool> OnUILoadComplete;

        /// <summary>
        /// UI管理器初始化完成事件
        /// </summary>
        public event Action OnManagerInitialized;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 确保单例
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                if (autoInitialize)
                {
                    Initialize();
                }
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (!isInitialized && autoInitialize)
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化UI管理器
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                LogDebug("UI管理器已经初始化");
                return;
            }

            try
            {
                LogDebug("开始初始化UI管理器...");

                // 创建UI根节点
                CreateUIRoot();

                // 初始化核心组件
                InitializeComponents();

                // 加载UI配置
                LoadUIConfig();

                // 设置初始化完成标志
                isInitialized = true;

                // 触发初始化完成事件
                OnManagerInitialized?.Invoke();

                LogDebug("UI管理器初始化完成");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIManager] 初始化失败: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 创建UI根节点
        /// </summary>
        private void CreateUIRoot()
        {
            // 查找现有的UI根节点
            if (uiRootCanvas != null)
            {
                uiRoot = uiRootCanvas.transform;
                LogDebug($"使用现有UI根节点: {uiRoot.name}");
                return;
            }

            // 创建新的UI根节点
            GameObject rootGO = new GameObject(uiRootName);
            DontDestroyOnLoad(rootGO);

            // 添加Canvas组件
            Canvas canvas = rootGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = canvasSortOrder;

            // 添加CanvasScaler组件
            var scaler = rootGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 添加GraphicRaycaster组件
            rootGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            uiRoot = rootGO.transform;
            uiRootCanvas = canvas;

            LogDebug($"创建UI根节点: {uiRootName}");
        }

        /// <summary>
        /// 初始化核心组件
        /// </summary>
        private void InitializeComponents()
        {
            // 创建层级管理器
            GameObject layerManagerGO = new GameObject("UILayerManager");
            layerManagerGO.transform.SetParent(transform, false);
            layerManager = layerManagerGO.AddComponent<UILayerManager>();
            layerManager.Initialize(uiRoot);

            // 创建加载队列
            GameObject loadQueueGO = new GameObject("UILoadQueue");
            loadQueueGO.transform.SetParent(transform, false);
            loadQueue = loadQueueGO.AddComponent<UILoadQueue>();
            loadQueue.Initialize(this);

            // 创建实例管理器
            GameObject instanceManagerGO = new GameObject("UIInstanceManager");
            instanceManagerGO.transform.SetParent(transform, false);
            instanceManager = instanceManagerGO.AddComponent<UIInstanceManager>();
            instanceManager.Initialize(layerManager);

            // 创建导航管理器
            GameObject navigationManagerGO = new GameObject("UINavigationManager");
            navigationManagerGO.transform.SetParent(transform, false);
            navigationManager = navigationManagerGO.AddComponent<UINavigationManager>();
            navigationManager.Initialize(this, instanceManager);

            // 创建导航输入处理器
            GameObject navigationInputGO = new GameObject("UINavigationInputHandler");
            navigationInputGO.transform.SetParent(transform, false);
            navigationInputHandler = navigationInputGO.AddComponent<UINavigationInputHandler>();
            navigationInputHandler.Initialize(navigationManager, this);

            // 绑定事件
            BindEvents();

            LogDebug("核心组件初始化完成");
        }

        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BindEvents()
        {
            // 绑定加载队列事件
            if (loadQueue != null)
            {
                loadQueue.OnRequestStarted += OnLoadRequestStarted;
                loadQueue.OnRequestCompleted += OnLoadRequestCompleted;
                loadQueue.OnRequestFailed += OnLoadRequestFailed;
            }

            // 绑定实例管理器事件
            if (instanceManager != null)
            {
                instanceManager.OnInstanceShown += OnInstanceShown;
                instanceManager.OnInstanceHidden += OnInstanceHidden;
                instanceManager.OnInstanceDestroyed += OnInstanceDestroyed;
            }
        }

        /// <summary>
        /// 加载UI配置
        /// </summary>
        private void LoadUIConfig()
        {
            if (uiConfig == null)
            {
                LogDebug("未设置UI配置，将使用默认配置");
                return;
            }

            // 验证配置
            var (isValid, errors) = uiConfig.ValidateConfig();
            if (!isValid)
            {
                Debug.LogWarning("[EnhanceUIManager] UI配置验证失败，可能存在配置错误");
                foreach (var error in errors)
                {
                    Debug.LogWarning($"[EnhanceUIManager] 配置错误: {error}");
                }
            }

            LogDebug($"加载UI配置完成，共 {uiConfig.GetAllConfigs().Count} 个UI配置");
        }

        #endregion

        #region UI打开方法

        /// <summary>
        /// 打开UI（同步）
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="data">传递的数据</param>
        /// <returns>UI面板</returns>
        public EnhanceUIPanel OpenUI(string uiName, object data = null)
        {
            return OpenUIInternal(uiName, data, UILoadMode.Sync, null);
        }

        /// <summary>
        /// 打开UI（同步，泛型版本，避免装箱）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="uiName">UI名称</param>
        /// <param name="data">传递的数据</param>
        /// <returns>UI面板</returns>
        public EnhanceUIPanel OpenUI<T>(string uiName, T data)
        {
            return OpenUIInternal<T>(uiName, data, UILoadMode.Sync, null);
        }

        /// <summary>
        /// 打开UI（同步，无参数版本）
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>UI面板</returns>
        public EnhanceUIPanel OpenUI(string uiName)
        {
            return OpenUIInternal(uiName, null, UILoadMode.Sync, null);
        }

        /// <summary>
        /// 打开UI（异步）
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="onComplete">完成回调</param>
        /// <param name="data">传递的数据</param>
        public void OpenUIAsync(string uiName, Action<EnhanceUIPanel> onComplete = null, object data = null)
        {
            OpenUIInternal(uiName, data, UILoadMode.Async, onComplete);
        }

        /// <summary>
        /// 打开UI（异步，泛型版本，避免装箱）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="uiName">UI名称</param>
        /// <param name="data">传递的数据</param>
        /// <param name="onComplete">完成回调</param>
        public void OpenUIAsync<T>(string uiName, T data, Action<EnhanceUIPanel> onComplete = null)
        {
            OpenUIInternal<T>(uiName, data, UILoadMode.Async, onComplete);
        }

        /// <summary>
        /// 打开UI（异步，无参数版本）
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="onComplete">完成回调</param>
        public void OpenUIAsync(string uiName, Action<EnhanceUIPanel> onComplete = null)
        {
            OpenUIInternal(uiName, null, UILoadMode.Async, onComplete);
        }

        /// <summary>
        /// 打开UI（带选项）
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="options">加载选项</param>
        /// <param name="onComplete">完成回调</param>
        /// <param name="data">传递的数据</param>
        public void OpenUI(string uiName, UILoadOptions options, Action<EnhanceUIPanel> onComplete = null,
            object data = null)
        {
            if (options == null)
            {
                OpenUIInternal(uiName, data, UILoadMode.Sync, onComplete);
                return;
            }

            OpenUIInternal(uiName, data, options.LoadMode, onComplete, options);
        }

        /// <summary>
        /// 打开UI（带选项，泛型版本，避免装箱）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="uiName">UI名称</param>
        /// <param name="data">传递的数据</param>
        /// <param name="options">加载选项</param>
        /// <param name="onComplete">完成回调</param>
        public void OpenUI<T>(string uiName, T data, UILoadOptions options, Action<EnhanceUIPanel> onComplete = null)
        {
            if (options == null)
            {
                OpenUIInternal<T>(uiName, data, UILoadMode.Sync, onComplete);
                return;
            }

            OpenUIInternal<T>(uiName, data, options.LoadMode, onComplete, options);
        }

        /// <summary>
        /// 打开UI（带选项，无参数版本）
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="options">加载选项</param>
        /// <param name="onComplete">完成回调</param>
        public void OpenUI(string uiName, UILoadOptions options, Action<EnhanceUIPanel> onComplete = null)
        {
            if (options == null)
            {
                OpenUIInternal(uiName, null, UILoadMode.Sync, onComplete);
                return;
            }

            OpenUIInternal(uiName, null, options.LoadMode, onComplete, options);
        }

        /// <summary>
        /// 内部打开UI方法
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="data">传递的数据</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="onComplete">完成回调</param>
        /// <param name="options">加载选项</param>
        /// <returns>UI面板（同步模式）</returns>
        private EnhanceUIPanel OpenUIInternal(string uiName, object data, UILoadMode loadMode,
            Action<EnhanceUIPanel> onComplete, UILoadOptions options = null)
        {
            return OpenUIInternalOriginal(uiName, data, loadMode, onComplete, options);
        }

        /// <summary>
        /// 内部打开UI方法（泛型版本，避免装箱）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="uiName">UI名称</param>
        /// <param name="data">传递的数据</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="onComplete">完成回调</param>
        /// <param name="options">加载选项</param>
        /// <returns>UI面板（同步模式）</returns>
        private EnhanceUIPanel OpenUIInternal<T>(string uiName, T data, UILoadMode loadMode,
            Action<EnhanceUIPanel> onComplete, UILoadOptions options = null)
        {
            if (!isInitialized)
            {
                Debug.LogError("[EnhanceUIManager] UI管理器未初始化");
                onComplete?.Invoke(null);
                return null;
            }

            if (string.IsNullOrEmpty(uiName))
            {
                Debug.LogError("[EnhanceUIManager] UI名称不能为空");
                onComplete?.Invoke(null);
                return null;
            }

            try
            {
                // 获取UI配置
                UIConfigData config = GetUIConfig(uiName);
                if (config == null)
                {
                    Debug.LogError($"[EnhanceUIManager] 找不到UI配置: {uiName}");
                    onComplete?.Invoke(null);
                    return null;
                }

                // 应用加载选项
                if (options != null)
                {
                    config = ApplyLoadOptions(config, options);
                }

                LogDebug($"开始打开UI: {uiName}, 模式: {loadMode}");

                // 触发加载开始事件
                OnUILoadStart?.Invoke(uiName);

                // 同步模式直接处理
                if (loadMode == UILoadMode.Sync)
                {
                    return ProcessSyncRequest<T>(uiName, data, config, onComplete);
                }
                else
                {
                    // 异步模式：为了避免装箱，我们需要将泛型数据转换为object
                    // 这里仍然会发生装箱，但这是在异步路径中，影响相对较小
                    // 创建加载请求
                    UILoadRequest request = new UILoadRequest(uiName, loadMode, (object)data, config,
                        options?.Priority ?? 0, options?.CanCancel ?? true);

                    // 设置回调
                    request.OnSuccess = (panel) =>
                    {
                        if (panel != null)
                        {
                            // 显示面板
                            instanceManager.ShowInstance(instanceManager.GetInstance(panel.InstanceId));
                            OnUIOpened?.Invoke(uiName, panel);
                        }

                        OnUILoadComplete?.Invoke(uiName, panel != null);
                        onComplete?.Invoke(panel);
                    };

                    request.OnFailure = (error) =>
                    {
                        Debug.LogError($"[EnhanceUIManager] 打开UI失败: {uiName}, 错误: {error}");
                        OnUILoadComplete?.Invoke(uiName, false);
                        onComplete?.Invoke(null);
                    };

                    // 异步模式添加到队列
                    loadQueue.EnqueueRequest(request);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIManager] 打开UI异常: {uiName}, 错误: {e.Message}");
                OnUILoadComplete?.Invoke(uiName, false);
                onComplete?.Invoke(null);
                return null;
            }
        }

        /// <summary>
        /// 处理同步请求（泛型版本，避免装箱）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="uiName">UI名称</param>
        /// <param name="data">传递的数据</param>
        /// <param name="config">UI配置</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>UI面板</returns>
        private EnhanceUIPanel ProcessSyncRequest<T>(string uiName, T data, UIConfigData config,
            Action<EnhanceUIPanel> onComplete)
        {
            try
            {
                // 同步加载UI
                EnhanceUIPanel panel = LoadUISync(uiName, config);

                if (panel != null)
                {
                    // 创建实例（泛型版本）
                    UIInstance instance = instanceManager.CreateInstance<T>(panel, config, data);

                    if (instance != null)
                    {
                        // 显示实例
                        instanceManager.ShowInstance(instance);

                        // 触发成功回调
                        OnUIOpened?.Invoke(uiName, panel);
                        OnUILoadComplete?.Invoke(uiName, true);
                        onComplete?.Invoke(panel);

                        return panel;
                    }
                }

                // 失败处理
                Debug.LogError($"[EnhanceUIManager] 创建UI实例失败: {uiName}");
                OnUILoadComplete?.Invoke(uiName, false);
                onComplete?.Invoke(null);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIManager] 同步加载异常: {uiName}, 错误: {e.Message}");
                OnUILoadComplete?.Invoke(uiName, false);
                onComplete?.Invoke(null);
                return null;
            }
        }

        /// <summary>
        /// 内部打开UI方法（原始版本）
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="data">传递的数据</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="onComplete">完成回调</param>
        /// <param name="options">加载选项</param>
        /// <returns>UI面板（同步模式）</returns>
        private EnhanceUIPanel OpenUIInternalOriginal(string uiName, object data, UILoadMode loadMode,
            Action<EnhanceUIPanel> onComplete, UILoadOptions options = null)
        {
            if (!isInitialized)
            {
                Debug.LogError("[EnhanceUIManager] UI管理器未初始化");
                onComplete?.Invoke(null);
                return null;
            }

            if (string.IsNullOrEmpty(uiName))
            {
                Debug.LogError("[EnhanceUIManager] UI名称不能为空");
                onComplete?.Invoke(null);
                return null;
            }

            try
            {
                // 获取UI配置
                UIConfigData config = GetUIConfig(uiName);
                if (config == null)
                {
                    Debug.LogError($"[EnhanceUIManager] 找不到UI配置: {uiName}");
                    onComplete?.Invoke(null);
                    return null;
                }

                // 应用加载选项
                if (options != null)
                {
                    config = ApplyLoadOptions(config, options);
                }

                LogDebug($"开始打开UI: {uiName}, 模式: {loadMode}");

                // 触发加载开始事件
                OnUILoadStart?.Invoke(uiName);

                // 创建加载请求
                UILoadRequest request = new UILoadRequest(uiName, loadMode, data, config,
                    options?.Priority ?? 0, options?.CanCancel ?? true);

                // 设置回调
                request.SetCallbacks(
                    onSuccess: (panel) =>
                    {
                        if (panel != null)
                        {
                            // 显示面板
                            instanceManager.ShowInstance(instanceManager.GetInstance(panel.InstanceId));

                            // 记录导航
                            if (navigationManager != null)
                            {
                                var context = new UINavigationManager.NavigationContext();
                                navigationManager.RecordUIOpen(uiName, panel.InstanceId,
                                    config.openStrategy, data, context);
                            }

                            OnUIOpened?.Invoke(uiName, panel);
                        }

                        OnUILoadComplete?.Invoke(uiName, panel != null);
                        onComplete?.Invoke(panel);
                    },
                    onFailure: (error) =>
                    {
                        Debug.LogError($"[EnhanceUIManager] 打开UI失败: {uiName}, 错误: {error}");
                        OnUILoadComplete?.Invoke(uiName, false);
                        onComplete?.Invoke(null);
                    }
                );

                // 同步模式直接处理
                if (loadMode == UILoadMode.Sync)
                {
                    return ProcessSyncRequest(request);
                }
                else
                {
                    // 异步模式添加到队列
                    loadQueue.EnqueueRequest(request);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIManager] 打开UI异常: {uiName}, 错误: {e.Message}");
                OnUILoadComplete?.Invoke(uiName, false);
                onComplete?.Invoke(null);
                return null;
            }
        }

        /// <summary>
        /// 处理同步请求
        /// </summary>
        /// <param name="request">加载请求</param>
        /// <returns>UI面板</returns>
        private EnhanceUIPanel ProcessSyncRequest(UILoadRequest request)
        {
            try
            {
                // 同步加载UI
                EnhanceUIPanel panel = LoadUISync(request.UIName, request.Config);

                if (panel != null)
                {
                    // 创建实例
                    UIInstance instance = instanceManager.CreateInstance(panel, request.Config, request.Data);

                    if (instance != null)
                    {
                        // 显示实例
                        instanceManager.ShowInstance(instance);

                        // 记录导航
                        if (navigationManager != null)
                        {
                            var context = new UINavigationManager.NavigationContext();
                            navigationManager.RecordUIOpen(request.UIName, instance.InstanceId,
                                request.Config.openStrategy, request.Data, context);
                        }

                        // 触发成功回调
                        request.InvokeSuccess(panel);

                        return panel;
                    }
                }

                // 失败处理
                request.InvokeFailure("创建实例失败");
                return null;
            }
            catch (Exception e)
            {
                request.InvokeFailure($"同步加载异常: {e.Message}");
                return null;
            }
        }

        #endregion

        #region UI关闭方法

        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>是否成功关闭</returns>
        public bool CloseUI(string uiName)
        {
            if (!isInitialized || string.IsNullOrEmpty(uiName))
                return false;

            var instances = instanceManager.GetActiveInstances(uiName);
            bool success = true;

            foreach (var instance in instances)
            {
                if (!CloseUIInstance(instance))
                    success = false;
            }

            return success;
        }

        /// <summary>
        /// 关闭UI实例
        /// </summary>
        /// <param name="panel">UI面板</param>
        /// <returns>是否成功关闭</returns>
        public bool CloseUI(EnhanceUIPanel panel)
        {
            if (panel == null)
                return false;

            var instance = instanceManager.GetInstance(panel.InstanceId);
            return CloseUIInstance(instance);
        }

        /// <summary>
        /// 根据实例ID关闭UI
        /// </summary>
        /// <param name="instanceId">实例ID</param>
        /// <returns>是否成功关闭</returns>
        public bool CloseUIByInstanceId(string instanceId)
        {
            var instance = instanceManager.GetInstance(instanceId);
            return CloseUIInstance(instance);
        }

        /// <summary>
        /// 关闭UI实例
        /// </summary>
        /// <param name="instance">UI实例</param>
        /// <returns>是否成功关闭</returns>
        private bool CloseUIInstance(UIInstance instance)
        {
            if (instance == null)
                return false;

            try
            {
                LogDebug($"关闭UI: {instance.UIName} (ID: {instance.InstanceId})");

                // 记录导航关闭
                if (navigationManager != null)
                {
                    navigationManager.RecordUIClose(instance.InstanceId);
                }

                // 隐藏实例
                instanceManager.HideInstance(instance);

                // 销毁实例
                instanceManager.DestroyInstance(instance);

                // 触发关闭事件
                OnUIClosed?.Invoke(instance.UIName, instance.Panel);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIManager] 关闭UI异常: {instance.UIName}, 错误: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 关闭所有UI
        /// </summary>
        public void CloseAllUI()
        {
            if (!isInitialized)
                return;

            var allInstances = instanceManager.GetAllActiveInstances();
            foreach (var instance in allInstances)
            {
                CloseUIInstance(instance);
            }

            LogDebug("关闭所有UI");
        }

        #endregion

        #region IUILoader接口实现

        /// <summary>
        /// 同步加载UI
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="config">UI配置</param>
        /// <returns>UI面板</returns>
        public EnhanceUIPanel LoadUISync(string uiName, UIConfigData config)
        {
            try
            {
                // 从对象池获取
                EnhanceUIPanel pooledPanel = instanceManager.GetInstanceFromPool(uiName);
                if (pooledPanel != null)
                {
                    LogDebug($"从对象池获取UI: {uiName}");
                    return pooledPanel;
                }

                // 加载预制体
                GameObject prefab = LoadUIPrefab(uiName, config.prefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"[EnhanceUIManager] 加载UI预制体失败: {uiName}");
                    return null;
                }

                // 实例化
                GameObject uiGO = Instantiate(prefab);
                EnhanceUIPanel panel = uiGO.GetComponent<EnhanceUIPanel>();

                if (panel == null)
                {
                    Debug.LogError($"[EnhanceUIManager] UI预制体缺少EnhanceUIPanel组件: {uiName}");
                    DestroyImmediate(uiGO);
                    return null;
                }

                LogDebug($"同步加载UI成功: {uiName}");
                return panel;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnhanceUIManager] 同步加载UI异常: {uiName}, 错误: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 异步加载UI
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="config">UI配置</param>
        /// <param name="onSuccess">成功回调</param>
        /// <param name="onFailure">失败回调</param>
        /// <param name="onProgress">进度回调</param>
        public void LoadUIAsync(string uiName, UIConfigData config,
            Action<EnhanceUIPanel> onSuccess,
            Action<string> onFailure,
            Action<float> onProgress)
        {
            StartCoroutine(LoadUIAsyncCoroutine(uiName, config, onSuccess, onFailure, onProgress));
        }

        /// <summary>
        /// 异步加载UI协程
        /// </summary>
        private IEnumerator LoadUIAsyncCoroutine(string uiName, UIConfigData config,
            Action<EnhanceUIPanel> onSuccess,
            Action<string> onFailure,
            Action<float> onProgress)
        {
            // 使用标志变量来跟踪异常
            bool hasException = false;
            string exceptionMessage = "";

            // 执行实际的加载逻辑
            yield return StartCoroutine(LoadUIAsyncCoroutineInternal(uiName, config, onSuccess,
                (error) =>
                {
                    hasException = true;
                    exceptionMessage = error;
                }, onProgress));

            // 处理异常（如果有的话）
            if (hasException)
            {
                onFailure?.Invoke(exceptionMessage);
            }
        }

        /// <summary>
        /// 异步加载UI协程内部实现（不包含异常处理）
        /// </summary>
        private IEnumerator LoadUIAsyncCoroutineInternal(string uiName, UIConfigData config,
            Action<EnhanceUIPanel> onSuccess,
            Action<string> onError,
            Action<float> onProgress)
        {
            onProgress?.Invoke(0f);

            // 从对象池获取
            EnhanceUIPanel pooledPanel = instanceManager.GetInstanceFromPool(uiName);
            if (pooledPanel != null)
            {
                onProgress?.Invoke(1f);
                onSuccess?.Invoke(pooledPanel);
                LogDebug($"从对象池获取UI: {uiName}");
                yield break;
            }

            onProgress?.Invoke(0.3f);

            // 异步加载预制体
            yield return StartCoroutine(LoadUIPrefabAsync(uiName, config.prefabPath,
                (prefab) =>
                {
                    if (prefab != null)
                    {
                        onProgress?.Invoke(0.8f);

                        // 实例化
                        GameObject uiGO = Instantiate(prefab);
                        EnhanceUIPanel panel = uiGO.GetComponent<EnhanceUIPanel>();

                        if (panel != null)
                        {
                            onProgress?.Invoke(1f);
                            onSuccess?.Invoke(panel);
                            LogDebug($"异步加载UI成功: {uiName}");
                        }
                        else
                        {
                            DestroyImmediate(uiGO);
                            onError?.Invoke($"UI预制体缺少EnhanceUIPanel组件: {uiName}");
                        }
                    }
                    else
                    {
                        onError?.Invoke($"加载UI预制体失败: {uiName}");
                    }
                }));
        }

        #endregion

        #region 资源加载方法

        /// <summary>
        /// 加载UI预制体
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="prefabPath">预制体路径</param>
        /// <returns>预制体GameObject</returns>
        private GameObject LoadUIPrefab(string uiName, string prefabPath)
        {
            // 检查缓存
            if (prefabCache.TryGetValue(uiName, out GameObject cachedPrefab))
            {
                return cachedPrefab;
            }

            // 构建完整路径
            string fullPath = string.IsNullOrEmpty(prefabPath) ? $"{uiPrefabPath}{uiName}" : prefabPath;

            // 加载预制体
            GameObject prefab = Resources.Load<GameObject>(fullPath);

            if (prefab != null)
            {
                // 添加到缓存
                prefabCache[uiName] = prefab;
                LogDebug($"加载UI预制体: {fullPath}");
            }
            else
            {
                Debug.LogError($"[EnhanceUIManager] 找不到UI预制体: {fullPath}");
            }

            return prefab;
        }

        /// <summary>
        /// 异步加载UI预制体
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程</returns>
        private IEnumerator LoadUIPrefabAsync(string uiName, string prefabPath, Action<GameObject> onComplete)
        {
            // 检查缓存
            if (prefabCache.TryGetValue(uiName, out GameObject cachedPrefab))
            {
                onComplete?.Invoke(cachedPrefab);
                yield break;
            }

            // 构建完整路径
            string fullPath = string.IsNullOrEmpty(prefabPath) ? $"{uiPrefabPath}{uiName}" : prefabPath;

            // 异步加载预制体
            ResourceRequest request = Resources.LoadAsync<GameObject>(fullPath);
            yield return request;

            GameObject prefab = request.asset as GameObject;

            if (prefab != null)
            {
                // 添加到缓存
                prefabCache[uiName] = prefab;
                LogDebug($"异步加载UI预制体: {fullPath}");
            }
            else
            {
                Debug.LogError($"[EnhanceUIManager] 异步加载UI预制体失败: {fullPath}");
            }

            onComplete?.Invoke(prefab);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取UI配置
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>UI配置</returns>
        private UIConfigData GetUIConfig(string uiName)
        {
            if (uiConfig != null)
            {
                return uiConfig.GetUIConfig(uiName);
            }

            // 返回默认配置
            return new UIConfigData
            {
                uiName = uiName,
                prefabPath = $"{uiPrefabPath}{uiName}",
                layerType = UILayerType.Normal,
                openStrategy = UIOpenStrategy.Single,
                showAnimation = UIAnimationType.Fade,
                defaultLoadMode = UILoadMode.Sync
            };
        }

        /// <summary>
        /// 应用加载选项
        /// </summary>
        /// <param name="config">原始配置</param>
        /// <param name="options">加载选项</param>
        /// <returns>应用选项后的配置</returns>
        private UIConfigData ApplyLoadOptions(UIConfigData config, UILoadOptions options)
        {
            // 创建配置副本
            UIConfigData newConfig = new UIConfigData
            {
                uiName = config.uiName,
                prefabPath = config.prefabPath,
                layerType = options.CustomLayer ?? config.layerType,
                openStrategy = config.openStrategy,
                showAnimation = options.SkipAnimation ? UIAnimationType.None : config.showAnimation,
                defaultLoadMode = options.LoadMode,
                maxInstances = config.maxInstances,
                animationDuration = config.animationDuration,
                closeOnBackgroundClick = config.closeOnBackgroundClick,
                playSound = config.playSound
            };

            return newConfig;
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message">日志信息</param>
        private void LogDebug(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[EnhanceUIManager] {message}");
            }
        }

        #endregion

        #region 事件处理方法

        /// <summary>
        /// 加载请求开始处理
        /// </summary>
        /// <param name="request">加载请求</param>
        private void OnLoadRequestStarted(UILoadRequest request)
        {
            LogDebug($"开始处理加载请求: {request.UIName}");
        }

        /// <summary>
        /// 加载请求完成处理
        /// </summary>
        /// <param name="request">加载请求</param>
        private void OnLoadRequestCompleted(UILoadRequest request)
        {
            LogDebug($"加载请求完成: {request.UIName}");
        }

        /// <summary>
        /// 加载请求失败处理
        /// </summary>
        /// <param name="request">加载请求</param>
        /// <param name="error">错误信息</param>
        private void OnLoadRequestFailed(UILoadRequest request, string error)
        {
            Debug.LogError($"[EnhanceUIManager] 加载请求失败: {request.UIName}, 错误: {error}");
        }

        /// <summary>
        /// 实例显示处理
        /// </summary>
        /// <param name="instance">UI实例</param>
        private void OnInstanceShown(UIInstance instance)
        {
            LogDebug($"UI实例显示: {instance.UIName} (ID: {instance.InstanceId})");
        }

        /// <summary>
        /// 实例隐藏处理
        /// </summary>
        /// <param name="instance">UI实例</param>
        private void OnInstanceHidden(UIInstance instance)
        {
            LogDebug($"UI实例隐藏: {instance.UIName} (ID: {instance.InstanceId})");
        }

        /// <summary>
        /// 实例销毁处理
        /// </summary>
        /// <param name="instance">UI实例</param>
        private void OnInstanceDestroyed(UIInstance instance)
        {
            LogDebug($"UI实例销毁: {instance.UIName} (ID: {instance.InstanceId})");
        }

        #endregion

        #region 公共查询方法

        /// <summary>
        /// 检查UI是否打开
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>是否打开</returns>
        public bool IsUIOpen(string uiName)
        {
            return instanceManager.HasActiveInstance(uiName);
        }

        /// <summary>
        /// 获取UI实例
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns>UI实例列表</returns>
        public List<UIInstance> GetUIInstances(string uiName)
        {
            return instanceManager.GetInstances(uiName);
        }

        /// <summary>
        /// 获取管理器状态
        /// </summary>
        /// <returns>管理器状态</returns>
        public ManagerStatus GetManagerStatus()
        {
            return new ManagerStatus
            {
                IsInitialized = isInitialized,
                QueueStatus = loadQueue.GetQueueStatus(),
                InstanceStatus = instanceManager.GetStatus(),
                LayerCount = layerManager.GetAllLayerInfo().Count,
                PrefabCacheCount = prefabCache.Count
            };
        }

        #endregion
    }

    /// <summary>
    /// 管理器状态结构
    /// </summary>
    [Serializable]
    public struct ManagerStatus
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized;

        /// <summary>
        /// 队列状态
        /// </summary>
        public QueueStatus QueueStatus;

        /// <summary>
        /// 实例状态
        /// </summary>
        public InstanceManagerStatus InstanceStatus;

        /// <summary>
        /// 层级数量
        /// </summary>
        public int LayerCount;

        /// <summary>
        /// 预制体缓存数量
        /// </summary>
        public int PrefabCacheCount;
    }
}