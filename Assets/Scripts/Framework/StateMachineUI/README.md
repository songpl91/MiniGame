# 状态机UI框架 (StateMachine UI Framework)

## 概述

状态机UI框架是一个基于状态机模式设计的Unity UI管理系统，提供了完整的UI状态管理、转换控制、生命周期管理和事件处理功能。该框架特别适合需要复杂UI状态管理的游戏项目。

## 核心特性

### 🎯 状态管理
- **多种状态类型**：Normal（普通）、Overlay（叠加）、Exclusive（独占）、System（系统）、Temporary（临时）
- **状态优先级**：支持状态优先级控制，高优先级状态可以覆盖低优先级状态
- **状态中断控制**：可配置状态是否允许被中断
- **状态历史记录**：自动记录状态转换历史，支持返回上一个状态

### 🔄 状态转换
- **智能转换规则**：根据状态类型自动处理状态转换逻辑
- **转换条件检查**：支持自定义状态转换条件
- **数据传递**：状态转换时支持数据传递
- **转换动画**：支持状态转换动画效果

### 🏗️ 架构设计
- **单例管理器**：StateMachineUIManager 提供全局访问点
- **状态工厂**：UIStateFactory 负责状态实例的创建和管理
- **状态基类**：UIStateBase 提供统一的状态生命周期管理
- **接口设计**：IUIState 定义状态标准接口

### 🎮 生命周期
- **OnEnter**：状态进入时调用
- **OnUpdate**：状态更新时调用
- **OnExit**：状态退出时调用
- **OnPause**：状态暂停时调用
- **OnResume**：状态恢复时调用

## 项目结构

```
StateMachineUI/
├── Core/                          # 核心系统
│   ├── IUIState.cs               # 状态接口定义
│   ├── UIStateBase.cs            # 状态基类
│   ├── UIStateMachine.cs         # 状态机核心
│   ├── StateMachineUIManager.cs  # UI管理器
│   └── UIStateFactory.cs         # 状态工厂
├── States/                        # 状态实现
│   ├── MainMenuState.cs          # 主菜单状态
│   ├── GamePlayState.cs          # 游戏状态
│   ├── SettingsState.cs          # 设置状态
│   ├── PauseState.cs             # 暂停状态
│   └── LoadingState.cs           # 加载状态
├── Examples/                      # 示例代码
│   └── StateMachineUIExample.cs  # 使用示例
└── README.md                      # 说明文档
```

## 快速开始

### 1. 基础设置

```csharp
// 获取UI管理器实例
var uiManager = StateMachineUIManager.Instance;

// 设置UI根节点
uiManager.SetUIRoot(UIStateType.Normal, normalRoot);
uiManager.SetUIRoot(UIStateType.Overlay, overlayRoot);
uiManager.SetUIRoot(UIStateType.System, systemRoot);

// 注册状态
uiManager.RegisterState<MainMenuState>("MainMenu");
uiManager.RegisterState<GamePlayState>("GamePlay");
uiManager.RegisterState<SettingsState>("Settings");

// 初始化系统
uiManager.Initialize();
```

### 2. 状态转换

```csharp
// 简单状态转换
uiManager.TransitionToState("MainMenu");

// 带数据的状态转换
var gameData = new GameData { Level = 1, Score = 0 };
uiManager.TransitionToState("GamePlay", gameData);

// 返回上一个状态
uiManager.GoBack();
```

### 3. 创建自定义状态

```csharp
public class CustomState : UIStateBase
{
    public CustomState()
    {
        StateName = "Custom";
        StateType = UIStateType.Normal;
        Priority = 5;
        CanBeInterrupted = true;
    }
    
    public override void OnEnter(object data = null)
    {
        base.OnEnter(data);
        // 状态进入逻辑
        CreateUI();
        InitializeUI();
    }
    
    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        // 状态更新逻辑
    }
    
    public override void OnExit()
    {
        // 状态退出逻辑
        CleanupUI();
        base.OnExit();
    }
}
```

## 状态类型说明

### Normal（普通状态）
- 标准的UI状态，可以与其他Normal状态共存
- 适用于：主菜单、游戏界面等主要UI

### Overlay（叠加状态）
- 叠加在其他状态之上显示
- 不会关闭底层状态，只是暂停它们
- 适用于：设置面板、背包界面等

### Exclusive（独占状态）
- 独占显示，会关闭所有其他状态
- 适用于：加载界面、过场动画等

### System（系统状态）
- 系统级状态，优先级最高
- 适用于：系统提示、错误对话框等

### Temporary（临时状态）
- 临时状态，通常自动关闭
- 适用于：提示信息、确认对话框等

## 内置状态介绍

### MainMenuState（主菜单状态）
- 游戏主菜单界面
- 包含开始游戏、设置、成就、商店、退出等功能
- 支持空闲演示和背景音乐

### GamePlayState（游戏状态）
- 游戏进行中的UI界面
- 包含分数、时间、生命、血条、能量条等游戏信息
- 支持暂停、设置、帮助、背包等功能

### SettingsState（设置状态）
- 游戏设置界面
- 包含音频、画质、游戏、控制等分类设置
- 支持设置保存和恢复

### PauseState（暂停状态）
- 游戏暂停界面
- 包含继续游戏、设置、保存、加载、返回主菜单等功能
- 支持快速保存/加载功能

