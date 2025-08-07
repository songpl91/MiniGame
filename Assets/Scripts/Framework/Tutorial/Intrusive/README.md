# 侵入式新手引导系统

## 概述

侵入式新手引导系统是一个基于游戏状态自动触发的Unity引导框架。它采用条件触发机制，无需手动在界面中添加引导代码，支持多种触发条件和复杂的引导流程。

## 核心特性

- **自动触发**: 基于游戏状态（等级、关卡、首次进入等）自动触发引导
- **条件丰富**: 支持等级、关卡、首次、功能解锁、时间、复合条件等多种触发方式
- **界面切换处理**: 使用延迟检测方案处理界面切换时的引导
- **轻量级设计**: 最小化性能开销，适合移动端游戏
- **高度可扩展**: 易于添加自定义触发器和步骤类型
- **调试友好**: 提供详细的调试信息和可视化工具

## 系统架构

### 核心组件

1. **TutorialTriggerSystem**: 引导触发系统核心，负责监控游戏状态并触发引导
2. **IntrusiveTutorialManager**: 侵入式引导管理器，负责执行具体的引导步骤序列
3. **TutorialTrigger**: 引导触发器基类，定义触发条件和引导创建逻辑
4. **IGameDataProvider**: 游戏数据提供接口，为触发系统提供游戏状态数据
5. **ITutorialStep**: 引导步骤接口，定义具体的引导行为

### 数据流程

```
游戏状态变化 → IGameDataProvider → TutorialTriggerSystem → 检查触发条件 → 创建引导序列 → IntrusiveTutorialManager → 执行引导步骤
```

## 使用方式

### 基础设置

1. **在场景中添加组件**：
```csharp
// 添加到GameObject上
gameObject.AddComponent<TutorialTriggerSystem>();
```

2. **实现游戏数据提供者**：
```csharp
public class MyGameDataProvider : IGameDataProvider
{
    public int GetPlayerLevel() => PlayerData.Level;
    public int GetCurrentStage() => GameManager.CurrentStage;
    // ... 实现其他接口方法
}
```

3. **创建自定义引导触发器**：
```csharp
public class MyTutorial : LevelBasedTrigger
{
    public MyTutorial() : base("my_tutorial", "我的引导", 5) { }
    
    public override List<TutorialStepBase> CreateTutorialSequence()
    {
        return new List<TutorialStepBase>
        {
            new ShowMessageStep("恭喜达到5级！"),
            new ClickButtonStep(shopButton, "点击商店按钮")
        };
    }
}
```

4. **注册触发器**：
```csharp
var triggerSystem = TutorialTriggerSystem.Instance;
triggerSystem.RegisterTrigger(new MyTutorial());
```

### 内置触发器类型

- **LevelBasedTrigger**: 基于玩家等级触发
- **StageBasedTrigger**: 基于关卡进度触发
- **FirstTimeBasedTrigger**: 基于首次进入触发
- **FunctionUnlockTrigger**: 基于功能解锁触发
- **TimeBasedTrigger**: 基于游戏时长或登录天数触发
- **CompositeConditionTrigger**: 基于复合条件触发

### 内置引导步骤

- **ShowMessageStep**: 显示消息提示
- **ClickButtonStep**: 点击按钮引导
- **WaitTimeStep**: 等待时间
- **DelayedDetectionTutorialStep**: 延迟检测引导步骤（处理界面切换）

## 调试功能

系统提供了丰富的调试功能：

- **实时状态面板**: 显示当前触发器状态和引导进度
- **快捷键操作**: 
  - `Ctrl+Q`: 停止当前引导
  - `Ctrl+W`: 跳过当前步骤
  - `Ctrl+I`: 显示系统信息
- **模拟测试方法**: 可以模拟等级提升、关卡完成等事件

## 完整示例

参考 `Examples` 目录下的示例文件：
- `GameTutorialManager.cs`: 完整的游戏引导管理器示例
- `GameTutorialTriggers.cs`: 各种引导触发器的实现示例
- `RealGameTutorialGuide.md`: 详细的使用指南

## 适用场景

- 新手教程引导（自动触发，无需手动调用）
- 功能解锁提示（基于游戏进度自动显示）
- 等级达成奖励引导（等级提升时自动触发）
- 复杂操作演示（支持界面切换处理）
- 游戏机制教学（基于条件智能触发）