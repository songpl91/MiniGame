# 非侵入式新手引导系统

## 概述

非侵入式新手引导系统是一个完全基于配置驱动的引导解决方案，无需在现有代码中添加任何引导相关的逻辑，通过配置文件和自动检测机制实现引导功能。

## 核心特性

### 🚀 零侵入性
- 无需修改现有UI代码
- 无需手动绑定UI元素
- 无需添加引导事件处理
- 完全通过配置文件驱动

### 🎯 自动化检测
- 自动发现UI元素
- 智能路径匹配
- 动态元素绑定
- 实时状态监控

### 📝 配置驱动
- JSON格式配置文件
- 可视化编辑器
- 热重载支持
- 版本控制友好

### 🎨 丰富的视觉效果
- 高亮、遮罩、箭头
- 动画效果支持
- 自定义样式
- 响应式布局

### 🔧 强大的扩展性
- 插件化架构
- 自定义步骤类型
- 事件系统
- 条件评估器

## 系统架构

```
NonIntrusiveTutorialManager (核心管理器)
├── TutorialAutoDetector (自动检测器)
├── TutorialConfigLoader (配置加载器)
├── TutorialStepExecutor (步骤执行器)
├── TutorialConditionEvaluator (条件评估器)
├── TutorialVisualEffectManager (视觉效果管理器)
├── TutorialProgressManager (进度管理器)
├── TutorialEventSystem (事件系统)
└── TutorialConfigEditor (配置编辑器)
```

## 快速开始

### 1. 初始化系统

```csharp
// 在场景中添加NonIntrusiveTutorialManager组件
var tutorialManager = gameObject.AddComponent<NonIntrusiveTutorialManager>();

// 或者通过代码初始化
NonIntrusiveTutorialManager.Instance.Initialize();
```

### 2. 创建配置文件

使用Unity编辑器菜单：`Tools > Tutorial > Config Editor`

或者手动创建JSON配置文件：

```json
{
  "version": "1.0",
  "globalSettings": {
    "enableTutorial": true,
    "autoStart": true,
    "allowSkip": true
  },
  "triggers": [
    {
      "triggerType": "SceneLoad",
      "sceneName": "MainMenu",
      "sequenceId": "welcome_tutorial",
      "isEnabled": true
    }
  ],
  "sequences": [
    {
      "id": "welcome_tutorial",
      "name": "欢迎引导",
      "steps": [
        {
          "id": "welcome_message",
          "stepType": "Message",
          "messageConfig": {
            "title": "欢迎",
            "content": "欢迎来到游戏！",
            "position": "Center"
          }
        }
      ]
    }
  ]
}
```

### 3. 配置文件路径

将配置文件放置在以下路径之一：
- `StreamingAssets/Tutorial/`
- `Resources/Tutorial/`
- `PersistentDataPath/Tutorial/`

## 配置文件详解

### 全局设置 (GlobalSettings)

```json
{
  "globalSettings": {
    "enableTutorial": true,          // 是否启用引导
    "autoStart": true,               // 是否自动开始
    "allowSkip": true,               // 是否允许跳过
    "showProgress": true,            // 是否显示进度
    "defaultStepDelay": 0.5,         // 默认步骤延迟
    "defaultStepTimeout": 30.0,      // 默认步骤超时
    "animationSpeed": 1.0,           // 动画速度
    "uiSettings": {
      "highlightColor": "#FFFF00",   // 高亮颜色
      "maskColor": "#000000AA",      // 遮罩颜色
      "fontSize": 16                 // 字体大小
    }
  }
}
```

### 触发器 (Triggers)

