# 引导系统示例

本目录包含了新版自动触发引导系统的各种使用示例和文档。

## 文件说明

### 示例文件
- `SimpleUsageExample.cs` - 快速入门示例，展示基本的系统集成方法
- `GameTutorialManager.cs` - 完整的游戏引导管理器示例
- `GameTutorialTriggers.cs` - 各种引导触发器的实现示例
- `GameDataProvider.cs` - 游戏数据提供者的示例实现
- `DelayedDetectionExample.cs` - 延迟检测引导示例

### 文档文件
- `RealGameTutorialGuide.md` - 详细的使用指南和最佳实践
- `DelayedDetectionVsCrossScene.md` - 延迟检测方案对比分析

## 快速开始

### 1. 简单测试
```csharp
// 在任意GameObject上添加SimpleTutorialTest脚本
// 运行场景后会自动开始一个简单的引导测试

// 快捷键：
// Space - 开始引导
// S - 跳过当前步骤
// Esc - 停止引导
```

### 2. 完整示例
```csharp
// 在Canvas上添加TutorialExample脚本
// 它会自动创建UI并演示完整的引导流程
```

## 创建自定义引导步骤

### 1. 继承TutorialStepBase
```csharp
public class CustomStep : TutorialStepBase
{
    public CustomStep(string id, string name, bool skipable = true) 
        : base(id, name, skipable)
    {
    }
    
    protected override void OnStepStart()
    {
        // 实现步骤逻辑
        // 完成时调用 CompleteStep()
    }
}
```

### 2. 使用协程
```csharp
// 在步骤中使用协程
TutorialCoroutineHelper.StartCoroutineStatic(YourCoroutine());
```

## 创建引导序列

```csharp
// 1. 创建序列
var sequence = new TutorialSequence("my_tutorial", "我的引导");

// 2. 添加步骤
sequence.AddStep(new ShowMessageStep("step1", "欢迎", "欢迎消息", 3f));
sequence.AddStep(new ClickButtonStep("step2", "点击", "MyButton", "请点击按钮"));
sequence.AddStep(new WaitTimeStep("step3", "等待", 2f));

// 3. 开始引导
TutorialManager.Instance.StartSequence(sequence);
```

## 事件监听

```csharp
var manager = TutorialManager.Instance;

// 序列事件
manager.OnSequenceStart += (sequenceId) => Debug.Log($"序列开始: {sequenceId}");
manager.OnSequenceComplete += (sequenceId) => Debug.Log($"序列完成: {sequenceId}");

// 步骤事件
manager.OnStepStart += (stepId) => Debug.Log($"步骤开始: {stepId}");
manager.OnStepComplete += (stepId) => Debug.Log($"步骤完成: {stepId}");
```

## 控制方法

```csharp
var manager = TutorialManager.Instance;

// 开始序列
manager.StartSequence(sequence);

// 停止当前序列
manager.StopCurrentSequence();

// 跳过当前步骤
manager.SkipCurrentStep();

// 检查状态
bool isRunning = manager.IsSequenceRunning;
```

## 内置步骤类型

### ShowMessageStep
显示消息对话框，支持自动关闭或手动确认。

```csharp
// 自动关闭（3秒）
new ShowMessageStep("id", "name", "消息内容", 3f);

// 手动确认
new ShowMessageStep("id", "name", "消息内容", 0f);
```

### ClickButtonStep
引导用户点击指定按钮，包含高亮效果。

```csharp
// 通过按钮名称查找
new ClickButtonStep("id", "name", "ButtonName", "提示文本");

// 通过路径查找
new ClickButtonStep("id", "name", "Canvas/Panel/Button", "提示文本");
```

### WaitTimeStep
等待指定时间后自动完成。

```csharp
// 等待2秒
new WaitTimeStep("id", "name", 2f);
```

## 注意事项

1. **协程使用**：步骤类不是MonoBehaviour，使用TutorialCoroutineHelper来启动协程
2. **UI查找**：ClickButtonStep会尝试多种方式查找目标按钮
3. **事件清理**：记得在适当时机移除事件监听，避免内存泄漏
4. **单例模式**：TutorialManager使用单例模式，通过Instance访问
5. **调试支持**：所有示例都包含详细的调试日志和快捷键支持

## 扩展建议

1. **自定义步骤**：根据游戏需求创建特定的引导步骤
2. **动画效果**：集成DOTween等动画库增强视觉效果
3. **音效支持**：在步骤中添加音效播放
4. **本地化**：支持多语言消息显示
5. **数据持久化**：保存引导进度到本地或云端