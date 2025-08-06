# SplUI Framework 使用指南

## 概述

SplUI是一个高效、简洁、可扩展且易用的Unity UI框架。它采用抽象类设计，避免了接口冗余，提供了完整的UI面板生命周期管理、动画系统和组件系统。

## 核心特性

- **极简设计**: 只使用抽象类，避免接口冗余
- **高效管理**: 单例管理器，统一的面板生命周期
- **动画支持**: 内置多种动画类型（淡入淡出、缩放、滑动等）
- **组件系统**: 支持可复用的UI组件
- **Unity友好**: 充分利用Unity特性，易于使用
- **可扩展性**: 灵活的架构，便于扩展新功能

## 框架结构

```
SplUI/
├── Core/                    # 核心文件
│   ├── SplUIBase.cs        # UI面板基类
│   ├── SplUIManager.cs     # UI管理器
│   ├── ISplUIComponent.cs  # UI组件接口
│   ├── SplUIComponent.cs   # UI组件基类
│   └── SplUIAnimator.cs    # 动画控制器
├── Config/                 # 配置文件
│   └── SplUIConfig.cs      # 框架配置
├── Examples/               # 示例代码
│   ├── ExamplePanel.cs     # 示例面板
│   └── ExampleComponent.cs # 示例组件
└── README.md              # 使用指南
```

## 快速开始

### 1. 创建UI面板

继承 `SplUIBase` 类创建你的UI面板：

```csharp
using Framework.SplUI.Core;

public class MyPanel : SplUIBase
{
    protected override void OnInitialize()
    {
        // 设置面板基本信息
        panelId = "MyPanel";
        displayName = "我的面板";
        panelType = SplUIType.Normal;
        
        // 设置动画
        showAnimationType = SplUIAnimationType.Fade;
        hideAnimationType = SplUIAnimationType.Fade;
    }
    
    protected override void OnShow()
    {
        // 面板显示时的逻辑
    }
    
    protected override void OnHide()
    {
        // 面板隐藏时的逻辑
    }
}
```

### 2. 使用UI管理器

通过 `SplUIManager` 管理面板的显示和隐藏：

```csharp
// 显示面板
SplUIManager.Instance.ShowPanel("MyPanel");

// 隐藏面板
SplUIManager.Instance.HidePanel("MyPanel");

// 销毁面板
SplUIManager.Instance.DestroyPanel("MyPanel");

// 获取面板
MyPanel panel = SplUIManager.Instance.GetPanel<MyPanel>("MyPanel");
```

### 3. 创建UI组件

继承 `SplUIComponent` 类创建可复用的UI组件：

```csharp
using Framework.SplUI.Core;

public class MyComponent : SplUIComponent
{
    protected override void OnInitialize()
    {
        // 组件初始化逻辑
    }
    
    protected override void OnComponentUpdate()
    {
        // 组件更新逻辑
    }
}
```

## 面板类型

框架支持四种面板类型：

- **Normal**: 普通面板，支持堆栈管理
- **Popup**: 弹窗面板，显示在普通面板之上
- **System**: 系统面板，最高优先级
- **HUD**: HUD面板，用于游戏内UI

## 动画系统

框架内置多种动画类型：

- **None**: 无动画
- **Fade**: 淡入淡出
- **Scale**: 缩放动画
- **Slide**: 滑动动画
- **FadeScale**: 淡入淡出+缩放

### 使用动画

```csharp
// 在面板中设置动画
protected override void OnInitialize()
{
    showAnimationType = SplUIAnimationType.FadeScale;
    hideAnimationType = SplUIAnimationType.FadeScale;
    animationDuration = 0.3f;
}
```

## 配置系统

使用 `SplUIConfig` 配置框架参数：

```csharp
// 创建配置资源
// Assets -> Create -> SplUI -> Config

// 在代码中使用配置
SplUIConfig config = Resources.Load<SplUIConfig>("SplUIConfig");
```

## 事件系统

框架提供丰富的事件支持：

```csharp
// 监听面板事件
SplUIManager.Instance.OnPanelShown += (panel) => {
    Debug.Log($"面板显示: {panel.PanelId}");
};

SplUIManager.Instance.OnPanelHidden += (panel) => {
    Debug.Log($"面板隐藏: {panel.PanelId}");
};
```

## 最佳实践

### 1. 面板设计

- 每个面板负责单一功能
- 使用组件系统拆分复杂UI
- 合理设置面板类型和优先级

### 2. 性能优化

- 使用对象池管理频繁创建的面板
- 及时销毁不需要的面板
- 避免在Update中进行复杂计算

### 3. 代码组织

- 将UI逻辑与业务逻辑分离
- 使用事件系统进行解耦
- 添加详细的注释和文档

## 示例代码

查看 `Examples` 文件夹中的示例代码：

- `ExamplePanel.cs`: 完整的面板示例
- `ExampleComponent.cs`: UI组件示例

## 常见问题

### Q: 如何创建模态对话框？

A: 使用 `SplUIType.Popup` 类型，并在显示时禁用背景交互：

```csharp
protected override void OnShow()
{
    // 禁用背景交互
    SplUIManager.Instance.SetBackgroundInteractable(false);
}

protected override void OnHide()
{
    // 恢复背景交互
    SplUIManager.Instance.SetBackgroundInteractable(true);
}
```

### Q: 如何实现面板间数据传递？

A: 可以通过以下方式：

1. 构造函数参数
2. 公共属性设置
3. 事件系统
4. 全局数据管理器

### Q: 如何自定义动画？

A: 继承 `SplUIAnimator` 类并重写动画方法，或者直接在面板中实现自定义动画逻辑。

## 更新日志

### v1.0.0
- 初始版本发布
- 基础面板管理功能
- 动画系统
- 组件系统
- 配置系统

## 技术支持

如有问题或建议，请联系开发团队。