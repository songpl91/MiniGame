# EnhanceUI 增强型UI框架

## 概述

EnhanceUI是基于Unity开发的增强型UI框架，在传统UI框架的基础上提供了更强大的功能和更好的扩展性。

## 主要特性

### 🎯 核心功能
- **层级管理**: 基于Canvas渲染顺序的UI层级管理
- **加载队列**: 支持同步/异步加载，并发控制和优先级管理
- **实例管理**: 多开策略支持，对象池管理
- **配置驱动**: 通过ScriptableObject配置UI行为

### 🚀 增强功能
- **动画支持**: 内置淡入淡出、缩放、滑动等动画效果
- **事件系统**: 完整的UI生命周期事件
- **状态管理**: UI状态跟踪和管理
- **性能优化**: 预制体缓存、对象池等优化机制

## 快速开始

### 1. 初始化框架

```csharp
// 在场景中添加EnhanceUIManager
var uiManager = EnhanceUIManager.Instance;

// 设置UI配置（可选）
uiManager.SetUIConfig(yourUIConfig);
```

### 2. 创建UI面板

继承`EnhanceUIPanel`创建你的UI面板：

```csharp
public class MyPanel : EnhanceUIPanel
{
    [SerializeField] private Button closeButton;
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        closeButton.onClick.AddListener(OnCloseButtonClick);
    }
    
    // 泛型版本，避免装箱
    protected override void OnInitialize<T>(T data)
    {
        base.OnInitialize(data);
        // 处理强类型数据
        if (data is PlayerData playerData)
        {
            // 处理玩家数据
        }
    }
    
    private void OnCloseButtonClick()
    {
        EnhanceUIManager.Instance.CloseUI(InstanceId);
    }
}
```

### 3. 配置UI

创建UI配置数据：

```csharp
var configData = new UIConfigData
{
    UIName = "MyPanel",
    PrefabPath = "UI/Panels/MyPanel",
    LayerType = UILayerType.Normal,
    OpenStrategy = UIOpenStrategy.Single,
    AnimationType = UIAnimationType.Fade,
    LoadMode = UILoadMode.Async
};
```

### 4. 打开UI

#### 传统方式（可能产生装箱）
```csharp
// 同步打开 - 值类型会装箱
var panel = EnhanceUIManager.Instance.OpenUI("MyPanel", 123);

// 异步打开 - 值类型会装箱
EnhanceUIManager.Instance.OpenUIAsync("MyPanel", 3.14f, (panel) =>
{
    if (panel != null)
    {
        Debug.Log("UI打开成功");
    }
});
```

#### 泛型方式（推荐，避免装箱）
```csharp
// 同步打开 - 无装箱
var panel1 = EnhanceUIManager.Instance.OpenUI<int>("MyPanel", 123);
var panel2 = EnhanceUIManager.Instance.OpenUI<float>("MyPanel", 3.14f);
var panel3 = EnhanceUIManager.Instance.OpenUI<PlayerData>("MyPanel", playerData);

// 异步打开 - 无装箱
EnhanceUIManager.Instance.OpenUIAsync<int>("MyPanel", 123, (panel) =>
{
    if (panel != null)
    {
        Debug.Log("UI打开成功");
    }
});

// 无参数打开
var panel = EnhanceUIManager.Instance.OpenUI("MyPanel");
```

#### 带选项的打开
```csharp
var options = new UILoadOptions
{
    LoadMode = UILoadMode.Async,
    Priority = 1,
    CustomLayer = UILayerType.Popup
};
EnhanceUIManager.Instance.OpenUI("MyPanel", options);
```

## 架构设计

### 核心组件

1. **EnhanceUIManager**: 核心管理器，统一管理所有UI操作
2. **UILayerManager**: 层级管理器，负责UI层级和渲染顺序
3. **UILoadQueue**: 加载队列管理器，处理UI加载请求
4. **UIInstanceManager**: 实例管理器，管理UI实例和对象池
5. **EnhanceUIPanel**: UI面板基类，提供标准的UI功能

### 层级系统

| 层级 | 排序值 | 用途 |
|------|--------|------|
| Background | 0 | 背景UI |
| Bottom | 100 | 底层UI |
| Normal | 200 | 普通UI |
| Popup | 300 | 弹窗UI |
| System | 400 | 系统UI |
| Top | 500 | 顶层UI |
| Debug | 600 | 调试UI |

### 多开策略

- **Single**: 单例模式，同时只能存在一个实例
- **Multiple**: 多实例模式，可以同时存在多个实例
- **Limited**: 限制模式，限制最大实例数量
- **Stack**: 栈模式，新实例会隐藏旧实例
- **Queue**: 队列模式，新请求会排队等待

## 配置说明

### UIConfigData 配置项

```csharp
public class UIConfigData
{
    public string UIName;              // UI名称
    public string PrefabPath;          // 预制体路径
    public UILayerType LayerType;      // 层级类型
    public UIOpenStrategy OpenStrategy; // 多开策略
    public UIAnimationType AnimationType; // 动画类型
    public float AnimationDuration;    // 动画时长
    public UILoadMode LoadMode;        // 加载模式
    public bool ClickBackgroundToClose; // 点击背景关闭
    public bool PlaySound;             // 播放音效
    public bool IsModal;               // 是否模态
    public int MaxInstanceCount;       // 最大实例数（Limited策略）
}
```

### UILoadOptions 加载选项