```json
{
  "triggers": [
    {
      "triggerType": "SceneLoad",    // 触发类型
      "sceneName": "MainMenu",       // 场景名称
      "sequenceId": "main_tutorial", // 序列ID
      "isEnabled": true,             // 是否启用
      "priority": 0                  // 优先级
    },
    {
      "triggerType": "GameEvent",    // 游戏事件触发
      "eventName": "PlayerLevelUp",  // 事件名称
      "sequenceId": "levelup_tutorial"
    },
    {
      "triggerType": "Condition",    // 条件触发
      "condition": {
        "conditionType": "Variable",
        "variableName": "tutorial_completed",
        "targetValue": "false",
        "comparisonType": "Equals"
      },
      "sequenceId": "first_time_tutorial"
    }
  ]
}
```

### 引导序列 (Sequences)

```json
{
  "sequences": [
    {
      "id": "main_tutorial",
      "name": "主要引导",
      "description": "游戏主要功能引导",
      "isEnabled": true,
      "priority": 0,
      "steps": [
        // 步骤配置...
      ]
    }
  ]
}
```

### 引导步骤 (Steps)

#### 消息步骤
```json
{
  "id": "welcome_message",
  "stepType": "Message",
  "delay": 0.0,
  "timeout": 10.0,
  "messageConfig": {
    "title": "欢迎",
    "content": "欢迎来到游戏世界！",
    "position": "Center",
    "showCloseButton": true
  }
}
```

#### 点击步骤
```json
{
  "id": "click_start_button",
  "stepType": "Click",
  "targetConfig": {
    "targetType": "UIElement",
    "targetName": "StartButton",
    "targetPath": "Canvas/MainMenu/StartButton"
  },
  "visualConfig": {
    "effectType": "Highlight",
    "color": "#FFFF00",
    "duration": 2.0
  }
}
```

#### 高亮步骤
```json
{
  "id": "highlight_menu",
  "stepType": "Highlight",
  "targetConfig": {
    "targetType": "UIElement",
    "targetName": "MenuPanel"
  },
  "visualConfig": {
    "effectType": "Glow",
    "color": "#00FF00",
    "duration": 3.0
  }
}
```

#### 等待步骤
```json
{
  "id": "wait_for_animation",
  "stepType": "Wait",
  "waitConfig": {
    "waitType": "Time",
    "duration": 2.0
  }
}
```

#### 动画步骤
```json
{
  "id": "arrow_animation",
  "stepType": "Animation",
  "targetConfig": {
    "targetType": "UIElement",
    "targetName": "InventoryButton"
  },
  "visualConfig": {
    "effectType": "Arrow",
    "direction": "Down",
    "duration": 1.5
  }
}
```

#### 音效步骤
```json
{
  "id": "play_notification",
  "stepType": "Audio",
  "audioConfig": {
    "audioType": "SFX",
    "clipName": "notification",
    "volume": 0.8
  }
}
```

## 目标配置

### UI元素目标
```json
{
  "targetType": "UIElement",
  "targetName": "StartButton",           // 元素名称
  "targetPath": "Canvas/Menu/StartButton", // 完整路径
  "targetTag": "Button",                 // 标签
  "targetComponent": "Button"            // 组件类型
}
```

### 游戏对象目标
```json
{
  "targetType": "GameObject",
  "targetName": "Player",
  "targetPath": "GameWorld/Player",
  "targetTag": "Player"
}
```

### 屏幕位置目标
```json
{
  "targetType": "ScreenPosition",
  "screenPosition": {
    "x": 0.5,
    "y": 0.5
  }
}
```

### 世界位置目标
```json
{
  "targetType": "WorldPosition",
  "worldPosition": {
    "x": 10.0,
    "y": 5.0,
    "z": 0.0
  }
}
```

## 条件系统

### 变量条件
```json
{
  "conditionType": "Variable",
  "variableName": "player_level",
  "targetValue": "5",
  "comparisonType": "GreaterThan"
}
```

### 对象条件
```json
{
  "conditionType": "Object",
  "objectName": "InventoryPanel",
  "objectState": "Active"
}
```

### 场景条件
```json
{
  "conditionType": "Scene",
  "sceneName": "BattleScene",
  "sceneState": "Loaded"
}
```

