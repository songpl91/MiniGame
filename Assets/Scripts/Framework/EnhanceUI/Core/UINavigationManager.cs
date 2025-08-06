using System;
using System.Collections.Generic;
using UnityEngine;
using Framework.EnhanceUI.Config;

namespace Framework.EnhanceUI.Core
{
    /// <summary>
    /// UI导航管理器
    /// 负责管理UI的导航历史和回退操作，支持多种打开策略的统一管理
    /// </summary>
    public class UINavigationManager : MonoBehaviour
    {
        #region 导航栈数据结构
        
        /// <summary>
        /// 导航栈项
        /// </summary>
        [Serializable]
        public class NavigationStackItem
        {
            /// <summary>
            /// UI名称
            /// </summary>
            public string uiName;
            
            /// <summary>
            /// 实例ID
            /// </summary>
            public string instanceId;
            
            /// <summary>
            /// 打开策略
            /// </summary>
            public UIOpenStrategy strategy;
            
            /// <summary>
            /// 传递的数据
            /// </summary>
            public object data;
            
            /// <summary>
            /// 打开时间
            /// </summary>
            public float timestamp;
            
            /// <summary>
            /// 是否可以回退
            /// </summary>
            public bool canGoBack;
            
            /// <summary>
            /// 导航上下文
            /// </summary>
            public NavigationContext context;
            
            /// <summary>
            /// 优先级（用于回退排序）
            /// </summary>
            public int priority;
        }
        
        /// <summary>
        /// 导航上下文
        /// </summary>
        [Serializable]
        public class NavigationContext
        {
            /// <summary>
            /// 来源UI
            /// </summary>
            public string fromUI;
            
            /// <summary>
            /// 打开原因
            /// </summary>
            public string reason;
            
            /// <summary>
            /// 额外参数
            /// </summary>
            public Dictionary<string, object> parameters;
            
            /// <summary>
            /// 是否自动返回来源UI
            /// </summary>
            public bool autoReturnToSource;
            
            public NavigationContext()
            {
                parameters = new Dictionary<string, object>();
                autoReturnToSource = false;
            }
        }
        
        #endregion
        
        #region 字段和属性
        
        [Header("导航配置")]
        [SerializeField] private int maxStackSize = 50;
        [SerializeField] private bool enableAutoCleanup = true;
        [SerializeField] private float cleanupInterval = 30f;
        [SerializeField] private bool enableDebugLog = false;
        
        [Header("回退优先级配置")]
        [SerializeField] private int stackModePriority = 100;      // Stack策略优先级最高
        [SerializeField] private int singleModePriority = 80;      // Single策略次之
        [SerializeField] private int limitedModePriority = 60;     // Limited策略再次
        [SerializeField] private int multipleModePriority = 40;    // Multiple策略较低
        [SerializeField] private int queueModePriority = 20;       // Queue策略最低
        
        // 主导航栈（支持不同策略的UI混合管理）
        private List<NavigationStackItem> navigationStack = new List<NavigationStackItem>();
        
        // 策略特定的栈管理（用于优化回退性能）
        private Stack<NavigationStackItem> stackModeItems = new Stack<NavigationStackItem>();
        private Dictionary<string, NavigationStackItem> singleModeItems = new Dictionary<string, NavigationStackItem>();
        private List<NavigationStackItem> limitedModeItems = new List<NavigationStackItem>();
        
        // UI管理器引用
        private EnhanceUIManager uiManager;
        private UIInstanceManager instanceManager;
        
        // 清理计时器
        private float lastCleanupTime;
        
        /// <summary>
        /// 当前导航栈大小
        /// </summary>
        public int StackSize => navigationStack.Count;
        
        /// <summary>
        /// 是否有可回退的UI
        /// </summary>
        public bool CanNavigateBack 
        { 
            get 
            { 
                var backableItems = GetBackableItems();
                foreach (var item in backableItems)
                {
                    return true; // 如果有任何一个可回退的项，返回true
                }
                return false;
            } 
        }
        
        #endregion
        
        #region 事件委托
        
        /// <summary>
        /// UI回退事件
        /// </summary>
        public event Action<string, NavigationContext> OnUINavigatedBack;
        
        /// <summary>
        /// 导航栈变化事件
        /// </summary>
        public event Action<List<NavigationStackItem>> OnNavigationStackChanged;
        
