# SplUITypes 使用示例

## 概述

`SplUITypes.cs` 是 SplUI 框架的类型定义文件，集中管理了所有UI相关的枚举、常量、委托和结构体。本文档提供详细的使用示例。

## 基本使用

### 1. 面板类型定义

```csharp
using Framework.SplUI.Core;

public class MyPanel : SplUIBase
{
    protected override void OnInitialize()
    {
        // 设置面板类型
        panelType = SplUIType.Popup;  // 弹窗类型
        
        // 根据类型设置不同的层级
        switch (panelType)
        {
            case SplUIType.Normal:
                sortingOrder = (int)SplUILayerType.Normal;
                break;
            case SplUIType.Popup:
                sortingOrder = (int)SplUILayerType.Popup;
                break;
            case SplUIType.System:
                sortingOrder = (int)SplUILayerType.System;
                break;
            case SplUIType.HUD:
                sortingOrder = (int)SplUILayerType.HUD;
                break;
        }
    }
}
```

### 2. 动画配置

```csharp
public class AnimatedPanel : SplUIBase
{
    protected override void OnInitialize()
    {
        // 使用默认动画配置
        var defaultConfig = SplUIAnimationConfig.Default;
        
        // 自定义动画配置
        var customConfig = new SplUIAnimationConfig
        {
            showAnimation = SplUIAnimationType.SlideFromBottom,
            hideAnimation = SplUIAnimationType.SlideToBottom,
            duration = 0.5f,
            easeType = SplUIEaseType.EaseOut,
            slideDirection = SplUISlideDirection.Bottom,
            ignoreTimeScale = false
        };
        
        // 应用配置到动画组件
        var animComponent = GetAnimationComponent();
        if (animComponent != null)
        {
            animComponent.ShowAnimation = customConfig.showAnimation;
            animComponent.HideAnimation = customConfig.hideAnimation;
            animComponent.AnimationDuration = customConfig.duration;
        }
    }
}
```

### 3. 面板状态管理

```csharp
public class StatefulPanel : SplUIBase
{
    private SplUIPanelState currentState = SplUIPanelState.Uninitialized;
    
    protected override void OnInitialize()
    {
        currentState = SplUIPanelState.Initialized;
        Debug.Log($"面板状态: {currentState}");
    }
    
    protected override void OnShow(object data = null)
    {
        currentState = SplUIPanelState.Showing;
        Debug.Log($"面板状态: {currentState}");
    }
    
    protected override void OnShowAnimationComplete()
    {
        base.OnShowAnimationComplete();
        currentState = SplUIPanelState.Shown;
        Debug.Log($"面板状态: {currentState}");
    }
    
    protected override void OnHide()
    {
        currentState = SplUIPanelState.Hiding;
        Debug.Log($"面板状态: {currentState}");
    }
    
    protected override void OnHideAnimationComplete()
    {
        base.OnHideAnimationComplete();
        currentState = SplUIPanelState.Hidden;
        Debug.Log($"面板状态: {currentState}");
    }
}
```

### 4. 事件处理

```csharp
public class EventDrivenPanel : SplUIBase
{
    // 定义事件
    public event SplUITypes.PanelStateChangedHandler OnStateChanged;
    public event SplUITypes.AnimationCompletedHandler OnAnimationCompleted;
    public event SplUITypes.PanelDataChangedHandler OnDataChanged;
    
    protected override void OnInitialize()
    {
        // 注册事件监听
        OnStateChanged += HandleStateChanged;
        OnAnimationCompleted += HandleAnimationCompleted;
        OnDataChanged += HandleDataChanged;
    }
    
    protected override void OnShow(object data = null)
    {
        // 触发状态变化事件
        OnStateChanged?.Invoke(panelId, this);
        
        // 触发数据变化事件
        if (data != null)
        {
            OnDataChanged?.Invoke(panelId, data);
        }
    }
    
    protected override void OnShowAnimationComplete()
    {
        base.OnShowAnimationComplete();
        
        // 触发动画完成事件
        var animComponent = GetAnimationComponent();
        if (animComponent != null)
        {
            OnAnimationCompleted?.Invoke(animComponent.ShowAnimation, true);
        }
    }
    
    private void HandleStateChanged(string panelId, SplUIBase panel)
    {
        Debug.Log($"面板状态变化: {panelId}");
    }
    
    private void HandleAnimationCompleted(SplUIAnimationType animationType, bool isShow)
    {
        Debug.Log($"动画完成: {animationType}, 显示: {isShow}");
    }
    
    private void HandleDataChanged(string panelId, object data)
    {
        Debug.Log($"数据变化: {panelId}, 数据: {data}");
    }
}
```

### 5. 面板信息配置

