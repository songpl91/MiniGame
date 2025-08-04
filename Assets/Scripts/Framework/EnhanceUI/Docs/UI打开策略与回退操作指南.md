# UI打开策略与回退操作指南

## 概述

在实际项目中，确实会同时使用多种UI打开策略。每种策略都有其特定的应用场景和特点。本指南将详细说明如何在包含多种策略的项目中实现统一的回退操作管理。

**✅ 完整实现已集成**

本指南对应的完整代码实现已经集成到框架中：
- **`UINavigationManager.cs`** - 核心导航管理器，支持多策略混合管理
- **`UINavigationInputHandler.cs`** - 输入处理器，处理回退操作
- **`UINavigationExample.cs`** - 完整使用示例和测试代码
- **`EnhanceUIManager.cs`** - 已集成导航功能，自动记录UI打开/关闭

## UI打开策略详解

### 1. UIOpenStrategy.Single（单例模式）
**适用场景：**
- 设置界面（Settings）
- 背包界面（Inventory）
- 角色面板（Character）
- 商店界面（Shop）

**特点：**
- 同时只能存在一个实例
- 新打开会关闭旧的实例
- 适用于独占性强的功能界面

```csharp
// 示例：设置界面配置
var settingsConfig = new UIConfigData
{
    UIName = "SettingsPanel",
    OpenStrategy = UIOpenStrategy.Single,
    MaxInstances = 1
};
```

### 2. UIOpenStrategy.Multiple（多开模式）
**适用场景：**
- 聊天窗口（Chat）
- 物品详情弹窗（ItemDetail）
- 提示信息（Tooltip）
- 浮动文字（FloatingText）

**特点：**
- 可以同时存在多个实例
- 没有数量限制
- 适用于信息展示类UI

```csharp
// 示例：聊天窗口配置
var chatConfig = new UIConfigData
{
    UIName = "ChatWindow",
    OpenStrategy = UIOpenStrategy.Multiple,
    MaxInstances = int.MaxValue
};
```

### 3. UIOpenStrategy.Limited（限制多开）
**适用场景：**
- 邮件详情（MailDetail）- 最多3个
- 任务追踪窗口（QuestTracker）- 最多5个
- 小地图标记（MapMarker）- 最多10个
- 战斗伤害显示（DamageText）- 最多20个

**特点：**
- 可以存在多个实例但有数量限制
- 超过限制时关闭最早的实例
- 适用于需要控制资源消耗的UI

```csharp
// 示例：邮件详情配置
var mailDetailConfig = new UIConfigData
{
    UIName = "MailDetailPanel",
    OpenStrategy = UIOpenStrategy.Limited,
    MaxInstances = 3
};
```

### 4. UIOpenStrategy.Stack（栈模式）
**适用场景：**
- 对话系统（Dialog）
- 教程引导（Tutorial）
- 确认弹窗（Confirmation）
- 错误提示（ErrorDialog）

**特点：**
- 新打开的会压入栈顶
- 关闭时按栈顺序（LIFO）
- 适用于有层级关系的UI

```csharp
// 示例：对话系统配置
var dialogConfig = new UIConfigData
{
    UIName = "DialogPanel",
    OpenStrategy = UIOpenStrategy.Stack,
    MaxInstances = 10
};
```

### 5. UIOpenStrategy.Queue（队列模式）
**适用场景：**
- 系统公告（SystemNotice）
- 奖励领取（RewardClaim）
- 升级提示（LevelUpNotice）
- 成就解锁（AchievementUnlock）

**特点：**
- 如果已有实例在显示，新的请求会排队等待
- 按先进先出（FIFO）顺序显示
- 适用于需要顺序展示的UI

```csharp
// 示例：系统公告配置
var noticeConfig = new UIConfigData
{
    UIName = "SystemNoticePanel",
    OpenStrategy = UIOpenStrategy.Queue,
    MaxInstances = 1
};
```

## 实际项目中的策略组合

在真实项目中，通常会同时使用多种策略：