        /// <summary>
        /// 回退失败事件
        /// </summary>
        public event Action<string> OnNavigateBackFailed;
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化导航管理器
        /// </summary>
        /// <param name="uiMgr">UI管理器</param>
        /// <param name="instanceMgr">实例管理器</param>
        public void Initialize(EnhanceUIManager uiMgr, UIInstanceManager instanceMgr)
        {
            uiManager = uiMgr ?? throw new ArgumentNullException(nameof(uiMgr));
            instanceManager = instanceMgr ?? throw new ArgumentNullException(nameof(instanceMgr));
            
            lastCleanupTime = Time.time;
            
            LogDebug("UI导航管理器初始化完成");
        }
        
        #endregion
        
        #region 导航记录方法
        
        /// <summary>
        /// 记录UI打开操作
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="instanceId">实例ID</param>
        /// <param name="strategy">打开策略</param>
        /// <param name="data">数据</param>
        /// <param name="context">导航上下文</param>
        public void RecordUIOpen(string uiName, string instanceId, UIOpenStrategy strategy, 
                               object data, NavigationContext context = null)
        {
            if (string.IsNullOrEmpty(uiName) || string.IsNullOrEmpty(instanceId))
            {
                Debug.LogError("[UI导航] 记录UI打开失败：UI名称或实例ID为空");
                return;
            }
            
            var stackItem = new NavigationStackItem
            {
                uiName = uiName,
                instanceId = instanceId,
                strategy = strategy,
                data = data,
                timestamp = Time.time,
                canGoBack = CanGoBack(strategy),
                context = context ?? new NavigationContext(),
                priority = GetStrategyPriority(strategy)
            };
            
            // 根据策略处理导航记录
            HandleStrategySpecificRecord(stackItem);
            
            // 添加到主导航栈
            AddToNavigationStack(stackItem);
            
            // 触发栈变化事件
            OnNavigationStackChanged?.Invoke(navigationStack);
            
            LogDebug($"记录UI打开：{uiName} (策略: {strategy}, 实例: {instanceId})");
        }
        
        /// <summary>
        /// 记录UI关闭操作
        /// </summary>
        /// <param name="instanceId">实例ID</param>
        public void RecordUIClose(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return;
            
            // 从主导航栈移除
            NavigationStackItem removedItem = null;
            for (int i = 0; i < navigationStack.Count; i++)
            {
                if (navigationStack[i].instanceId == instanceId)
                {
                    removedItem = navigationStack[i];
                    break;
                }
            }
            
            if (removedItem != null)
            {
                navigationStack.Remove(removedItem);
                
                // 从策略特定的集合中移除
                RemoveFromStrategySpecificCollections(removedItem);
                
                LogDebug($"记录UI关闭：{removedItem.uiName} (实例: {instanceId})");
                
                // 触发栈变化事件
                OnNavigationStackChanged?.Invoke(navigationStack);
            }
        }
        
        #endregion
        
        #region 回退操作方法
        
        /// <summary>
        /// 执行回退操作
        /// </summary>
        /// <returns>是否成功回退</returns>
        public bool NavigateBack()
        {
            var backableItems = GetBackableItems();
            
            // 检查是否有可回退的项
            bool hasBackableItems = false;
            foreach (var item in backableItems)
            {
                hasBackableItems = true;
                break;
            }
            
            if (!hasBackableItems)
            {
                LogDebug("没有可回退的UI");
                OnNavigateBackFailed?.Invoke("没有可回退的UI");
                return false;
            }
            
            // 按优先级排序，优先回退Stack策略的UI
            NavigationStackItem targetItem = null;
            int highestPriority = int.MinValue;
            float latestTimestamp = float.MinValue;
            
            foreach (var item in backableItems)
            {
                if (item.priority > highestPriority || 
                    (item.priority == highestPriority && item.timestamp > latestTimestamp))
                {
                    targetItem = item;
                    highestPriority = item.priority;
                    latestTimestamp = item.timestamp;
                }
            }
            
            return ExecuteNavigateBack(targetItem);
        }
        
