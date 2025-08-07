# 新手引导系统 - 极简版

## 概述

这是一个极简版的新手引导系统，分为两个独立的子系统：
- **侵入式引导系统** (`Intrusive`): 需要在代码中手动调用的引导系统
- **非侵入式引导系统** (`NonIntrusive`): 通过配置文件驱动的引导系统

## 系统架构

```
Tutorial/
├── Intrusive/                  # 侵入式引导系统
│   ├── TutorialManager.cs      # 核心管理器
│   ├── TutorialSequence.cs     # 引导序列
│   ├── TutorialStep.cs         # 引导步骤
│   └── README.md               # 侵入式系统说明
├── NonIntrusive/               # 非侵入式引导系统
│   ├── NonIntrusiveTutorialManager.cs  # 非侵入式管理器
│   ├── TutorialConfigData.cs   # 配置数据结构
│   └── README.md               # 非侵入式系统说明
└── README.md                   # 总体说明文档
```

## 侵入式引导系统

### 特点
- 需要在代码中手动调用
- 适合复杂的业务逻辑引导
- 灵活性高，可以精确控制引导流程
- 轻量级，无额外依赖

### 使用方法
```csharp
// 获取管理器实例
var tutorialManager = TutorialManager.Instance;

// 注册引导序列
tutorialManager.RegisterSequence(myTutorialSequence);

// 开始引导
tutorialManager.StartSequence("sequenceId");

// 停止引导
tutorialManager.StopCurrentSequence();
```

## 非侵入式引导系统

### 特点
- 通过配置文件驱动
- 零代码侵入
- 适合简单的UI引导
- 支持ScriptableObject配置

### 使用方法
1. 创建引导配置文件 (右键菜单 -> Framework/Tutorial/Tutorial Config)
2. 配置引导序列和步骤
3. 在管理器中添加配置
4. 调用 `StartTutorial("configId")` 开始引导

### 配置示例
```csharp
// 创建配置
var config = ScriptableObject.CreateInstance<TutorialConfigData>();
config.configId = "firstLogin";
config.configName = "首次登录引导";

// 添加序列
var sequence = new TutorialSequenceConfigData();
sequence.sequenceId = "loginSequence";
sequence.sequenceName = "登录流程";

// 添加步骤
var step = new TutorialStepConfigData();
step.stepId = "clickLogin";
step.stepName = "点击登录按钮";
step.stepType = TutorialStepType.Click;
step.targetPath = "Canvas/LoginPanel/LoginButton";
step.message = "点击这里登录游戏";

sequence.steps.Add(step);
config.sequences.Add(sequence);
```

## 设计原则

1. **极简化**: 移除了过度设计的功能，保留核心能力
2. **模块化**: 两个系统完全独立，可以单独使用
3. **易扩展**: 简单的架构便于后续添加复杂功能
4. **零依赖**: 不依赖外部框架，仅使用Unity基础功能

## 已移除的功能

为了实现极简化目标，以下功能已被移除：
- 复杂的动画系统
- 音频管理
- 网络同步
- 本地化支持
- 分析统计
- 对象缓存
- 复杂的保存系统
- 过度的配置选项
- 复杂的条件系统
- 视觉效果管理器

## 后续扩展建议

如果需要添加复杂功能，建议：
1. 保持当前的简单架构
2. 通过插件模式添加新功能
3. 避免破坏现有的简单性
4. 优先考虑组合而非继承

## 注意事项

- 这是一个极简版本，功能有限
- 适合快速原型开发和简单项目
- 如需复杂功能，请基于此版本进行扩展
- 保持代码简洁，避免过度设计