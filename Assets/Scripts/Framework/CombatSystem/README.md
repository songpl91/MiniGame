# 战斗系统框架 (Combat System Framework)

这是一个完整的逻辑与表现分离的战斗框架，专门设计用于教学演示面向对象编程的核心概念：抽象类、接口、继承、组合等设计原则。

## 🎯 框架目标

本框架的主要目标是通过一个实际可运行的战斗系统，清晰地展示以下面向对象设计原则：

- **抽象类 (Abstract Classes)**: 定义通用行为和强制子类实现特定方法
- **接口 (Interfaces)**: 定义行为契约，实现多重继承的效果
- **继承 (Inheritance)**: 代码复用和层次结构设计
- **组合 (Composition)**: 通过组合实现复杂功能
- **多态 (Polymorphism)**: 统一处理不同类型的对象
- **封装 (Encapsulation)**: 隐藏实现细节，提供清晰接口

## 🏗️ 架构设计

### 三层架构

```
┌─────────────────┐
│   表现层 (View)   │  ← Unity MonoBehaviour, UI, 特效, 音效
├─────────────────┤
│   逻辑层 (Logic)  │  ← 纯C#逻辑, 独立于Unity, 可单元测试
├─────────────────┤
│   配置层 (Config) │  ← 数据配置, ScriptableObject
└─────────────────┘
```

### 核心设计原则体现

#### 1. 单一职责原则 (SRP)
- 每个类只负责一个功能
- 逻辑层只处理游戏逻辑
- 表现层只处理视觉效果
- UI层只处理用户界面

#### 2. 开放封闭原则 (OCP)
- 对扩展开放：可以轻松添加新的角色类型、技能、AI行为
- 对修改封闭：添加新功能不需要修改现有代码

#### 3. 里氏替换原则 (LSP)
- 子类可以完全替换父类
- PlayerLogic、EnemyLogic、MonsterLogic都可以作为GameCharacterLogic使用

#### 4. 接口隔离原则 (ISP)
- 接口功能单一：IMovable、IAttacker、IAIBehavior、IControllable
- 客户端不依赖不需要的接口

#### 5. 依赖倒置原则 (DIP)
- 高层模块不依赖低层模块，都依赖抽象
- 通过接口和抽象类实现解耦

## 📁 目录结构

```
CombatSystem/
├── Core/                    # 核心逻辑层
│   ├── GameCharacterLogic.cs      # 角色逻辑抽象基类 ⭐
│   ├── CombatManager.cs           # 战斗管理器
│   ├── Vector3Logic.cs            # 逻辑层向量
│   └── CharacterType.cs           # 角色类型枚举
├── Interfaces/              # 接口定义
│   ├── ICharacterComponent.cs     # 组件基础接口 ⭐
│   ├── IMovable.cs               # 移动能力接口 ⭐
│   ├── IAttacker.cs              # 攻击能力接口 ⭐
│   ├── IControllable.cs          # 控制能力接口 ⭐
│   └── IAIBehavior.cs            # AI行为接口 ⭐
├── Components/              # 组件系统
│   ├── MovementComponent.cs       # 移动组件抽象类 ⭐
│   └── AttackComponent.cs         # 攻击组件抽象类 ⭐
├── Entities/                # 具体角色实现
│   ├── PlayerLogic.cs            # 玩家逻辑 (继承+接口) ⭐
│   ├── EnemyLogic.cs             # 敌人逻辑 (继承+接口) ⭐
│   └── MonsterLogic.cs           # 怪物逻辑 (继承+接口) ⭐
├── Presentation/            # 表现层
│   ├── GameCharacterView.cs      # 角色视图抽象基类 ⭐
│   ├── PlayerView.cs             # 玩家视图 ⭐
│   ├── EnemyView.cs              # 敌人视图 ⭐
│   ├── MonsterView.cs            # 怪物视图 ⭐
│   ├── PlayerInputController.cs   # 玩家输入控制器
│   └── CombatUIManager.cs        # 战斗UI管理器
├── CombatSystemDemo.cs      # 战斗系统演示类 🎮
├── CombatSystemManager.cs   # 战斗系统管理器 🎯
└── README.md                # 本文档
```

## 🔍 核心概念演示

### 1. 抽象类 (Abstract Class)

**GameCharacterLogic.cs** - 角色逻辑抽象基类
```csharp
public abstract class GameCharacterLogic
{
    // 通用属性和字段
    protected string _name;
    protected float _maxHealth;
    
    // 抽象方法 - 强制子类实现
    public abstract void OnDeath();
    public abstract void OnLevelUp();
    
    // 虚方法 - 提供默认实现，子类可重写
    public virtual void TakeDamage(float damage) { /* 默认实现 */ }
    
    // 具体方法 - 所有子类共享
    public void Heal(float amount) { /* 通用实现 */ }
}
```

### 2. 接口 (Interface)

**IMovable.cs** - 移动能力接口
```csharp
public interface IMovable
{
    float MoveSpeed { get; set; }
    bool IsMoving { get; }
    void MoveTo(Vector3Logic targetPosition);
    void StopMovement();
}
```

### 3. 继承 (Inheritance)

**PlayerLogic.cs** - 玩家逻辑类
```csharp
// 继承抽象类 + 实现接口
public class PlayerLogic : GameCharacterLogic, IControllable
{
    // 重写抽象方法
    public override void OnDeath() { /* 玩家特有的死亡逻辑 */ }
    
    // 重写虚方法
    public override void TakeDamage(float damage) { /* 玩家特有的受伤逻辑 */ }
    
    // 实现接口方法
    public void HandleMoveInput(Vector3Logic direction) { /* 处理移动输入 */ }
}
```

