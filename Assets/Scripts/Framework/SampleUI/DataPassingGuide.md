# SampleUI 数据传递机制说明

## 问题背景

在原始设计中，`OnBeforeShow(data)` 方法用于在面板显示前传递数据，但存在以下问题：

1. **设计不一致**：`Show(object data)` 接收数据，但只在 `OnBeforeShow(data)` 中传递
2. **子类无法访问**：子类通常重写 `OnShow()` 而不是 `OnBeforeShow(data)`
3. **数据丢失**：传递的数据无法在子类的 `OnShow()` 方法中使用

## 修复方案

### 1. 在 SampleUIBase.cs 中简化 OnShow 方法设计

采用**单一方法**设计，使用可选参数来简化架构：

```csharp
/// <summary>
/// 面板显示时调用（子类重写）
/// </summary>
/// <param name="data">传递的数据，可为null</param>
protected virtual void OnShow(object data = null) 
{
    // 默认实现为空，子类可以重写此方法来处理显示逻辑
    // 子类可以通过检查 data 是否为 null 来判断是否有数据传入
}
```

### 2. 修改调用时序

在 `OnShowComplete()` 方法中，直接调用 `OnShow`：

```csharp
protected virtual void OnShowComplete()
{
    canvasGroup.interactable = true;
    canvasGroup.blocksRaycasts = true;
    
    // 调用子类的OnShow方法，传递数据
    OnShow(currentData);
    
    OnShowCompleted?.Invoke(this);
}
```

### 3. 添加泛型支持

在 `SampleUIManager.cs` 中添加了泛型版本的 `ShowPanel` 方法：

```csharp
/// <summary>
/// 显示面板（泛型版本）
/// </summary>
/// <typeparam name="T">面板类型</typeparam>
/// <param name="data">传递的数据</param>
/// <returns>面板实例</returns>
public T ShowPanel<T>(object data = null) where T : class, ISampleUIBase
{
    string panelId = typeof(T).Name;
    return ShowPanel(panelId, data) as T;
}
```

## 使用方法

### 1. 在子类中重写 OnShow 方法

```csharp
public class ExamplePanel : SampleUIBase
{
    /// <summary>
    /// 面板显示时调用（重写基类方法）
    /// </summary>
    /// <param name="data">传递的数据，可为null</param>
    protected override void OnShow(object data = null)
    {
        base.OnShow(data);
        
        if (data != null)
        {
            Debug.Log($"[ExamplePanel] 面板显示（带数据）: {data}");
            // 根据传入的数据更新UI
            UpdateUIWithData(data);
        }
        else
        {
            Debug.Log("[ExamplePanel] 面板显示（无数据）");
        }
        
        // 播放打开音效
        if (audioComponent != null)
        {
            audioComponent.PlayOpenSound();
        }
        
        // 播放显示动画
        if (animationComponent != null)
        {
            animationComponent.PlayAnimation(CustomAnimationType.ScaleBounce, 0.5f);
        }
    }
}
```

### 2. 显示面板时传递数据

```csharp
// 方法1：使用泛型版本
var panelData = new Dictionary<string, object>
{
    { "title", "自定义标题" },
    { "content", "动态内容" },
    { "volume", 0.8f }
};
var panel = SampleUIManager.Instance.ShowPanel<ExamplePanel>(panelData);

// 方法2：使用字符串ID版本
SampleUIManager.Instance.ShowPanel("ExamplePanel", panelData);

// 方法3：传递简单数据
SampleUIManager.Instance.ShowPanel<ExamplePanel>("Hello World!");
SampleUIManager.Instance.ShowPanel<ExamplePanel>(42);
```

### 3. 处理不同类型的数据

```csharp
private void UpdateUIWithData(object data)
{
    switch (data)
    {
        case string message:
            // 处理字符串数据
            contentText.text = $"消息: {message}";
            break;
            
        case int number:
            // 处理数字数据
            titleText.text = $"面板 #{number}";
            break;
            
        case Dictionary<string, object> dataDict:
            // 处理复合数据
            foreach (var kvp in dataDict)
            {
                ProcessDataItem(kvp.Key, kvp.Value);
            }
            break;
            
        default:
            // 通用处理
            contentText.text = data?.ToString() ?? "null";
            break;
    }
}
```

## 调用时序

优化后的调用流程：

1. `SampleUIManager.ShowPanel(panelId, data)` 被调用
2. 面板的 `Show(data)` 方法被调用（**非virtual**，框架标准流程）
3. `currentData = data` 保存数据
4. `OnBeforeShow(data)` 被调用（数据预处理）
5. `OnShow(data)` 被调用（**显示前初始化**，子类可重写）
6. 设置面板状态和播放动画
7. 动画完成后 `OnShowComplete()` 被调用
8. `OnShowComplete()` 调用 `OnShown(currentData)`（**显示完成后处理**，子类可重写）

**设计优势：**
- ✅ **语义清晰**：`OnShow`用于显示前初始化，`OnShown`用于显示完成后处理
- ✅ **职责分离**：显示前和显示后的逻辑分离，避免混淆
- ✅ **避免递归**：不存在方法间相互调用的问题
- ✅ **时序明确**：调用顺序清晰，便于理解和调试
- ✅ **良好扩展性**：框架方法非virtual，扩展点明确
- ✅ **减少维护**：标准流程固化，减少错误使用

## 优势

1. **向后兼容**：现有的 `OnShow()` 方法仍然有效
2. **数据可访问**：子类可以通过 `OnShow(object data)` 接收数据
3. **类型安全**：支持泛型调用，编译时类型检查
4. **灵活性**：支持任意类型的数据传递
5. **一致性**：数据传递机制统一且清晰

## 最佳实践

1. **优先使用带数据的 OnShow 方法**：在新项目中推荐重写 `OnShow(object data)`
2. **数据类型检查**：使用 pattern matching 或 is/as 操作符进行类型检查
3. **空值处理**：始终检查 data 是否为 null
4. **复合数据**：对于复杂数据，使用 Dictionary 或自定义类
5. **性能考虑**：避免在 OnShow 中进行耗时操作

这样的设计既解决了原有的数据传递问题，又保持了框架的易用性和扩展性。