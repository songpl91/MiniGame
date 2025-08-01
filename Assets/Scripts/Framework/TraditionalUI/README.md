# 传统UI框架 (Traditional UI Framework)

一个功能完善、易于使用的Unity UI管理框架，提供传统的面板管理方式和丰富的UI功能。

## 🚀 快速开始

```csharp
// 打开面板
TraditionalUIManager.Instance.OpenPanel("MainMenu");

// 显示消息框
MessageBoxPanel.ShowInfo("提示", "操作成功！");

// 播放音效
UIAudioManager.Instance.PlayButtonClick();
```

## 📁 项目结构

```
TraditionalUI/
├── Core/                          # 核心系统
│   ├── TraditionalUIManager.cs    # UI管理器（单例）
│   ├── TraditionalUIPanel.cs      # 面板基类
│   └── UIConfig.cs                # 配置管理
├── Panels/                        # 示例面板
│   ├── MainMenuPanel.cs           # 主菜单面板
│   ├── SettingsPanel.cs           # 设置面板
│   ├── MessageBoxPanel.cs         # 消息框面板
│   └── ShopPanel.cs               # 商店面板
├── Utils/                         # 工具类
│   ├── UIAnimationHelper.cs       # 动画辅助工具
│   └── UIAudioManager.cs          # 音效管理器
├── Examples/                      # 使用示例
│   └── TraditionalUIExample.cs    # 完整使用示例
├── 传统UI框架使用指南.md           # 详细使用指南
└── README.md                      # 项目说明
```

## ✨ 核心特性

### 🎯 面板管理系统
- **完整生命周期**：初始化 → 显示 → 隐藏 → 销毁
- **多种面板类型**：Normal、Popup、System、Top
- **UI栈管理**：支持历史记录和返回功能
- **层级控制**：自动管理显示层级

### 🎨 动画系统
- **丰富动画效果**：淡入淡出、缩放、滑动、弹跳、摇摆
- **自定义缓动**：多种缓动函数支持
- **动画链式调用**：支持序列动画和回调

### 🔊 音效系统
- **UI音效管理**：按钮、面板、成功/失败等音效
- **音量控制**：支持音量调节和静音
- **自动音效组件**：按钮自动播放音效

### ⚙️ 配置系统
- **集中配置**：所有设置统一管理
- **面板配置**：预制体路径、动画、层级等
- **性能配置**：缓存池、预加载、限制等

## 🎮 面板类型

| 类型 | 用途 | 特点 |
|------|------|------|
| **Normal** | 主要游戏界面 | 支持UI栈，可被覆盖 |
| **Popup** | 弹窗对话框 | 模态显示，自动管理 |
| **System** | 系统级UI | 高优先级，不受影响 |
| **Top** | 顶层面板 | 始终最顶层显示 |

## 🎬 动画效果

| 动画类型 | 效果描述 | 使用场景 |
|----------|----------|----------|
| **Fade** | 淡入淡出 | 平滑过渡 |
| **Scale** | 缩放显示 | 弹窗效果 |
| **Slide** | 滑动进出 | 页面切换 |
| **Bounce** | 弹跳效果 | 强调显示 |
| **Shake** | 摇摆效果 | 错误提示 |

## 🔧 使用示例

### 创建自定义面板

```csharp
public class MyPanel : TraditionalUIPanel
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Text titleText;
    
    protected override void InitializePanel()
    {
        base.InitializePanel();
        closeButton.onClick.AddListener(CloseSelf);
        titleText.text = "我的面板";
    }
    
    protected override void OnPanelShow()
    {
        base.OnPanelShow();
        // 面板显示时的逻辑
    }
}
```

### 配置面板

```csharp
// 在UIConfig中添加配置
new PanelConfig
{
    panelName = "MyPanel",
    prefabPath = "UI/Panels/MyPanel",
    panelType = UIPanelType.Normal,
    animationType = UIAnimationType.Scale
}
```

### 使用消息框

```csharp
// 快速显示消息
MessageBoxPanel.ShowInfo("提示", "操作成功！");
MessageBoxPanel.ShowConfirm("确认", "确定删除吗？", onConfirm, onCancel);

// 自定义消息框
var data = new MessageBoxData
{
    title = "自定义",
    message = "自定义消息内容",
    messageType = MessageType.Warning,
    onConfirm = () => Debug.Log("确认")
};
TraditionalUIManager.Instance.OpenPanel("MessageBox", data);
```

## 📊 性能优化

### 缓存机制
- **面板缓存**：避免重复创建销毁
- **预加载**：提前加载常用面板
- **对象池**：复用UI对象

### 内存管理
- **智能销毁**：自动清理不需要的面板
- **引用管理**：避免内存泄漏
- **资源释放**：及时释放资源

## 🛠️ 调试功能

### 可视化调试
- **面板边框**：显示面板边界
- **层级信息**：显示面板层级
- **状态监控**：实时监控面板状态

### 日志系统
- **详细日志**：记录面板操作
- **性能监控**：监控性能指标
- **错误追踪**：快速定位问题

## 📋 API 参考

### TraditionalUIManager
```csharp
// 面板操作
OpenPanel(string panelName, object data = null)
ClosePanel(string panelName)
CloseAllPanels()
GoBack()

// 查询方法
GetOpenPanelCount()
GetPopupCount()
IsAnyPanelOpen()
GetTopPanelName()

// 缓存管理
PreloadPanel(string panelName)
ClearCache()
```

### UIAnimationHelper
```csharp
// 淡入淡出
FadeIn(CanvasGroup canvasGroup, float duration, Action onComplete = null)
FadeOut(CanvasGroup canvasGroup, float duration, Action onComplete = null)

// 缩放动画
ScaleIn(Transform target, float duration, Action onComplete = null)
ScaleOut(Transform target, float duration, Action onComplete = null)

// 滑动动画
SlideInFromLeft(RectTransform target, float duration, Action onComplete = null)
SlideOutToRight(RectTransform target, float duration, Action onComplete = null)
```

### UIAudioManager
```csharp
// 音效播放
PlayButtonClick()
PlaySuccess()
PlayError()
PlayPurchase()

// 音量控制
SetVolume(float volume)
SetMute(bool mute)
```

## 🎯 适用场景

- ✅ **游戏UI系统**：RPG、策略、休闲游戏等
- ✅ **应用界面**：工具应用、教育应用等
- ✅ **原型开发**：快速UI原型制作
- ✅ **商业项目**：需要稳定UI框架的项目

## 🔄 版本历史

### v1.0.0 (当前版本)
- ✨ 完整的面板管理系统
- ✨ 丰富的动画效果
- ✨ 音效系统集成
- ✨ 配置化管理
- ✨ 调试工具支持

## 📝 许可证

本项目采用 MIT 许可证，详见 LICENSE 文件。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request 来改进这个框架！

## 📞 联系方式

如有问题或建议，请通过以下方式联系：
- 📧 Email: [your-email@example.com]
- 💬 QQ群: [123456789]
- 🐛 Issues: [GitHub Issues]

---

**传统UI框架** - 让Unity UI开发更简单、更高效！ 🚀