### 时间条件
```json
{
  "conditionType": "Time",
  "timeType": "GameTime",
  "targetValue": "300",
  "comparisonType": "GreaterThan"
}
```

### PlayerPrefs条件
```json
{
  "conditionType": "PlayerPrefs",
  "key": "tutorial_completed",
  "targetValue": "true",
  "comparisonType": "Equals"
}
```

### 自定义条件
```json
{
  "conditionType": "Custom",
  "customConditionId": "has_enough_coins",
  "parameters": {
    "required_amount": "100"
  }
}
```

## 视觉效果

### 高亮效果
- **Highlight**: 基础高亮
- **Glow**: 发光效果
- **Pulse**: 脉冲效果
- **Outline**: 轮廓高亮

### 指示效果
- **Arrow**: 箭头指示
- **Circle**: 圆圈标记
- **Hand**: 手势指示
- **Pointer**: 指针效果

### 动画效果
- **FadeIn/FadeOut**: 淡入淡出
- **ScaleUp/ScaleDown**: 缩放
- **SlideIn/SlideOut**: 滑动
- **Bounce**: 弹跳
- **Shake**: 震动

### 遮罩效果
- **FullMask**: 全屏遮罩
- **SpotlightMask**: 聚光灯遮罩
- **RectMask**: 矩形遮罩
- **CircleMask**: 圆形遮罩

## 事件系统

### 监听引导事件

```csharp
// 监听序列开始
TutorialEventSystem.Instance.AddListener(TutorialEventSystem.Events.SEQUENCE_STARTED, OnSequenceStarted);

// 监听步骤完成
TutorialEventSystem.Instance.AddListener(TutorialEventSystem.Events.STEP_COMPLETED, OnStepCompleted);

// 监听UI交互
TutorialEventSystem.Instance.AddListener(TutorialEventSystem.Events.UI_CLICKED, OnUIClicked);
```

### 触发自定义事件

```csharp
// 触发游戏事件
TutorialEventSystem.Instance.TriggerEvent("PlayerLevelUp");

// 触发带数据的事件
var eventData = new TutorialUIEventData("ui_interaction", "InventoryButton", "Click", Vector3.zero);
TutorialEventSystem.Instance.TriggerEvent(eventData);
```

## 进度管理

### 检查进度

```csharp
var progressManager = FindObjectOfType<TutorialProgressManager>();

// 检查序列是否完成
bool isCompleted = progressManager.IsSequenceCompleted("main_tutorial");

// 检查步骤是否完成
bool stepCompleted = progressManager.IsStepCompleted("main_tutorial", "welcome_message");

// 获取序列进度
var progress = progressManager.GetSequenceProgress("main_tutorial");
```

### 设置全局变量

```csharp
// 设置变量
progressManager.SetGlobalVariable("player_level", 5);
progressManager.SetGlobalVariable("has_sword", true);

// 获取变量
int level = progressManager.GetGlobalVariable<int>("player_level", 1);
bool hasSword = progressManager.GetGlobalVariable<bool>("has_sword", false);
```

### 重置进度

```csharp
// 重置所有进度
progressManager.ResetProgress();

// 重置特定序列
progressManager.ResetSequenceProgress("main_tutorial");
```

## 自定义扩展

### 自定义步骤类型

```csharp
public class CustomTutorialStep : ITutorialStepExecutor
{
    public async Task<bool> ExecuteStep(TutorialStepConfigData stepConfig, CancellationToken cancellationToken)
    {
        // 实现自定义步骤逻辑
        Debug.Log("执行自定义步骤");
        
        // 模拟异步操作
        await Task.Delay(1000, cancellationToken);
        
        return true; // 返回执行结果
    }
}

// 注册自定义步骤
TutorialStepExecutor.RegisterCustomStep("CustomStep", new CustomTutorialStep());
```

### 自定义条件评估器