```csharp
// 游戏中的UI策略配置示例
public class GameUIConfig
{
    public static readonly Dictionary<string, UIConfigData> UIConfigs = new()
    {
        // 主要功能界面 - Single策略
        ["MainMenu"] = new() { OpenStrategy = UIOpenStrategy.Single },
        ["Settings"] = new() { OpenStrategy = UIOpenStrategy.Single },
        ["Inventory"] = new() { OpenStrategy = UIOpenStrategy.Single },
        ["Character"] = new() { OpenStrategy = UIOpenStrategy.Single },
        
        // 对话和引导 - Stack策略
        ["Dialog"] = new() { OpenStrategy = UIOpenStrategy.Stack, MaxInstances = 5 },
        ["Tutorial"] = new() { OpenStrategy = UIOpenStrategy.Stack, MaxInstances = 3 },
        ["Confirmation"] = new() { OpenStrategy = UIOpenStrategy.Stack, MaxInstances = 3 },
        
        // 通知系统 - Queue策略
        ["SystemNotice"] = new() { OpenStrategy = UIOpenStrategy.Queue, MaxInstances = 1 },
        ["RewardNotice"] = new() { OpenStrategy = UIOpenStrategy.Queue, MaxInstances = 1 },
        
        // 信息展示 - Limited策略
        ["ItemTooltip"] = new() { OpenStrategy = UIOpenStrategy.Limited, MaxInstances = 3 },
        ["MailDetail"] = new() { OpenStrategy = UIOpenStrategy.Limited, MaxInstances = 2 },
        
        // 效果展示 - Multiple策略
        ["DamageText"] = new() { OpenStrategy = UIOpenStrategy.Multiple },
        ["FloatingText"] = new() { OpenStrategy = UIOpenStrategy.Multiple }
    };
}
```

## 统一回退操作管理

### 1. UI导航栈设计

为了支持统一的回退操作，需要设计一个全局的UI导航栈：