```csharp
public class ConfigurablePanel : SplUIBase
{
    [SerializeField] private SplUIPanelInfo panelInfo;
    
    protected override void OnInitialize()
    {
        // 配置面板信息
        panelInfo = new SplUIPanelInfo
        {
            panelId = "ConfigurablePanel",
            displayName = "可配置面板",
            panelType = SplUIType.Normal,
            sortingOrder = (int)SplUILayerType.Normal + 10,
            isSingleton = true,
            isCache = true,
            prefabPath = "UI/Panels/ConfigurablePanel"
        };
        
        // 应用配置
        this.panelId = panelInfo.panelId;
        this.displayName = panelInfo.displayName;
        this.panelType = panelInfo.panelType;
        this.sortingOrder = panelInfo.sortingOrder;
        
        Debug.Log($"面板配置: {panelInfo.displayName}, 类型: {panelInfo.panelType}");
    }
}
```

## 高级用法

### 1. 动态动画切换

```csharp
public class DynamicAnimationPanel : SplUIBase
{
    private SplUIAnimationType[] showAnimations = 
    {
        SplUIAnimationType.Fade,
        SplUIAnimationType.Scale,
        SplUIAnimationType.FadeScale,
        SplUIAnimationType.SlideFromBottom
    };
    
    private int currentAnimationIndex = 0;
    
    public void SwitchAnimation()
    {
        currentAnimationIndex = (currentAnimationIndex + 1) % showAnimations.Length;
        var newAnimation = showAnimations[currentAnimationIndex];
        
        SetShowAnimation(newAnimation);
        Debug.Log($"切换到动画: {newAnimation}");
    }
    
    public void SetRandomAnimation()
    {
        var randomAnimation = (SplUIAnimationType)UnityEngine.Random.Range(1, 13);
        SetShowAnimation(randomAnimation);
        Debug.Log($"随机动画: {randomAnimation}");
    }
}
```

### 2. 层级管理器

```csharp
public static class LayerManager
{
    /// <summary>
    /// 获取指定类型的基础层级
    /// </summary>
    public static int GetBaseLayer(SplUIType panelType)
    {
        return panelType switch
        {
            SplUIType.Normal => (int)SplUILayerType.Normal,
            SplUIType.Popup => (int)SplUILayerType.Popup,
            SplUIType.System => (int)SplUILayerType.System,
            SplUIType.HUD => (int)SplUILayerType.HUD,
            _ => (int)SplUILayerType.Normal
        };
    }
    
    /// <summary>
    /// 分配新的层级
    /// </summary>
    public static int AllocateLayer(SplUIType panelType, int offset = 0)
    {
        int baseLayer = GetBaseLayer(panelType);
        return baseLayer + offset * SplUITypes.LAYER_ORDER_INTERVAL;
    }
}
```

### 3. 动画配置管理器

```csharp
public static class AnimationConfigManager
{
    private static Dictionary<string, SplUIAnimationConfig> configs = 
        new Dictionary<string, SplUIAnimationConfig>();
    
    static AnimationConfigManager()
    {
        // 预定义配置
        configs["快速"] = new SplUIAnimationConfig
        {
            showAnimation = SplUIAnimationType.Fade,
            hideAnimation = SplUIAnimationType.Fade,
            duration = 0.1f,
            easeType = SplUIEaseType.Linear
        };
        
        configs["标准"] = SplUIAnimationConfig.Default;
        
        configs["华丽"] = new SplUIAnimationConfig
        {
            showAnimation = SplUIAnimationType.FadeScale,
            hideAnimation = SplUIAnimationType.FadeScale,
            duration = 0.8f,
            easeType = SplUIEaseType.Elastic
        };
    }
    
    public static SplUIAnimationConfig GetConfig(string name)
    {
        return configs.TryGetValue(name, out var config) ? config : SplUIAnimationConfig.Default;
    }
    
    public static void RegisterConfig(string name, SplUIAnimationConfig config)
    {
        configs[name] = config;
    }
}
```

## 常量使用

```csharp
public class ConstantUsageExample : SplUIBase
{
    protected override void OnInitialize()
    {
        // 使用预定义常量
        float duration = SplUITypes.DEFAULT_ANIMATION_DURATION;
        
        // 验证动画时长范围
        duration = Mathf.Clamp(duration, 
            SplUITypes.MIN_ANIMATION_DURATION, 
            SplUITypes.MAX_ANIMATION_DURATION);
        
        // 计算层级偏移
        int layerOffset = SplUITypes.LAYER_ORDER_INTERVAL;
        sortingOrder = (int)SplUILayerType.Popup + layerOffset;
        
        Debug.Log($"动画时长: {duration}, 层级: {sortingOrder}");
    }
}
```

## 最佳实践

1. **类型安全**: 始终使用枚举而不是魔法数字
2. **配置复用**: 使用 `SplUIAnimationConfig` 结构体管理动画配置
3. **事件驱动**: 利用预定义的委托类型实现事件通信
4. **层级管理**: 使用 `SplUILayerType` 枚举管理UI层级
5. **常量使用**: 使用 `SplUITypes` 中的常量避免硬编码

通过这些示例，你可以充分利用 `SplUITypes.cs` 提供的类型定义，编写更加规范、安全和可维护的UI代码。