### LoadingState（加载状态）
- 游戏加载界面
- 支持进度条显示、加载任务管理、提示信息轮播
- 支持自定义加载任务

## 高级功能

### 状态转换规则

```csharp
// 添加状态转换规则
stateMachine.AddTransitionRule("GamePlay", "Pause", 
    () => Input.GetKeyDown(KeyCode.Escape));

// 检查状态转换条件
if (currentState.CanTransitionTo("Settings"))
{
    TransitionToState("Settings");
}
```

### 状态数据传递

```csharp
// 定义状态数据
public class GameData
{
    public int Level;
    public int Score;
    public int Lives;
}

// 传递数据
var gameData = new GameData { Level = 1, Score = 0, Lives = 3 };
uiManager.TransitionToState("GamePlay", gameData);

// 在状态中接收数据
public override void OnEnter(object data = null)
{
    if (data is GameData gameData)
    {
        currentLevel = gameData.Level;
        currentScore = gameData.Score;
        currentLives = gameData.Lives;
    }
}
```

### 事件监听

```csharp
// 监听状态改变事件
uiManager.OnUIStateChanged += (from, to, data) => 
{
    Debug.Log($"状态改变: {from} -> {to}");
};

// 监听初始化完成事件
uiManager.OnInitialized += () => 
{
    Debug.Log("UI系统初始化完成");
};

// 监听错误事件
uiManager.OnUIError += (error) => 
{
    Debug.LogError($"UI错误: {error}");
};
```

### 状态配置

```csharp
// 使用状态配置
var config = new UIStateConfig
{
    StateName = "Custom",
    StateType = UIStateType.Normal,
    Priority = 5,
    CanBeInterrupted = true,
    UISettings = new UISettings
    {
        PrefabPath = "UI/CustomUI",
        EnableAnimation = true,
        FadeInDuration = 0.3f,
        FadeOutDuration = 0.2f
    }
};

uiManager.RegisterState<CustomState>("Custom", config);
```

## 调试功能

### 调试模式
```csharp
// 启用调试模式
uiManager.SetDebugMode(true);

// 获取调试信息
var debugInfo = uiManager.GetDebugInfo();
Debug.Log($"活跃状态: {debugInfo.ActiveStateCount}");
Debug.Log($"状态历史: {debugInfo.StateHistoryCount}");
```

### 状态信息查询
```csharp
// 获取当前活跃状态
var activeStates = stateMachine.GetActiveStates();

// 获取状态历史
var stateHistory = stateMachine.GetStateHistory();

// 检查状态是否存在
bool hasState = stateMachine.HasState("MainMenu");

// 获取状态实例
var state = stateMachine.GetState("MainMenu");
```

## 性能优化

### 状态缓存
- 状态实例自动缓存，避免重复创建
- 支持手动清理缓存释放内存
- 预制体加载缓存，提高加载速度

### 内存管理
- 自动管理UI对象生命周期
- 状态退出时自动清理资源
- 支持手动触发垃圾回收

### 异步加载
- 支持异步加载UI预制体
- 加载状态提供进度反馈
- 避免加载时卡顿

## 最佳实践

### 1. 状态设计原则
- 每个状态职责单一，功能明确
- 状态之间保持松耦合
- 合理设置状态优先级和类型

### 2. 数据管理
- 使用数据类传递状态参数
- 避免在状态间直接共享数据
- 重要数据及时持久化

### 3. 性能考虑
- 避免在Update中进行复杂计算
- 合理使用对象池减少GC
- 及时清理不需要的UI对象

### 4. 错误处理
- 添加状态转换条件检查
- 处理状态加载失败情况
- 提供回退机制

## 扩展开发

### 自定义状态类型
```csharp
public enum CustomUIStateType
{
    Dialog = 100,
    Popup = 101,
    Tutorial = 102
}
```

### 自定义转换动画
```csharp
public class CustomTransitionAnimation : MonoBehaviour
{
    public void PlayTransition(UIStateBase fromState, UIStateBase toState)
    {
        // 自定义转换动画逻辑
    }
}
```

### 状态持久化
```csharp
public class StatePersistence
{
    public void SaveState(UIStateBase state)
    {
        var data = state.GetStateData();
        // 保存状态数据
    }
    
    public void LoadState(string stateName)
    {
        // 加载状态数据
    }
}
```

## 常见问题

### Q: 如何处理状态转换失败？
A: 检查CanTransitionTo方法返回值，添加转换条件验证，提供错误回调处理。

### Q: 如何实现状态间通信？
A: 使用事件系统、数据传递或共享数据管理器，避免直接引用。

### Q: 如何优化加载性能？
A: 使用异步加载、预制体缓存、资源预加载等技术。

### Q: 如何处理复杂的UI层级？
A: 合理设置状态类型和优先级，使用UI根节点分层管理。

## 版本历史

- **v1.0.0** - 初始版本，包含核心状态机功能
- 支持基本状态管理和转换
- 提供内置状态实现
- 包含完整的示例代码

## 许可证

本框架采用MIT许可证，可自由使用和修改。

## 贡献

欢迎提交Issue和Pull Request来改进这个框架。

---

*更多详细信息请参考源代码注释和示例代码。*