        /// <summary>
        /// 回退到指定UI
        /// </summary>
        /// <param name="targetUIName">目标UI名称</param>
        /// <returns>是否成功回退</returns>
        public bool NavigateBackTo(string targetUIName)
        {
            if (string.IsNullOrEmpty(targetUIName))
            {
                Debug.LogError("[UI导航] 目标UI名称不能为空");
                return false;
            }
            
            // 查找目标UI
            NavigationStackItem targetItem = null;
            for (int i = 0; i < navigationStack.Count; i++)
            {
                var item = navigationStack[i];
                if (item.uiName == targetUIName && IsUIActive(item.instanceId))
                {
                    targetItem = item;
                    break;
                }
            }
            
            if (targetItem == null)
            {
                LogDebug($"未找到目标UI或UI未激活：{targetUIName}");
                OnNavigateBackFailed?.Invoke($"未找到目标UI：{targetUIName}");
                return false;
            }
            
            // 收集需要关闭的UI（在目标UI之后打开的）
            var itemsToClose = new List<NavigationStackItem>();
            for (int i = 0; i < navigationStack.Count; i++)
            {
                var item = navigationStack[i];
                if (item.timestamp > targetItem.timestamp && 
                    item.canGoBack && 
                    IsUIActive(item.instanceId))
                {
                    itemsToClose.Add(item);
                }
            }
            
            // 按时间戳降序排序（最新的先关闭）
            itemsToClose.Sort((a, b) => b.timestamp.CompareTo(a.timestamp));
            
            // 逐个关闭UI
            bool allSuccess = true;
            foreach (var item in itemsToClose)
            {
                if (!CloseUIInstance(item))
                {
                    allSuccess = false;
                    Debug.LogWarning($"[UI导航] 关闭UI失败：{item.uiName}");
                }
            }
            
            LogDebug($"回退到目标UI：{targetUIName}，关闭了 {itemsToClose.Count} 个UI");
            return allSuccess;
        }
        
        /// <summary>
        /// 回退指定数量的UI
        /// </summary>
        /// <param name="count">回退数量</param>
        /// <returns>实际回退的数量</returns>
        public int NavigateBack(int count)
        {
            if (count <= 0)
                return 0;
            
            int actualCount = 0;
            
            for (int i = 0; i < count; i++)
            {
                if (NavigateBack())
                {
                    actualCount++;
                }
                else
                {
                    break;
                }
            }
            
            LogDebug($"批量回退：请求 {count} 个，实际回退 {actualCount} 个");
            return actualCount;
        }
        
