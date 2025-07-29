# Scripts 目录结构说明

## 重构后的目录结构

```
Assets/Scripts/
├── Framework/           # 框架核心代码 - 可复用的基础框架
│   ├── Core/           # 核心系统管理
│   │   └── GameManager.cs
│   ├── Managers/       # 功能管理器集合
│   │   ├── AudioManager.cs     # 音频管理器
│   │   ├── TimerManager.cs     # 定时器管理器
│   │   └── UIManager.cs        # UI管理器
│   ├── Events/         # 事件系统
│   │   └── EventManager.cs
│   ├── UI/            # UI框架基础
│   │   ├── UIBase.cs          # UI基类
│   │   └── UIPath.cs          # UI路径配置
│   └── Patterns/      # 设计模式实现
│       └── Singleton/ # 单例模式
│           ├── ISingleton.cs
│           ├── Singleton.cs
│           └── MonoSingleton.cs
├── Utils/             # 工具类集合 - 纯静态工具方法
│   ├── DateTimeUtil.cs        # 日期时间工具
│   ├── StringUtil.cs          # 字符串工具
│   ├── PlayerPrefsUtil.cs     # 存档工具
│   └── Tools.cs               # 通用工具
├── Extensions/        # 扩展方法
│   └── ExtensionMethod.cs     # C#扩展方法
├── Constants/         # 常量定义
│   ├── GameConst.cs           # 游戏常量
│   └── AudioConst.cs          # 音频常量
├── Helpers/          # 辅助类 - 有状态的辅助功能
│   ├── SettingHelper.cs       # 设置辅助
│   └── ScreenshotHandler.cs   # 截图处理
└── Game/             # 游戏逻辑代码 - 项目特定的业务逻辑
    └── (待添加具体游戏模块)
```

## 各目录职责说明

### Framework/ - 框架层
**职责**: 提供可复用的基础框架功能
- **Core/**: 游戏核心管理类，负责整体生命周期管理
- **Managers/**: 各种功能管理器，每个管理器负责特定领域的功能
- **Events/**: 事件系统，提供解耦的消息通信机制
- **UI/**: UI框架基础类，定义UI的基本结构和行为
- **Patterns/**: 常用设计模式的实现，如单例模式等

### Utils/ - 工具层
**职责**: 提供纯静态的工具方法
- 无状态，可以在任何地方直接调用
- 提供各种数据处理、转换、计算等功能
- 不依赖Unity生命周期

### Extensions/ - 扩展层
**职责**: 为现有类型添加扩展方法
- 增强C#内置类型的功能
- 提供更便捷的API调用方式

### Constants/ - 常量层
**职责**: 集中管理项目中的常量定义
- 便于维护和修改
- 避免魔法数字和硬编码字符串

### Helpers/ - 辅助层
**职责**: 提供有状态的辅助功能
- 通常需要实例化使用
- 可能依赖Unity组件或生命周期
- 提供复杂的业务辅助逻辑

### Game/ - 游戏逻辑层
**职责**: 项目特定的游戏业务逻辑
- 按功能模块进一步划分子目录
- 如: Player/, Enemy/, Items/, Levels/, UI/, Logic/ 等

## 重构的优势

1. **职责清晰**: 每个目录都有明确的职责定义
2. **易于维护**: 相关功能集中管理，便于查找和修改
3. **可复用性**: Framework层可以在其他项目中复用
4. **扩展性**: 新功能可以按照规则放入对应目录
5. **团队协作**: 统一的目录结构便于团队成员理解和协作

## 使用建议

1. **新增功能时**: 先判断是框架级功能还是游戏特定功能
2. **工具类**: 优先考虑放入Utils，如果需要状态管理则放入Helpers
3. **管理器**: 统一放入Framework/Managers目录
4. **常量**: 按功能模块分类放入Constants目录
5. **游戏逻辑**: 按模块划分放入Game目录的子文件夹中