```csharp
public class CustomConditionEvaluator : ITutorialConditionEvaluator
{
    public bool EvaluateCondition(TutorialConditionConfig condition)
    {
        // 实现自定义条件逻辑
        switch (condition.customConditionId)
        {
            case "has_enough_coins":
                int requiredAmount = int.Parse(condition.parameters["required_amount"]);
                return GameManager.Instance.PlayerCoins >= requiredAmount;
                
            default:
                return false;
        }
    }
}

// 注册自定义条件评估器
TutorialConditionEvaluator.RegisterCustomEvaluator("has_enough_coins", new CustomConditionEvaluator());
```

### 自定义视觉效果

```csharp
public class CustomVisualEffect : ITutorialVisualEffect
{
    public void ShowEffect(GameObject target, TutorialVisualConfig config)
    {
        // 实现自定义视觉效果
        Debug.Log($"显示自定义效果: {config.effectType}");
    }
    
    public void HideEffect(GameObject target)
    {
        // 隐藏效果
        Debug.Log("隐藏自定义效果");
    }
}

// 注册自定义视觉效果
TutorialVisualEffectManager.RegisterCustomEffect("CustomGlow", new CustomVisualEffect());
```

## 最佳实践

### 1. 配置文件组织
- 按功能模块分离配置文件
- 使用有意义的ID和名称
- 添加详细的描述信息
- 保持配置文件的版本控制

### 2. 目标元素命名
- 使用一致的命名规范
- 避免使用动态生成的名称
- 为重要UI元素添加固定的标识

### 3. 条件设计
- 使用简单明确的条件
- 避免过于复杂的条件组合
- 提供合理的默认值和容错机制

### 4. 视觉效果
- 保持效果的一致性
- 避免过于炫目的效果
- 考虑不同设备的性能差异

### 5. 测试和调试
- 使用调试模式验证配置
- 测试不同的触发条件
- 验证在不同设备上的表现

## 性能优化

### 1. 对象池
- 视觉效果对象复用
- UI元素缓存
- 减少频繁的创建和销毁

### 2. 异步加载
- 配置文件异步加载
- 大型资源延迟加载
- 避免阻塞主线程

### 3. 条件缓存
- 缓存条件评估结果
- 减少重复计算
- 智能的缓存失效机制

### 4. 事件优化
- 合理使用事件冷却
- 避免事件风暴
- 及时清理无用的监听器

## 故障排除

### 常见问题

1. **UI元素找不到**
   - 检查元素名称和路径
   - 确认元素在场景中存在
   - 使用调试模式查看检测结果

2. **引导不触发**
   - 检查触发条件配置
   - 验证场景名称是否正确
   - 确认引导系统已初始化

3. **步骤执行失败**
   - 检查步骤配置的完整性
   - 验证目标对象的状态
   - 查看控制台错误信息

4. **视觉效果不显示**
   - 检查Canvas设置
   - 确认UI层级关系
   - 验证材质和着色器

### 调试工具

1. **配置验证器**
   ```csharp
   TutorialConfigEditor.ValidateConfig();
   ```

2. **运行时调试**
   ```csharp
   NonIntrusiveTutorialManager.Instance.EnableDebugMode(true);
   ```

3. **事件监控**
   ```csharp
   TutorialEventSystem.Instance.PrintEventStats();
   ```

4. **进度查看**
   ```csharp
   var stats = TutorialProgressManager.Instance.GetStatistics();
   Debug.Log($"完成序列数: {stats.totalSequencesCompleted}");
   ```

## 版本历史

### v1.0.0
- 初始版本发布
- 基础引导功能
- 配置文件支持
- 可视化编辑器

### 未来计划
- 多语言支持增强
- 更多视觉效果
- 性能优化
- 移动端适配
- 云端配置同步

## 许可证

本项目采用 MIT 许可证，详情请参阅 LICENSE 文件。

## 贡献指南

欢迎提交 Issue 和 Pull Request 来改进这个项目。

## 联系方式

如有问题或建议，请通过以下方式联系：
- 邮箱: [your-email@example.com]
- GitHub: [your-github-username]