        /// <summary>
        /// 清空所有可回退的UI
        /// </summary>
        /// <returns>清空的UI数量</returns>
        public int ClearAllBackableUI()
        {
            var backableItems = new List<NavigationStackItem>();
            var backableItemsEnumerable = GetBackableItems();
            foreach (var item in backableItemsEnumerable)
            {
                backableItems.Add(item);
            }
            
            int clearedCount = 0;
            
            foreach (var item in backableItems)
            {
                if (CloseUIInstance(item))
                {
                    clearedCount++;
                }
            }
            
            LogDebug($"清空所有可回退UI：共清空 {clearedCount} 个");
            return clearedCount;
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 获取当前最顶层的UI
        /// </summary>
        /// <returns>最顶层的UI名称</returns>
        public string GetTopUI()
        {
            NavigationStackItem topItem = null;
            float latestTimestamp = float.MinValue;
            
            for (int i = 0; i < navigationStack.Count; i++)
            {
                var item = navigationStack[i];
                if (IsUIActive(item.instanceId) && item.timestamp > latestTimestamp)
                {
                    topItem = item;
                    latestTimestamp = item.timestamp;
                }
            }
            
            return topItem?.uiName;
        }
        
        /// <summary>
        /// 获取指定策略的活跃UI列表
        /// </summary>
        /// <param name="strategy">打开策略</param>
        /// <returns>UI名称列表</returns>
        public List<string> GetActiveUIsByStrategy(UIOpenStrategy strategy)
        {
            var result = new List<string>();
            
            for (int i = 0; i < navigationStack.Count; i++)
            {
                var item = navigationStack[i];
                if (item.strategy == strategy && IsUIActive(item.instanceId))
                {
                    result.Add(item.uiName);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取导航历史信息
        /// </summary>
        /// <returns>导航历史字符串</returns>
        public string GetNavigationHistory()
        {
            var activeItems = new List<NavigationStackItem>();
            
            // 收集活跃的UI项
            for (int i = 0; i < navigationStack.Count; i++)
            {
                var item = navigationStack[i];
                if (IsUIActive(item.instanceId))
                {
                    activeItems.Add(item);
                }
            }
            
            // 按时间戳排序
            activeItems.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
            
            // 构建历史字符串
            var historyParts = new List<string>();
            for (int i = 0; i < activeItems.Count; i++)
            {
                var item = activeItems[i];
                historyParts.Add($"{item.uiName}({item.strategy})");
            }
            
            var history = string.Join(" -> ", historyParts);
            
            return $"导航历史 ({activeItems.Count}): {history}";
        }
        
        #endregion
        
        #region 私有辅助方法
        
        /// <summary>
        /// 处理策略特定的记录
        /// </summary>
        /// <param name="stackItem">栈项</param>
        private void HandleStrategySpecificRecord(NavigationStackItem stackItem)
        {
            switch (stackItem.strategy)
            {
                case UIOpenStrategy.Single:
                    // 单例模式：移除同名UI的历史记录
                    RemoveUIFromStack(stackItem.uiName);
                    singleModeItems[stackItem.uiName] = stackItem;
                    break;
                    
                case UIOpenStrategy.Stack:
                    // 栈模式：添加到栈管理
                    stackModeItems.Push(stackItem);
                    break;
                    
                case UIOpenStrategy.Limited:
                    // 限制模式：添加到限制管理
                    limitedModeItems.Add(stackItem);
                    break;
                    
                case UIOpenStrategy.Multiple:
                case UIOpenStrategy.Queue:
                    // 多开和队列模式：只记录在主栈中
                    break;
            }
        }
        
        /// <summary>
        /// 从策略特定集合中移除
        /// </summary>
        /// <param name="item">要移除的项</param>
        private void RemoveFromStrategySpecificCollections(NavigationStackItem item)
        {
            switch (item.strategy)
            {
                case UIOpenStrategy.Single:
                    singleModeItems.Remove(item.uiName);
                    break;
                    
                case UIOpenStrategy.Stack:
                    // Stack使用的是栈结构，需要特殊处理
                    var tempStack = new Stack<NavigationStackItem>();
                    while (stackModeItems.Count > 0)
                    {
                        var stackItem = stackModeItems.Pop();
                        if (stackItem.instanceId != item.instanceId)
                        {
                            tempStack.Push(stackItem);
                        }
                    }
                    while (tempStack.Count > 0)
                    {
                        stackModeItems.Push(tempStack.Pop());
                    }
                    break;
                    
                case UIOpenStrategy.Limited:
                    limitedModeItems.Remove(item);
                    break;
            }
        }
        
        /// <summary>
        /// 获取可回退的UI项
        /// </summary>
        /// <returns>可回退的UI项列表</returns>
        private IEnumerable<NavigationStackItem> GetBackableItems()
        {
            var result = new List<NavigationStackItem>();
            
            for (int i = 0; i < navigationStack.Count; i++)
            {
                var item = navigationStack[i];
                if (item.canGoBack && IsUIActive(item.instanceId))
                {
                    result.Add(item);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 执行回退操作
        /// </summary>
        /// <param name="item">要回退的项</param>
        /// <returns>是否成功</returns>
        private bool ExecuteNavigateBack(NavigationStackItem item)
        {
            bool success = CloseUIInstance(item);
            
            if (success)
            {
                // 触发回退事件
                OnUINavigatedBack?.Invoke(item.uiName, item.context);
                
                // 如果设置了自动返回来源UI
                if (item.context.autoReturnToSource && !string.IsNullOrEmpty(item.context.fromUI))
                {
                    // 延迟一帧执行，避免在UI关闭过程中立即打开新UI
                    StartCoroutine(DelayedOpenSourceUI(item.context.fromUI));
                }
                
                LogDebug($"回退成功：{item.uiName}");
            }
            else
            {
                OnNavigateBackFailed?.Invoke($"关闭UI失败：{item.uiName}");
                LogDebug($"回退失败：{item.uiName}");
            }
            
            return success;
        }
        
        /// <summary>
        /// 延迟打开来源UI
        /// </summary>
        /// <param name="sourceUIName">来源UI名称</param>
        /// <returns>协程</returns>
        private System.Collections.IEnumerator DelayedOpenSourceUI(string sourceUIName)
        {
            yield return null; // 等待一帧
            
            if (uiManager != null)
            {
                uiManager.OpenUI(sourceUIName);
                LogDebug($"自动返回来源UI：{sourceUIName}");
            }
        }
        
        /// <summary>
        /// 关闭UI实例
        /// </summary>
        /// <param name="item">栈项</param>
        /// <returns>是否成功</returns>
        private bool CloseUIInstance(NavigationStackItem item)
        {
            if (uiManager == null)
                return false;
            
            return uiManager.CloseUIByInstanceId(item.instanceId);
        }
        
        /// <summary>
        /// 判断策略是否支持回退
        /// </summary>
        /// <param name="strategy">打开策略</param>
        /// <returns>是否支持回退</returns>
        private bool CanGoBack(UIOpenStrategy strategy)
        {
            return strategy switch
            {
                UIOpenStrategy.Single => true,
                UIOpenStrategy.Stack => true,
                UIOpenStrategy.Limited => true,
                UIOpenStrategy.Multiple => false,  // 多开模式通常不支持回退
                UIOpenStrategy.Queue => false,     // 队列模式不支持回退
                _ => false
            };
        }
        
        /// <summary>
        /// 获取策略的优先级
        /// </summary>
        /// <param name="strategy">打开策略</param>
        /// <returns>优先级值</returns>
        private int GetStrategyPriority(UIOpenStrategy strategy)
        {
            return strategy switch
            {
                UIOpenStrategy.Stack => stackModePriority,
                UIOpenStrategy.Single => singleModePriority,
                UIOpenStrategy.Limited => limitedModePriority,
                UIOpenStrategy.Multiple => multipleModePriority,
                UIOpenStrategy.Queue => queueModePriority,
                _ => 0
            };
        }
        
        /// <summary>
        /// 添加到导航栈
        /// </summary>
        /// <param name="item">栈项</param>
        private void AddToNavigationStack(NavigationStackItem item)
        {
            navigationStack.Add(item);
            
            // 限制栈大小
            if (navigationStack.Count > maxStackSize)
            {
                // 查找最旧的项
                NavigationStackItem oldestItem = null;
                float earliestTimestamp = float.MaxValue;
                
                for (int i = 0; i < navigationStack.Count; i++)
                {
                    var stackItem = navigationStack[i];
                    if (stackItem.timestamp < earliestTimestamp)
                    {
                        oldestItem = stackItem;
                        earliestTimestamp = stackItem.timestamp;
                    }
                }
                
                if (oldestItem != null)
                {
                    navigationStack.Remove(oldestItem);
                    RemoveFromStrategySpecificCollections(oldestItem);
                    LogDebug($"导航栈已满，移除最旧的项：{oldestItem.uiName}");
                }
            }
        }
        
        /// <summary>
        /// 从栈中移除指定UI
        /// </summary>
        /// <param name="uiName">UI名称</param>
        private void RemoveUIFromStack(string uiName)
        {
            var itemsToRemove = new List<NavigationStackItem>();
            
            // 收集需要移除的项
            for (int i = 0; i < navigationStack.Count; i++)
            {
                var item = navigationStack[i];
                if (item.uiName == uiName)
                {
                    itemsToRemove.Add(item);
                }
            }
            
            // 移除收集到的项
            foreach (var item in itemsToRemove)
            {
                navigationStack.Remove(item);
                RemoveFromStrategySpecificCollections(item);
            }
        }
        
        /// <summary>
        /// 检查UI是否活跃
        /// </summary>
        /// <param name="instanceId">实例ID</param>
        /// <returns>是否活跃</returns>
        private bool IsUIActive(string instanceId)
        {
            if (instanceManager == null)
                return false;
            
            var instance = instanceManager.GetInstance(instanceId);
            return instance != null && instance.State != UIInstanceState.Destroyed;
        }
        
        /// <summary>
        /// 清理过期的导航记录
        /// </summary>
        private void CleanupExpiredItems()
        {
            if (!enableAutoCleanup)
                return;
            
            float currentTime = Time.time;
            if (currentTime - lastCleanupTime < cleanupInterval)
                return;
            
            var expiredItems = new List<NavigationStackItem>();
            
            // 收集过期的项
            for (int i = 0; i < navigationStack.Count; i++)
            {
                var item = navigationStack[i];
                if (!IsUIActive(item.instanceId))
                {
                    expiredItems.Add(item);
                }
            }
            
            // 移除过期的项
            foreach (var item in expiredItems)
            {
                navigationStack.Remove(item);
                RemoveFromStrategySpecificCollections(item);
            }
            
            if (expiredItems.Count > 0)
            {
                LogDebug($"清理过期导航记录：{expiredItems.Count} 个");
                OnNavigationStackChanged?.Invoke(navigationStack);
            }
            
            lastCleanupTime = currentTime;
        }
        
        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        private void LogDebug(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[UI导航] {message}");
            }
        }
        
        #endregion
        
        #region Unity生命周期
        
        private void Update()
        {
            // 定期清理过期记录
            if (enableAutoCleanup)
            {
                CleanupExpiredItems();
            }
        }
        
        #endregion
    }
}