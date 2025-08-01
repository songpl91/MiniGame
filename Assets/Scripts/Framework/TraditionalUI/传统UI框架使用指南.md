# 传统UI框架使用指南

## 概述

传统UI框架是一个功能完善的Unity UI管理系统，提供了面板管理、动画效果、音效系统、层级控制等核心功能。该框架采用传统的面板管理方式，适合大多数游戏和应用的UI需求。

## 核心特性

### 🎯 面板管理
- **完整的生命周期管理**：初始化、显示、隐藏、销毁
- **多种面板类型**：普通面板、弹窗、系统面板、顶层面板
- **UI栈管理**：支持面板历史记录和返回功能
- **层级控制**：自动管理面板的显示层级

### 🎨 动画系统
- **多种动画效果**：淡入淡出、缩放、滑动、弹跳、摇摆
- **自定义缓动函数**：支持多种缓动效果
- **动画链式调用**：支持动画序列和回调

### 🔊 音效系统
- **UI音效管理**：按钮点击、面板切换、成功/失败等
- **音量控制**：支持音效音量调节和静音
- **自动音效**：按钮组件自动播放音效

### ⚙️ 配置系统
- **集中配置管理**：所有设置集中在UIConfig中
- **面板配置**：预制体路径、动画类型、层级设置
- **性能配置**：缓存池、预加载、最大面板数限制

## 快速开始

### 1. 基础设置

```csharp
// 获取UI管理器实例
var uiManager = TraditionalUIManager.Instance;

// 打开面板
uiManager.OpenPanel("MainMenu");

// 关闭面板
uiManager.ClosePanel("MainMenu");

// 返回上一个面板
uiManager.GoBack();
```

### 2. 创建自定义面板

```csharp
using Framework.TraditionalUI.Core;

public class MyPanel : TraditionalUIPanel
{
    [Header("UI组件")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Text titleText;
    
    protected override void InitializePanel()
    {
        base.InitializePanel();
        
        // 设置按钮事件
        closeButton.onClick.AddListener(OnCloseClick);
        
        // 初始化UI
        titleText.text = "我的面板";
    }
    
    protected override void OnPanelShow()
    {
        base.OnPanelShow();
        Debug.Log("面板显示");
    }
    
    private void OnCloseClick()
    {
        CloseSelf();
    }
}
```

### 3. 配置面板

在UIConfig中添加面板配置：

```csharp
// 在UIConfig的panelConfigs列表中添加
new PanelConfig
{
    panelName = "MyPanel",
    prefabPath = "UI/Panels/MyPanel",
    panelType = UIPanelType.Normal,
    animationType = UIAnimationType.Scale,
    isModal = false,
    destroyOnClose = false
}
```

## 详细功能说明

### 面板类型

#### Normal（普通面板）
- 用于主要的游戏界面
- 支持UI栈管理
- 可以被其他面板覆盖

#### Popup（弹窗）
- 用于临时显示的对话框
- 通常具有模态特性
- 自动管理弹窗列表

#### System（系统面板）
- 用于系统级UI（如加载界面）
- 优先级较高
- 不受普通面板影响

#### Top（顶层面板）
- 始终显示在最顶层
- 用于重要提示或调试信息

### 动画类型

#### Fade（淡入淡出）
```csharp
// 自动应用淡入淡出动画
panelConfig.animationType = UIAnimationType.Fade;

// 手动控制
UIAnimationHelper.FadeIn(canvasGroup, 0.5f, onComplete);
UIAnimationHelper.FadeOut(canvasGroup, 0.5f, onComplete);
```

#### Scale（缩放）
```csharp
// 从小到大显示，从大到小隐藏
panelConfig.animationType = UIAnimationType.Scale;

// 手动控制
UIAnimationHelper.ScaleIn(transform, 0.3f, onComplete);
UIAnimationHelper.ScaleOut(transform, 0.3f, onComplete);
```

#### Slide（滑动）
```csharp
// 支持四个方向的滑动
panelConfig.animationType = UIAnimationType.SlideFromLeft;

// 手动控制
UIAnimationHelper.SlideInFromLeft(rectTransform, 0.3f, onComplete);
UIAnimationHelper.SlideOutToRight(rectTransform, 0.3f, onComplete);
```

### 音效系统

#### 基础音效播放
```csharp
// 播放按钮点击音效
UIAudioManager.Instance.PlayButtonClick();

// 播放成功音效
UIAudioManager.Instance.PlaySuccess();

// 播放错误音效
UIAudioManager.Instance.PlayError();
```

#### 自动音效组件
```csharp
// 为按钮添加自动音效
var audioButton = button.gameObject.AddComponent<UIAudioButton>();
audioButton.clickAudioType = UIAudioType.ButtonClick;
audioButton.hoverAudioType = UIAudioType.ButtonHover;
```