```csharp
public class UILoadOptions
{
    public UILoadMode LoadMode;        // 加载模式
    public int Priority;               // 优先级
    public bool CanCancel;             // 是否可取消
    public bool ForceReload;           // 强制重新加载
    public bool SkipAnimation;         // 跳过动画
    public UILayerType? CustomLayer;   // 自定义层级
    public float TimeoutSeconds;       // 超时时间
}
```

## 示例代码

查看 `Examples` 文件夹中的示例代码：

- `ExampleMainMenuPanel.cs`: 主菜单面板示例
- `EnhanceUIExample.cs`: 框架使用示例
- `CreateExampleUIConfig.cs`: 配置创建示例

## 性能优化

### 泛型接口优化
EnhanceUI框架提供了泛型接口来避免值类型的装箱操作，显著提升性能：

#### 装箱问题
```csharp
// ❌ 传统方式 - 会产生装箱
int playerId = 12345;
EnhanceUIManager.Instance.OpenUI("PlayerPanel", playerId); // int装箱为object

float score = 99.5f;
EnhanceUIManager.Instance.OpenUI("ScorePanel", score); // float装箱为object
```

#### 泛型解决方案
```csharp
// ✅ 泛型方式 - 避免装箱
int playerId = 12345;
EnhanceUIManager.Instance.OpenUI<int>("PlayerPanel", playerId); // 无装箱

float score = 99.5f;
EnhanceUIManager.Instance.OpenUI<float>("ScorePanel", score); // 无装箱

// 结构体数据也不会装箱
PlayerData data = new PlayerData { id = 1, name = "Player" };
EnhanceUIManager.Instance.OpenUI<PlayerData>("PlayerPanel", data); // 无装箱
```

#### 性能对比
- **内存分配**：泛型接口减少GC压力，避免不必要的堆分配
- **执行效率**：消除装箱/拆箱开销，提升调用性能
- **类型安全**：编译时类型检查，减少运行时错误

#### 支持的泛型方法
```csharp
// UI管理器泛型方法
EnhanceUIManager.Instance.OpenUI<T>(string uiName, T data);
EnhanceUIManager.Instance.OpenUIAsync<T>(string uiName, T data, Action<EnhanceUIPanel> callback);

// UI面板泛型方法
panel.Initialize<T>(T data);
panel.Show<T>(T data, bool skipAnimation = false);

// 面板生命周期泛型回调
protected virtual void OnInitialize<T>(T data);
protected virtual void OnBeforeShow<T>(T data);
```

### 对象池

框架内置对象池支持，可以重用UI实例：

```csharp
// 启用对象池
var options = new UILoadOptions
{
    UseObjectPool = true
};
```

### 预制体缓存

预制体会自动缓存，避免重复加载：

```csharp
// 预加载预制体
EnhanceUIManager.Instance.PreloadPrefab("MyPanel");

// 清理缓存
EnhanceUIManager.Instance.ClearPrefabCache();
```

### 批量操作

支持批量UI操作以提高性能：

```csharp
// 批量关闭UI
EnhanceUIManager.Instance.CloseAllUI();

// 批量关闭指定类型
EnhanceUIManager.Instance.CloseUIByType("MyPanel");
```

## 扩展开发

### 自定义动画

继承并重写动画方法：

```csharp
public class CustomPanel : EnhanceUIPanel
{
    protected override IEnumerator PlayShowAnimation()
    {
        // 自定义显示动画
        yield return base.PlayShowAnimation();
    }
    
    protected override IEnumerator PlayHideAnimation()
    {
        // 自定义隐藏动画
        yield return base.PlayHideAnimation();
    }
}
```

### 自定义加载器

实现`IUILoader`接口：

```csharp
public class CustomUILoader : IUILoader
{
    public GameObject LoadUIPrefab(string prefabPath)
    {
        // 自定义同步加载逻辑
        return Resources.Load<GameObject>(prefabPath);
    }
    
    public void LoadUIPrefabAsync(string prefabPath, System.Action<GameObject> onComplete)
    {
        // 自定义异步加载逻辑
        StartCoroutine(LoadAsync(prefabPath, onComplete));
    }
}
```

## 调试工具

### 状态查询

```csharp
// 获取管理器状态
var status = EnhanceUIManager.Instance.GetManagerStatus();
Debug.Log($"活跃实例数: {status.InstanceStatus.ActiveInstanceCount}");

// 获取层级信息
var layerInfo = EnhanceUIManager.Instance.LayerManager.GetAllLayerInfo();
```

### 日志输出

框架提供详细的日志输出，可以通过以下方式控制：

```csharp
// 启用详细日志
EnhanceUIManager.Instance.EnableDebugLog = true;
```

## 注意事项

1. **初始化顺序**: 确保在使用前初始化UI管理器
2. **资源路径**: 预制体路径必须正确，建议使用相对路径
3. **内存管理**: 及时关闭不需要的UI，避免内存泄漏
4. **线程安全**: UI操作必须在主线程中进行
5. **配置验证**: 使用前验证UI配置的有效性

## 版本信息

- **当前版本**: 1.0.0
- **Unity版本**: 2020.3 LTS 及以上
- **依赖**: 无外部依赖

## 更新日志

### v1.0.0 (2024-01-01)
- 初始版本发布
- 实现核心功能和增强功能
- 提供完整的示例和文档

## 技术支持

如有问题或建议，请查看：
1. 示例代码和文档
2. 源码注释
3. 调试日志输出

---

*EnhanceUI框架 - 让UI开发更简单、更高效*