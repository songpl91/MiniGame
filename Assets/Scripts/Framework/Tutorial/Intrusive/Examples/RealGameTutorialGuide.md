# 实际游戏引导系统使用指南

## 概述

这是一个基于条件触发的实际游戏引导系统，解决了传统手动调用引导的问题。系统会根据玩家的游戏状态（等级、关卡进度、功能解锁等）自动触发相应的引导，更贴近真实游戏的需求。

## 核心特性

### 1. 自动触发机制
- **条件检查**: 系统持续监控游戏状态，当满足特定条件时自动触发引导
- **优先级管理**: 多个引导同时满足条件时，按优先级顺序执行
- **防重复触发**: 已完成的引导不会重复触发

### 2. 丰富的触发条件
- **等级触发**: 玩家达到指定等级时触发
- **关卡触发**: 完成指定关卡后触发
- **首次触发**: 首次进入游戏或特定界面时触发
- **功能解锁触发**: 新功能解锁时触发
- **时间触发**: 基于游戏时长或登录天数触发
- **复合条件触发**: 多个条件组合触发

### 3. 跨界面支持
- **场景切换**: 支持引导过程中的场景切换
- **UI等待**: 自动等待目标UI元素准备就绪
- **状态同步**: 保持引导状态在场景切换中的连续性

## 系统架构

### 核心组件

1. **TutorialTriggerSystem**: 引导触发系统核心，管理所有触发器
2. **IGameDataProvider**: 游戏数据接口，提供统一的数据访问
3. **TutorialTrigger**: 引导触发器基类，定义触发条件和引导序列
4. **GameTutorialManager**: 游戏引导管理器，整合所有组件

### 数据流程

```
游戏状态变化 → 数据提供者 → 触发系统检查 → 满足条件 → 创建引导序列 → 执行引导
```

## 使用方法

### 1. 基础设置

#### 1.1 添加游戏数据提供者
```csharp
// 在场景中添加 GameDataProvider 组件
var dataProvider = gameObject.AddComponent<GameDataProvider>();

// 或者实现自己的数据提供者
public class MyGameDataProvider : MonoBehaviour, IGameDataProvider
{
    public int GetPlayerLevel() => PlayerManager.Instance.Level;
    public int GetPlayerGold() => PlayerManager.Instance.Gold;
    // ... 实现其他接口方法
}
```

#### 1.2 添加引导管理器
```csharp
// 在场景中添加 GameTutorialManager 组件
var tutorialManager = gameObject.AddComponent<GameTutorialManager>();
```

### 2. 创建自定义引导触发器

#### 2.1 简单等级触发器
```csharp
public class MyLevelTutorial : LevelBasedTrigger
{
    public MyLevelTutorial() : base("MyTutorial", "我的引导", "等级引导描述", 5)
    {
        Priority = 800;
    }
    
    public override List<TutorialStepBase> CreateTutorialSequence()
    {
        var steps = new List<TutorialStepBase>();
        
        steps.Add(new ShowMessageStep
        {
            Message = "恭喜达到5级！",
            Duration = 3f
        });
        
        steps.Add(new ClickButtonStep
        {
            TargetPath = "Canvas/UI/Button",
            HintMessage = "点击这里"
        });
        
        return steps;
    }
}
```

#### 2.2 复合条件触发器
```csharp
public class AdvancedTutorial : CompositeConditionTrigger
{
    public AdvancedTutorial() : base("AdvancedTutorial", "高级引导", "复合条件引导")
    {
        Priority = 700;
        
        // 添加多个条件
        AddCondition(new LevelCondition(10));
        AddCondition(new StageCondition(5));
        AddCondition(new FunctionUnlockedCondition("Shop"));
    }
    
    public override List<TutorialStepBase> CreateTutorialSequence()
    {
        // 创建引导步骤
        return new List<TutorialStepBase>();
    }
}
```

#### 2.3 自定义条件触发器
```csharp
public class CustomTutorial : TutorialTrigger
{
    public CustomTutorial() : base("CustomTutorial", "自定义引导", "自定义条件引导")
    {
        Priority = 600;
    }
    
    public override bool CheckCondition(IGameDataProvider dataProvider)
    {
        // 自定义触发条件
        return dataProvider.GetPlayerGold() >= 1000 && 
               dataProvider.GetItemCount("Sword") > 0 &&
               !dataProvider.IsInBattle();
    }
    
    public override List<TutorialStepBase> CreateTutorialSequence()
    {
        // 创建引导步骤
        return new List<TutorialStepBase>();
    }
}
```

### 3. 注册引导触发器

#### 3.1 在 GameTutorialManager 中注册
```csharp
private void RegisterAllTriggers()
{
    // 注册内置引导
    RegisterTrigger(new GameTutorialTriggers.NewPlayerTutorial());
    RegisterTrigger(new GameTutorialTriggers.InventoryTutorial());
    
    // 注册自定义引导
    RegisterTrigger(new MyLevelTutorial());
    RegisterTrigger(new AdvancedTutorial());
    RegisterTrigger(new CustomTutorial());
}
```

#### 3.2 运行时动态注册
```csharp
// 获取引导管理器
var tutorialManager = FindObjectOfType<GameTutorialManager>();

// 动态注册新的引导触发器
var newTrigger = new MyCustomTrigger();
tutorialManager.RegisterTrigger(newTrigger);
```

### 4. 界面切换引导示例（使用延迟检测方案）