#### 音效设置
```csharp
// 设置音效音量
UIAudioManager.Instance.SetVolume(0.8f);

// 静音/取消静音
UIAudioManager.Instance.SetMute(true);
```

### 消息框系统

#### 快速显示消息框
```csharp
// 信息消息
MessageBoxPanel.ShowInfo("标题", "消息内容", onConfirm);

// 警告消息
MessageBoxPanel.ShowWarning("警告", "警告内容", onConfirm);

// 错误消息
MessageBoxPanel.ShowError("错误", "错误内容", onConfirm);

// 确认对话框
MessageBoxPanel.ShowConfirm("确认", "确认内容", onConfirm, onCancel);
```

#### 自定义消息框
```csharp
var messageData = new MessageBoxData
{
    title = "自定义标题",
    message = "自定义消息内容",
    messageType = MessageType.Info,
    showCancelButton = true,
    confirmButtonText = "确定",
    cancelButtonText = "取消",
    onConfirm = () => Debug.Log("确认"),
    onCancel = () => Debug.Log("取消")
};

TraditionalUIManager.Instance.OpenPanel("MessageBox", messageData);
```

## 高级功能

### 面板数据传递

```csharp
// 打开面板时传递数据
var shopData = new ShopData { category = ShopCategory.Weapons };
uiManager.OpenPanel("Shop", shopData);

// 在面板中接收数据
public override void SetData(object data)
{
    if (data is ShopData shopData)
    {
        currentCategory = shopData.category;
        RefreshShopItems();
    }
}
```

### 面板缓存

```csharp
// 配置面板缓存
panelConfig.destroyOnClose = false; // 关闭时不销毁，保存在缓存中

// 预加载面板
uiManager.PreloadPanel("MainMenu");

// 清理缓存
uiManager.ClearCache();
```

### 性能优化

#### 面板池管理
```csharp
// 在UIConfig中配置
public int maxVisiblePanels = 10;      // 最大可见面板数
public int panelCachePoolSize = 5;     // 面板缓存池大小
public bool enablePreloading = true;   // 启用预加载
```

#### 内存管理
```csharp
// 定期清理不需要的面板
uiManager.CleanupUnusedPanels();

// 强制垃圾回收
System.GC.Collect();
```

### 调试功能

#### 调试信息显示
```csharp
// 在UIConfig中启用调试
public bool showDebugInfo = true;      // 显示调试信息
public bool showPanelBorders = true;   // 显示面板边框
public bool enableDetailedLogging = true; // 详细日志
```

#### 运行时信息
```csharp
// 获取当前打开的面板数量
int openCount = uiManager.GetOpenPanelCount();

// 获取弹窗数量
int popupCount = uiManager.GetPopupCount();

// 获取当前顶层面板
string topPanel = uiManager.GetTopPanelName();
```

## 最佳实践

### 1. 面板设计原则
- **单一职责**：每个面板只负责一个功能模块
- **松耦合**：面板之间通过事件或数据传递通信
- **可复用**：设计通用的面板组件

### 2. 性能优化建议
- **合理使用缓存**：频繁使用的面板不要销毁
- **预加载关键面板**：游戏开始时预加载主要面板
- **控制同时显示的面板数量**：避免过多面板同时显示

### 3. 动画使用建议
- **保持一致性**：同类型面板使用相同动画
- **控制动画时长**：不要过长影响用户体验
- **提供跳过选项**：允许用户跳过动画

### 4. 音效使用建议
- **适度使用**：不要过度使用音效
- **提供设置选项**：允许用户调节或关闭音效
- **音效质量**：使用高质量的音效文件

## 常见问题

### Q: 如何处理面板之间的通信？
A: 可以使用事件系统、数据传递或单例模式的数据管理器。

### Q: 如何优化面板加载性能？
A: 使用预加载、对象池、异步加载等技术。

### Q: 如何处理不同分辨率的适配？
A: 使用Unity的Canvas Scaler和Anchor系统。

### Q: 如何添加自定义动画？
A: 继承UIAnimationHelper类或直接使用DOTween等动画库。

## 示例项目

查看 `TraditionalUIExample.cs` 文件，其中包含了框架的完整使用示例，包括：
- 面板打开/关闭
- 动画效果演示
- 音效播放测试
- 消息框使用
- 性能监控

## 总结

传统UI框架提供了一套完整的UI管理解决方案，具有以下优势：

✅ **易于使用**：简单的API设计，快速上手
✅ **功能完善**：涵盖UI开发的各个方面
✅ **性能优化**：内置多种性能优化机制
✅ **可扩展性**：支持自定义扩展和配置
✅ **调试友好**：提供丰富的调试信息和工具

该框架适用于各种类型的游戏和应用，特别是需要复杂UI交互的项目。通过合理的配置和使用，可以大大提高UI开发效率和用户体验。