### 4. 组合 (Composition)

**角色组件系统**
```csharp
public class PlayerLogic : GameCharacterLogic
{
    // 组合：角色包含多个功能组件
    private List<ICharacterComponent> _components;
    private MovementComponent _movementComponent;
    private AttackComponent _attackComponent;
    
    public void AddComponent(ICharacterComponent component)
    {
        _components.Add(component);
    }
}
```

## 🎮 快速开始

### 1. 使用演示系统

框架提供了完整的演示系统，可以直接运行查看效果：

```csharp
// 方法1：通过管理器启动
CombatSystemManager.Instance.StartDemo();

// 方法2：直接使用演示类
var demo = FindObjectOfType<CombatSystemDemo>();
demo.StartDemo();
```

### 2. 演示控制

**键盘控制：**
- `WASD`: 玩家移动
- `鼠标左键`: 攻击
- `Q/E/R/F`: 技能
- `Space`: 交互
- `Tab`: 切换目标
- `1-4`: 快捷技能

**演示控制：**
- `F1`: 切换调试信息
- `F2`: 暂停/继续
- `F3`: 重置演示
- `F4`: 添加敌人
- `F5`: 添加怪物
- `F10`: 显示系统状态
- `F11`: 显示设计原则说明
- `F12`: 重新初始化系统

### 3. 创建自定义角色

```csharp
// 1. 创建玩家逻辑
var playerLogic = new PlayerLogic("玩家", 100f, Vector3Logic.Zero);

// 2. 添加组件 (组合模式)
var movement = new GroundMovementComponent(playerLogic, 5f);
var attack = new MeleeAttackComponent(playerLogic, 20f, 2f, 1.5f);
playerLogic.AddComponent(movement);
playerLogic.AddComponent(attack);

// 3. 创建表现层
var playerView = playerObject.GetComponent<PlayerView>();
playerView.BindCharacterLogic(playerLogic);

// 4. 添加到战斗管理器
CombatSystemManager.Instance.CombatManager.AddCharacter(playerLogic);
```

### 4. 多态处理示例

```csharp
// 统一处理不同类型的角色 (多态)
List<GameCharacterLogic> allCharacters = new List<GameCharacterLogic>
{
    new PlayerLogic("玩家", 100f, Vector3Logic.Zero),
    new EnemyLogic("敌人", 80f, Vector3Logic.Zero),
    new MonsterLogic("怪物", 150f, Vector3Logic.Zero, MonsterType.Elite)
};

// 统一调用，具体行为由子类决定
foreach (var character in allCharacters)
{
    character.Update(deltaTime); // 多态调用
}
```

## 🎯 学习要点

### 1. 抽象类 vs 接口
- **抽象类**: 提供部分实现，强制子类实现特定方法
- **接口**: 只定义契约，不提供实现

### 2. 继承 vs 组合
- **继承**: "是一个"关系 (PlayerLogic 是一个 GameCharacterLogic)
- **组合**: "有一个"关系 (PlayerLogic 有一个 MovementComponent)

### 3. 逻辑与表现分离
- **逻辑层**: 纯C#，独立于Unity，可单元测试
- **表现层**: Unity MonoBehaviour，处理视觉效果

### 4. 事件驱动架构
- 使用事件实现逻辑层与表现层的解耦
- 观察者模式的实际应用

## 🚀 扩展指南

### 添加新角色类型
1. 继承 `GameCharacterLogic`
2. 实现必要的接口
3. 重写抽象方法和虚方法
4. 创建对应的View类

### 添加新组件
1. 实现 `ICharacterComponent` 接口
2. 根据需要实现功能接口 (IMovable, IAttacker等)
3. 在角色逻辑中添加组件

### 添加新AI行为
1. 实现 `IAIBehavior` 接口
2. 定义新的AI状态
3. 实现状态转换逻辑

## 📚 设计模式应用

- **单例模式**: CombatSystemManager
- **观察者模式**: 事件系统
- **组合模式**: 角色组件系统
- **策略模式**: 不同的移动和攻击方式
- **状态模式**: AI状态机
- **工厂模式**: 角色和组件创建
- **门面模式**: CombatManager和CombatSystemManager统一接口

## 🎓 教学价值

### 面向对象核心概念
1. **抽象类**: GameCharacterLogic, MovementComponent, AttackComponent
2. **接口**: IMovable, IAttacker, IAIBehavior, IControllable
3. **继承**: PlayerLogic, EnemyLogic, MonsterLogic继承GameCharacterLogic
4. **组合**: 角色包含多个组件，管理器管理多个角色
5. **多态**: 统一处理不同类型的角色和组件
6. **封装**: 逻辑与表现分离，私有字段和公共接口

### SOLID原则实践
- **S**: 每个类职责单一
- **O**: 对扩展开放，对修改封闭
- **L**: 子类可以替换父类
- **I**: 接口功能单一
- **D**: 依赖抽象而非具体实现

### 设计模式应用
- 实际项目中常用设计模式的具体实现
- 模式之间的协作和组合使用
- 解决实际问题的模式选择

---

这个框架不仅是一个可运行的战斗系统，更是一个完整的面向对象编程教学案例。通过实际代码演示，帮助理解抽象类、接口、继承、组合等核心概念的实际应用。

**立即开始体验：**
1. 在场景中添加 `CombatSystemManager`
2. 运行游戏，系统会自动初始化
3. 按 `F1-F12` 查看各种演示功能
4. 通过GUI按钮控制演示流程
5. 查看控制台输出了解系统运行状态