```csharp
/// <summary>
/// UI导航管理器
/// 负责管理UI的导航历史和回退操作
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
        public string uiName;                    // UI名称
        public string instanceId;                // 实例ID
        public UIOpenStrategy strategy;          // 打开策略
        public object data;                      // 传递的数据
        public float timestamp;                  // 打开时间
        public bool canGoBack;                   // 是否可以回退
        public NavigationContext context;        // 导航上下文
    }
    
    /// <summary>
    /// 导航上下文
    /// </summary>
    [Serializable]
    public class NavigationContext
    {
        public string fromUI;                    // 来源UI
        public string reason;                    // 打开原因
        public Dictionary<string, object> parameters; // 额外参数
    }
    
    #endregion
    
    #region 字段和属性
    
    [Header("导航配置")]
    [SerializeField] private int maxStackSize = 50;
    [SerializeField] private bool enableAutoCleanup = true;
    [SerializeField] private float cleanupInterval = 30f;
    
    // 导航栈（支持不同策略的UI混合管理）
    private List<NavigationStackItem> navigationStack = new List<NavigationStackItem>();
    
    // 策略特定的栈管理
    private Stack<NavigationStackItem> stackModeItems = new Stack<NavigationStackItem>();
    private Queue<NavigationStackItem> queueModeItems = new Queue<NavigationStackItem>();
    
    // UI管理器引用
    private EnhanceUIManager uiManager;
    
    #endregion
    
    #region 导航操作
    
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
        var stackItem = new NavigationStackItem
        {
            uiName = uiName,
            instanceId = instanceId,
            strategy = strategy,
            data = data,
            timestamp = Time.time,
            canGoBack = CanGoBack(strategy),
            context = context ?? new NavigationContext()
        };
        
        // 根据策略处理导航记录
        switch (strategy)
        {
            case UIOpenStrategy.Single:
                // 单例模式：移除同名UI的历史记录
                RemoveUIFromStack(uiName);
                AddToNavigationStack(stackItem);
                break;
                
            case UIOpenStrategy.Stack:
                // 栈模式：直接添加到栈
                AddToNavigationStack(stackItem);
                stackModeItems.Push(stackItem);
                break;
                
            case UIOpenStrategy.Queue:
                // 队列模式：添加到队列管理
                AddToNavigationStack(stackItem);
                queueModeItems.Enqueue(stackItem);
                break;
                
            case UIOpenStrategy.Limited:
            case UIOpenStrategy.Multiple:
                // 限制/多开模式：正常添加
                AddToNavigationStack(stackItem);
                break;
        }
        
        // 清理过期记录
        if (enableAutoCleanup)
        {
            CleanupExpiredItems();
        }
    }
    
    /// <summary>
    /// 执行回退操作
    /// </summary>
    /// <returns>是否成功回退</returns>
    public bool NavigateBack()
    {
        // 优先处理栈模式的UI
        if (stackModeItems.Count > 0)
        {
            var stackItem = stackModeItems.Pop();
            return CloseUIAndNavigateBack(stackItem);
        }
        
        // 处理其他可回退的UI
        for (int i = navigationStack.Count - 1; i >= 0; i--)
        {
            var item = navigationStack[i];
            if (item.canGoBack && IsUIActive(item.instanceId))
            {
                navigationStack.RemoveAt(i);
                return CloseUIAndNavigateBack(item);
            }
        }
        
        Debug.LogWarning("[UI导航] 没有可回退的UI");
        return false;
    }
    
    /// <summary>
    /// 回退到指定UI
    /// </summary>
    /// <param name="targetUIName">目标UI名称</param>
    /// <returns>是否成功回退</returns>
    public bool NavigateBackTo(string targetUIName)
    {
        var targetItems = new List<NavigationStackItem>();
        
        // 收集需要关闭的UI
        for (int i = navigationStack.Count - 1; i >= 0; i--)
        {
            var item = navigationStack[i];
            targetItems.Add(item);
            
            if (item.uiName == targetUIName)
            {
                break;
            }
        }
        
        // 逐个关闭UI直到目标UI
        bool success = true;
        foreach (var item in targetItems)
        {
            if (item.uiName != targetUIName)
            {
                if (!uiManager.CloseUI(item.instanceId))
                {
                    success = false;
                }
                navigationStack.Remove(item);
            }
        }
        
        return success;
    }
    
    #endregion
    
    #region 辅助方法
    
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
    /// 添加到导航栈
    /// </summary>
    /// <param name="item">栈项</param>
    private void AddToNavigationStack(NavigationStackItem item)
    {
        navigationStack.Add(item);
        
        // 限制栈大小
        if (navigationStack.Count > maxStackSize)
        {
            navigationStack.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 从栈中移除指定UI
    /// </summary>
    /// <param name="uiName">UI名称</param>
    private void RemoveUIFromStack(string uiName)
    {
        navigationStack.RemoveAll(item => item.uiName == uiName);
    }
    
    /// <summary>
    /// 关闭UI并执行回退
    /// </summary>
    /// <param name="item">栈项</param>
    /// <returns>是否成功</returns>
    private bool CloseUIAndNavigateBack(NavigationStackItem item)
    {
        bool success = uiManager.CloseUI(item.instanceId);
        
        if (success)
        {
            // 触发回退事件
            OnUINavigatedBack?.Invoke(item.uiName, item.context);
            
            // 如果有来源UI，尝试重新打开
            if (!string.IsNullOrEmpty(item.context.fromUI))
            {
                // 这里可以根据需要重新打开来源UI
                // uiManager.OpenUI(item.context.fromUI);
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// 检查UI是否活跃
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <returns>是否活跃</returns>
    private bool IsUIActive(string instanceId)
    {
        // 这里需要调用UIInstanceManager的方法检查实例是否存在
        return uiManager.GetUIInstance(instanceId) != null;
    }
    
    /// <summary>
    /// 清理过期的导航记录
    /// </summary>
    private void CleanupExpiredItems()
    {
        float currentTime = Time.time;
        navigationStack.RemoveAll(item => 
            currentTime - item.timestamp > cleanupInterval && 
            !IsUIActive(item.instanceId));
    }
    
    #endregion
    
    #region 事件
    
    /// <summary>
    /// UI回退事件
    /// </summary>
    public event Action<string, NavigationContext> OnUINavigatedBack;
    
    #endregion
}
```

### 2. 回退操作的实际应用

```csharp
/// <summary>
/// 游戏输入管理器中的回退处理
/// </summary>
public class GameInputManager : MonoBehaviour
{
    [Header("回退配置")]
    public KeyCode backKey = KeyCode.Escape;
    public bool enableAndroidBackButton = true;
    
    private UINavigationManager navigationManager;
    
    private void Update()
    {
        // PC平台回退键处理
        if (Input.GetKeyDown(backKey))
        {
            HandleBackInput();
        }
        
        // Android平台返回键处理
        if (enableAndroidBackButton && Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackInput();
            }
        }
    }
    
    /// <summary>
    /// 处理回退输入
    /// </summary>
    private void HandleBackInput()
    {
        // 优先级处理：
        // 1. Stack策略的UI（对话、确认框等）
        // 2. Single策略的UI（设置、背包等）
        // 3. Limited策略的UI
        // 4. 如果没有可回退的UI，执行游戏级回退（如暂停菜单）
        
        if (!navigationManager.NavigateBack())
        {
            // 没有可回退的UI，显示暂停菜单或退出确认
            ShowPauseMenuOrExitConfirmation();
        }
    }
    
    /// <summary>
    /// 显示暂停菜单或退出确认
    /// </summary>
    private void ShowPauseMenuOrExitConfirmation()
    {
        // 根据当前游戏状态决定行为
        if (GameManager.Instance.IsInGame())
        {
            // 游戏中显示暂停菜单
            UIManager.Instance.OpenUI("PauseMenu");
        }
        else
        {
            // 主菜单中显示退出确认
            UIManager.Instance.OpenUI("ExitConfirmation");
        }
    }
}
```

