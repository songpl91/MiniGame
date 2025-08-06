# SampleUI 框架

## 概述

SampleUI 是一个渐进式 UI 框架，专为 Unity 项目设计。它采用"初期极简、中期扩展、后期完善"的设计理念，能够满足从 Demo 快速搭建到复杂项目的各种需求。

## 设计理念

### 渐进式架构
- **初期（Demo阶段）**：提供极简的核心功能，快速搭建UI
- **中期（开发阶段）**：通过组件系统扩展功能，保持代码整洁
- **后期（完善阶段）**：集成状态机、配置化等高级特性

### 核心特性
1. **简单明确的继承关系**：`ISampleUIBase` -> `SampleUIBase` -> 具体面板
2. ISampleUIBase的设计是为了方便扩展其他的UI类型，但是对于Unity项目来说，这不是必要的。只有一个抽象类就可以了
3. **组合优于继承**：通过组件系统扩展功能
4. **接口驱动**：所有核心功能都有对应接口
5. **配置化**：支持通过配置文件管理UI行为

## 架构设计

### 核心类结构
```
SampleUIManager (单例管理器)
├── ISampleUIBase (面板接口)
├── SampleUIBase (面板基类)
├── ISampleUIComponent (组件接口)
├── SampleUIComponent (组件基类)
└── SampleUIConfig (配置管理)
```

### 组件系统
- `AnimationComponent`：动画效果组件
- `AudioComponent`：音效播放组件
- `InputComponent`：输入处理组件
- 更多组件可按需扩展...

## 文件结构

```
SampleUI/
├── Core/                           # 核心模块
│   ├── ISampleUIBase.cs          # 面板接口定义
│   ├── SampleUIBase.cs           # 面板基类实现
│   ├── ISampleUIComponent.cs      # 组件接口和基类
│   └── SampleUIManager.cs         # UI管理器
├── Components/                     # 组件模块
│   ├── AnimationComponent.cs      # 动画组件
│   ├── AudioComponent.cs          # 音频组件
│   └── InputComponent.cs          # 输入组件
├── Config/                         # 配置模块
│   └── SampleUIConfig.cs          # 框架配置
├── Examples/                       # 示例模块
│   ├── ExamplePanel.cs            # 示例面板
│   └── ExampleUsage.cs            # 使用示例
└── README.md                       # 说明文档
```

## 使用示例

### 基础使用

```csharp
// 1. 创建面板类
public class MainMenuPanel : SampleUIBase
{
    protected override void OnInitialized()
    {
        base.OnInitialized();
        PanelId = "MainMenu";
        DisplayName = "主菜单";
        PanelType = SampleUIBaseType.Normal;
    }
}

// 2. 显示面板
var panel = SampleUIManager.Instance.ShowPanel<MainMenuPanel>();

// 3. 隐藏面板
SampleUIManager.Instance.HidePanel("MainMenu");
```

### 组件扩展

```csharp
public class GamePanel : SampleUIBase
{
    private AnimationComponent animationComponent;
    private AudioComponent audioComponent;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // 添加动画组件
        animationComponent = AddComponent<AnimationComponent>();
        
        // 添加音频组件
        audioComponent = AddComponent<AudioComponent>();
    }
    
    private void PlayShowAnimation()
    {
        animationComponent.PlayAnimation(CustomAnimationType.ScaleBounce);
    }
    
    private void PlayClickSound()
    {
        audioComponent.PlayClickSound();
    }
}
```

### 快速开始

1. **创建UI根节点**：在场景中创建Canvas作为UI根节点
2. **初始化管理器**：确保SampleUIManager在场景中初始化
3. **创建面板预制体**：为你的UI面板创建预制体
4. **编写面板脚本**：继承SampleUIBase并实现具体逻辑
5. **显示面板**：通过SampleUIManager显示面板

### 测试示例

框架提供了完整的测试示例：