```csharp
public class EquipmentTutorial : CompositeConditionTrigger
{
    public EquipmentTutorial() : base("EquipmentTutorial", "装备系统引导", "装备系统的使用引导")
    {
        Priority = 750;
        AddCondition(new LevelCondition(10));
        AddCondition(new TutorialCompletedCondition("InventoryTutorial"));
    }
    
    public override List<TutorialStepBase> CreateTutorialSequence()
    {
        var steps = new List<TutorialStepBase>();
        
        // 1. 在背包界面显示消息
        steps.Add(new ShowMessageStep
        {
            Message = "现在让我们去装备界面穿戴装备！",
            Duration = 3f
        });
        
        // 2. 点击装备按钮（使用延迟检测）
        steps.Add(new DelayedDetectionTutorialStep(
            "clickEquipment",
            "点击装备按钮",
            "Canvas/MainUI/EquipmentButton",
            "点击打开装备界面",
            1.0f // 1秒初始延迟
        ));
        
        // 3. 等待装备界面加载（使用延迟检测）
        steps.Add(new DelayedDetectionTutorialStep(
            "waitEquipmentUI",
            "等待装备界面",
            "Canvas/EquipmentPanel", // 检测装备面板是否加载
            "正在加载装备界面...",
            2.0f // 2秒初始延迟
        ));
        
        // 4. 在装备界面进行操作
        steps.Add(new DelayedDetectionTutorialStep(
            "clickWeaponSlot",
            "点击武器槽位",
            "Canvas/EquipmentPanel/WeaponSlot",
            "点击武器槽位装备武器",
            0.5f // 0.5秒初始延迟
        ));
        
        return steps;
    }
}
```

## 配置参数

### 触发器优先级建议
- **新手引导**: 1000 (最高优先级)
- **核心功能引导**: 800-900
- **进阶功能引导**: 600-700
- **可选功能引导**: 400-500
- **社交功能引导**: 200-300

### 检查间隔设置
```csharp
// 在 GameTutorialManager 中设置
[SerializeField] private float checkInterval = 1f; // 每秒检查一次

// 对于性能敏感的项目，可以增加间隔
[SerializeField] private float checkInterval = 2f; // 每2秒检查一次
```

## 最佳实践

### 1. 引导设计原则
- **渐进式**: 从简单到复杂，逐步引导玩家
- **情境化**: 在合适的时机触发相关引导
- **可跳过**: 为老玩家提供跳过选项
- **非阻塞**: 不强制打断玩家的正常游戏流程

### 2. 条件设计建议
- **明确性**: 触发条件要明确，避免模糊判断
- **稳定性**: 条件检查要稳定，避免频繁变化
- **合理性**: 条件要符合游戏逻辑和玩家预期

### 3. 性能优化
- **批量检查**: 将多个简单条件合并检查
- **缓存结果**: 对复杂计算结果进行缓存
- **按需检查**: 只在必要时进行条件检查

### 4. 数据管理
- **持久化**: 引导完成状态要持久化保存
- **版本控制**: 支持引导系统的版本更新
- **数据清理**: 定期清理过期的引导数据

## 调试功能

### 1. 调试面板
系统提供了完整的调试面板，显示：
- 当前系统状态
- 游戏数据概览
- 可触发引导列表
- 引导执行历史

### 2. 快捷键操作
- **Ctrl+T**: 跳过当前引导
- **Ctrl+R**: 重置所有引导数据
- **Ctrl+L**: 模拟玩家升级

### 3. 控制台命令
```csharp
// 手动触发指定引导
tutorialManager.TriggerTutorial("TutorialId");

// 检查引导完成状态
bool completed = tutorialManager.IsTutorialCompleted("TutorialId");

// 获取系统信息
string info = tutorialManager.GetSystemInfo();
```

## 常见问题

### Q1: 引导不触发怎么办？
1. 检查触发条件是否满足
2. 确认引导是否已经完成过
3. 查看调试面板中的条件状态
4. 检查引导系统是否启用

### Q2: 如何处理引导冲突？
1. 设置合理的优先级
2. 添加互斥条件
3. 使用前置条件控制顺序

### Q3: 界面切换时引导失败怎么办？
1. 确认UI路径是否有效
2. 增加初始延迟时间
3. 调整重试间隔和次数
4. 检查超时处理逻辑

### Q4: 如何添加新的触发条件类型？
1. 继承 `TutorialTrigger` 基类
2. 实现 `CheckCondition` 方法
3. 实现 `CreateTutorialSequence` 方法
4. 在管理器中注册新触发器

## 扩展建议

### 1. 数据分析集成
- 记录引导完成率
- 分析引导跳过原因
- 优化引导流程

### 2. A/B测试支持
- 支持多版本引导
- 动态切换引导内容
- 效果对比分析

### 3. 本地化支持
- 多语言引导文本
- 文化适配调整
- 动态语言切换

### 4. 云端配置
- 远程引导配置
- 热更新支持
- 实时调整能力

## 总结

这个实际游戏引导系统提供了完整的解决方案，从自动触发到跨界面支持，从简单条件到复合逻辑，能够满足大部分游戏项目的引导需求。通过合理的配置和扩展，可以构建出符合项目特色的引导体验。

关键优势：
- ✅ 自动化触发，无需手动调用
- ✅ 丰富的触发条件类型
- ✅ 简化的界面切换处理（延迟检测方案）
- ✅ 强大的调试和测试功能
- ✅ 良好的扩展性和可维护性

这样的设计更贴近实际游戏开发的需求，能够有效提升玩家的游戏体验和开发效率。