## 最佳实践建议

### 1. 策略选择原则

- **Single**: 功能性强、独占性强的界面
- **Stack**: 有层级关系、需要顺序关闭的界面
- **Queue**: 需要顺序展示、不能同时显示的界面
- **Limited**: 需要控制数量、允许多开的界面
- **Multiple**: 轻量级、信息展示类的界面

### 2. 回退操作设计

- 建立清晰的UI层级关系
- 优先处理Stack策略的UI回退
- 为每个UI配置是否支持回退
- 提供回退到指定UI的功能
- 处理特殊情况（如强制关闭、异常状态）

### 3. 性能优化

- 限制导航栈的大小
- 定期清理过期的导航记录
- 避免在回退时重复创建UI实例
- 使用对象池管理频繁开关的UI

### 4. 用户体验

- 提供视觉反馈（如返回按钮状态）
- 支持手势操作（如滑动返回）
- 处理快速连续的回退操作
- 提供回退历史查看功能

## 实际使用说明

### 1. 基础设置

```csharp
// 获取UI管理器（已自动初始化导航功能）
var uiManager = EnhanceUIManager.Instance;
var navigationManager = uiManager.NavigationManager;
var inputHandler = uiManager.NavigationInputHandler;
```

### 2. 配置导航行为

```csharp
// 在UINavigationInputHandler中配置
[SerializeField] private string homeUIName = "MainMenu";
[SerializeField] private bool confirmBeforeExit = true;
[SerializeField] private string exitConfirmUIName = "ExitConfirmDialog";
```

### 3. 使用示例

```csharp
// 正常打开UI（自动记录导航）
uiManager.OpenUI("Settings");
uiManager.OpenUI("Inventory");

// 执行回退操作
navigationManager.NavigateBack();

// 回到主界面
inputHandler.BackToHome();

// 清空所有UI并回到主界面
inputHandler.ClearAllAndBackToHome();
```

### 4. 高级功能

```csharp
// 回退到指定UI
navigationManager.NavigateBackTo("MainMenu");

// 批量回退
int backCount = navigationManager.NavigateBack(3);

// 获取导航信息
string topUI = navigationManager.GetTopUI();
string history = navigationManager.GetNavigationHistory();
bool canBack = navigationManager.CanNavigateBack;
```

### 5. 事件监听

```csharp
// 监听导航事件
navigationManager.OnUINavigatedBack += (uiName, context) => {
    Debug.Log($"UI回退: {uiName}");
};

navigationManager.OnNavigationStackChanged += (stack) => {
    Debug.Log($"导航栈变化，当前大小: {stack.Count}");
};

inputHandler.OnExitApplicationRequested += () => {
    // 处理应用退出逻辑
};
```

### 6. 测试和调试

参考 `UINavigationExample.cs` 中的完整示例：
- 提供了所有策略的测试方法
- 包含GUI调试界面
- 演示复杂导航场景
- 显示实时导航状态

**快捷键测试：**
- `1` - 测试Single策略
- `2` - 测试Stack策略  
- `3` - 测试Multiple策略
- `4` - 测试Limited策略
- `5` - 测试Queue策略
- `ESC` - 执行回退操作

这套导航系统已经完全集成到框架中，可以直接在项目中使用，支持所有UI打开策略的混合管理和统一回退操作。

## 总结

在实际项目中，不同的UI确实会同时使用多种打开策略。通过合理的导航管理系统设计，可以实现统一而灵活的回退操作，提升用户体验的同时保持代码的可维护性。关键是要根据UI的功能特性选择合适的策略，并建立清晰的导航层级关系。