1. **ExamplePanel**：展示所有功能的完整示例面板
2. **ExampleUsage**：演示如何在项目中使用框架
3. **快捷键支持**：
   - F1：显示示例面板
   - F2：显示弹窗面板
   - F3：隐藏所有面板
   - F4：返回上一个面板
   - F5：演示动画效果
   - F6：演示音效

## 扩展指南

### 添加新组件

1. 继承 `SampleUIComponent`
2. 实现必要的生命周期方法
3. 在面板中通过 `AddComponent<T>()` 使用

```csharp
public class CustomComponent : SampleUIComponent
{
    protected override void OnInitialize()
    {
        // 组件初始化逻辑
    }
    
    protected override void OnPanelShow()
    {
        // 面板显示时的处理
    }
    
    protected override void OnPanelHide()
    {
        // 面板隐藏时的处理
    }
}
```

### 自定义面板类型

1. 在 `SampleUIBaseType` 枚举中添加新类型
2. 在 `SampleUIManager` 中添加对应的处理逻辑
3. 在配置文件中设置相应的层级信息

### 状态机集成（后期扩展）

框架预留了状态机集成的接口，可以在后期需要时：
1. 实现 `IUIState` 接口
2. 创建状态机管理器
3. 将面板与状态进行绑定

## 配置说明

通过 `SampleUIConfig` 可以配置：
- **层级设置**：不同类型面板的层级和排序
- **动画参数**：默认动画时长、缓动类型等
- **音频设置**：音效开关、音量控制等
- **输入配置**：快捷键、拖拽等输入行为
- **性能参数**：对象池、缓存等性能优化设置

## 最佳实践

1. **面板职责单一**：每个面板只负责特定的UI功能
2. **组件化扩展**：复杂功能通过组件实现，保持面板代码简洁
3. **配置驱动**：尽量通过配置文件而非硬编码管理UI行为
4. **事件解耦**：使用事件系统减少组件间的直接依赖
5. **资源管理**：合理使用对象池和缓存机制

## 性能优化

1. **对象池**：频繁创建销毁的面板使用对象池
2. **延迟加载**：非必要面板延迟到需要时再加载
3. **批量操作**：UI更新操作尽量批量进行
4. **内存管理**：及时清理不需要的资源和事件监听

## 版本规划

- **v1.0**：核心功能实现（当前版本）
- **v1.1**：组件系统完善
- **v1.2**：状态机集成
- **v2.0**：配置化系统
- **v2.1**：性能优化和工具链

## 注意事项

1. 框架采用组合模式，避免深层继承
2. 所有公共接口都有对应的抽象，便于测试和扩展
3. 配置文件支持运行时修改，便于调试
4. 组件系统支持热插拔，提高开发效率
5. 禁止使用System.Linq相关语法，保持兼容性

## 技术特点

### 组件系统优势
- **热插拔**：组件可以在运行时动态添加和移除
- **解耦合**：组件之间通过事件通信，减少直接依赖
- **可复用**：同一组件可以在多个面板中使用
- **易扩展**：新功能通过新组件实现，不影响现有代码

### 动画系统特色
- **多种动画类型**：淡入淡出、缩放、滑动、旋转、翻转等
- **缓动函数支持**：内置多种缓动函数，提供流畅的动画效果
- **自定义动画**：支持弹跳、摇摆、脉冲等特殊动画效果
- **动画序列**：支持动画序列播放和组合

### 音频系统功能
- **UI音效管理**：点击、悬停、打开、关闭等常用UI音效
- **音量控制**：支持全局和单独音量控制
- **静音功能**：支持一键静音和恢复
- **音效淡入淡出**：平滑的音量过渡效果

### 输入系统能力
- **多种输入支持**：鼠标、键盘、触摸等输入方式
- **快捷键系统**：支持单键和组合键快捷键
- **拖拽功能**：完整的拖拽事件处理
- **输入状态管理**：实时跟踪输入状态变化