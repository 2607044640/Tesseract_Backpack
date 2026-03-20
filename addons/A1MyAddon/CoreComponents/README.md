# CoreComponents - Godot.Composition 组件库

基于 `Godot.Composition` 框架的可复用组件库，用于快速构建 Entity（玩家、敌人、箱子等）。

## 📚 目录结构

```
CoreComponents/
├── README.md                           # 本文档
├── Examples/                           # 示例代码
│   ├── Player3D_Example.cs            # 玩家实体示例
│   ├── Enemy_Example.cs               # 敌人实体示例
│   ├── Box_Example.cs                 # 箱子实体示例
│   └── SceneTemplates/                # 场景模板
│       ├── Player3D_Template.tscn
│       ├── Enemy_Template.tscn
│       └── Box_Template.tscn
│
├── Input/                              # 输入组件
│   ├── PlayerInputComponent.cs        # 玩家输入（WASD + 鼠标）
│   └── AIInputComponent.cs            # AI 输入（待实现）
│
├── Movement/                           # 移动组件
│   ├── MovementComponent.cs           # 基础移动（重力 + 跳跃）
│   └── CharacterRotationComponent.cs  # 角色朝向控制
│
├── Camera/                             # 相机组件
│   └── CameraControlComponent.cs      # 第三人称相机控制
│
├── Animation/                          # 动画组件
│   └── AnimationControllerComponent.cs # 动画状态机
│
└── Physics/                            # 物理组件（待扩展）
    ├── HealthComponent.cs             # 生命值（待实现）
    ├── DamageComponent.cs             # 伤害（待实现）
    └── PushableComponent.cs           # 可推动（待实现）
```

---

## 🎯 核心概念

### 1. Entity（实体）
实体是纯粹的容器，不包含任何逻辑。

**规则：**
- 必须是 `partial class`
- 添加 `[Entity]` 标签
- 在 `_Ready()` 中调用 `InitializeEntity()`

**示例：**
```csharp
using Godot;
using Godot.Composition;

[Entity]
public partial class Player3D : CharacterBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}
```

### 2. Component（组件）
组件是单一职责的逻辑块，通过依赖注入获取其他组件。

**规则：**
- 必须是 `partial class`
- 添加 `[Component(typeof(父节点类型))]` 标签
- 在 `_Ready()` 中调用 `InitializeComponent()`
- 使用 `parent` 变量访问父节点
- 使用 `[ComponentDependency]` 注入其他组件
- 在 `OnEntityReady()` 中订阅事件

**示例：**
```csharp
using Godot;
using Godot.Composition;

[Component(typeof(CharacterBody3D))]
[ComponentDependency(typeof(PlayerInputComponent))]
public partial class MovementComponent : Node
{
    public override void _Ready()
    {
        InitializeComponent();
    }
    
    public void OnEntityReady()
    {
        // playerInputComponent 是自动生成的变量
        playerInputComponent.OnMovementInput += HandleMovementInput;
    }
    
    private void HandleMovementInput(Vector2 inputDir)
    {
        // 使用 parent 访问 CharacterBody3D
        parent.Velocity = ...;
    }
}
```

---

## 🚀 快速开始

### 创建新的 Player 实体

#### 1. 创建 Entity 脚本
```csharp
// Scripts/Player3D.cs
using Godot;
using Godot.Composition;

[Entity]
public partial class Player3D : CharacterBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}
```

#### 2. 在场景中添加组件节点
```
Player3D (CharacterBody3D)
├── PlayerInputComponent (Node)
├── MovementComponent (Node)
├── CharacterRotationComponent (Node)
├── CameraControlComponent (Node)
└── AnimationControllerComponent (Node)
```

#### 3. 完成！
所有组件会自动连接，无需手动 `GetNode` 或订阅事件。

---

## 📦 可复用组件清单

### 输入组件

#### PlayerInputComponent
**用途：** 读取玩家输入（WASD + 空格 + 鼠标）

**事件：**
- `OnMovementInput(Vector2)` - 移动输入
- `OnJumpJustPressed()` - 跳跃按下

**适用于：** Player

**示例：**
```csharp
[Component(typeof(CharacterBody3D))]
public partial class PlayerInputComponent : Node
{
    public event Action<Vector2> OnMovementInput;
    public event Action OnJumpJustPressed;
    
    // 自动读取输入并发出事件
}
```

---

### 移动组件

#### MovementComponent
**用途：** 处理物理移动、重力、跳跃

**依赖：** PlayerInputComponent（或其他输入组件）

**参数：**
- `Speed` - 移动速度
- `JumpVelocity` - 跳跃初速度
- `Gravity` - 重力加速度

**适用于：** Player, Enemy（配合 AIInputComponent）

**示例：**
```csharp
[Component(typeof(CharacterBody3D))]
[ComponentDependency(typeof(PlayerInputComponent))]
public partial class MovementComponent : Node
{
    [Export] public float Speed { get; set; } = 5.0f;
    [Export] public float JumpVelocity { get; set; } = 4.5f;
    
    public void OnEntityReady()
    {
        playerInputComponent.OnMovementInput += HandleMovementInput;
    }
}
```

#### CharacterRotationComponent
**用途：** 让角色模型面向移动方向

**依赖：** PlayerInputComponent, Camera3D

**参数：**
- `CharacterModelPath` - 角色模型节点路径
- `RotationSpeed` - 旋转平滑速度

**适用于：** Player, Enemy

---

### 相机组件

#### CameraControlComponent
**用途：** 第三人称相机控制（鼠标旋转）

**参数：**
- `MouseSensitivity` - 鼠标灵敏度
- `MinPitch` / `MaxPitch` - 上下视角限制

**适用于：** Player

---

### 动画组件

#### AnimationControllerComponent
**用途：** 根据速度自动切换动画

**参数：**
- `CharacterModelPath` - 角色模型路径
- `AnimationPlayerPath` - AnimationPlayer 路径
- `AnimConfig` - 动画配置资源

**适用于：** Player, Enemy

---

## 🎨 实体示例

### 示例 1：Player3D（完整功能）

**Entity 脚本：**
```csharp
using Godot;
using Godot.Composition;

[Entity]
public partial class Player3D : CharacterBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}
```

**场景结构：**
```
Player3D (CharacterBody3D)
├── PlayerInputComponent
├── MovementComponent
├── CharacterRotationComponent
├── CameraControlComponent
├── AnimationControllerComponent
├── CollisionShape3D
├── CharacterModel
└── CameraPivot
    └── SpringArm3D
        └── Camera3D
```

**功能：**
- ✅ WASD 移动
- ✅ 空格跳跃
- ✅ 鼠标控制相机
- ✅ 角色自动转向移动方向
- ✅ 动画自动切换

---

### 示例 2：Enemy（AI 控制）

**Entity 脚本：**
```csharp
using Godot;
using Godot.Composition;

[Entity]
public partial class Enemy : CharacterBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}
```

**场景结构：**
```
Enemy (CharacterBody3D)
├── AIInputComponent              # 替换 PlayerInputComponent
├── MovementComponent             # 复用
├── CharacterRotationComponent    # 复用
├── AnimationControllerComponent  # 复用
├── HealthComponent               # 新增
└── CollisionShape3D
```

**需要实现的组件：**
- `AIInputComponent` - 发出 `OnMovementInput` 事件（AI 决策）
- `HealthComponent` - 处理生命值

**复用的组件：**
- `MovementComponent` - 无需修改
- `CharacterRotationComponent` - 无需修改
- `AnimationControllerComponent` - 无需修改

---

### 示例 3：Box（可推动物体）

**Entity 脚本：**
```csharp
using Godot;
using Godot.Composition;

[Entity]
public partial class Box : RigidBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}
```

**场景结构：**
```
Box (RigidBody3D)
├── PushableComponent    # 新增：响应推力
├── HealthComponent      # 可选：可破坏
└── CollisionShape3D
```

**需要实现的组件：**
- `PushableComponent` - 响应玩家推动

---

## 🛠️ 创建新组件的步骤

### 1. 确定组件职责
组件应该只做一件事。例如：
- ❌ `PlayerController` - 太宽泛
- ✅ `MovementComponent` - 只处理移动
- ✅ `HealthComponent` - 只处理生命值

### 2. 创建组件脚本
```csharp
using Godot;
using Godot.Composition;

[GlobalClass]  // 可选：让组件在编辑器中全局可见
[Component(typeof(CharacterBody3D))]  // 指定父节点类型
[ComponentDependency(typeof(OtherComponent))]  // 可选：声明依赖
public partial class MyComponent : Node
{
    #region Export Properties
    
    [Export] public float SomeValue { get; set; } = 1.0f;
    
    #endregion
    
    #region Godot Lifecycle
    
    public override void _Ready()
    {
        InitializeComponent();
        // 初始化逻辑
    }
    
    public void OnEntityReady()
    {
        // 订阅其他组件的事件
        // otherComponent.OnSomeEvent += HandleEvent;
    }
    
    #endregion
}
```

### 3. 在场景中添加组件节点
在 Entity 下添加一个 Node 节点，附加组件脚本。

### 4. 完成！
Godot.Composition 会自动处理依赖注入和初始化。

---

## 📋 组件通信模式

### 模式 1：事件驱动（推荐）
组件 A 发出事件，组件 B 订阅事件。

```csharp
// 组件 A：发出事件
public partial class InputComponent : Node
{
    public event Action<Vector2> OnMovementInput;
    
    public override void _Process(double delta)
    {
        Vector2 input = Input.GetVector(...);
        OnMovementInput?.Invoke(input);
    }
}

// 组件 B：订阅事件
[ComponentDependency(typeof(InputComponent))]
public partial class MovementComponent : Node
{
    public void OnEntityReady()
    {
        inputComponent.OnMovementInput += HandleMovementInput;
    }
    
    private void HandleMovementInput(Vector2 input)
    {
        // 处理输入
    }
}
```

### 模式 2：直接调用（谨慎使用）
组件 B 直接调用组件 A 的方法。

```csharp
[ComponentDependency(typeof(HealthComponent))]
public partial class DamageComponent : Node
{
    public void ApplyDamage(float amount)
    {
        healthComponent.TakeDamage(amount);
    }
}
```

### 模式 3：通过 Parent 共享状态
多个组件读取 Parent 的状态。

```csharp
public partial class AnimationComponent : Node
{
    public override void _Process(double delta)
    {
        // 读取 parent 的速度
        float speed = parent.Velocity.Length();
        // 根据速度切换动画
    }
}
```

---

## ⚠️ 常见错误

### 错误 1：忘记 `partial class`
```csharp
// ❌ 错误
public class MyComponent : Node

// ✅ 正确
public partial class MyComponent : Node
```

### 错误 2：在 `_Ready()` 中订阅事件
```csharp
// ❌ 错误：依赖可能还未初始化
public override void _Ready()
{
    InitializeComponent();
    otherComponent.OnEvent += Handler;  // 可能为 null
}

// ✅ 正确：在 OnEntityReady() 中订阅
public void OnEntityReady()
{
    otherComponent.OnEvent += Handler;  // 保证已初始化
}
```

### 错误 3：手动 `GetNode` 获取组件
```csharp
// ❌ 错误
private OtherComponent _other;
public override void _Ready()
{
    _other = GetNode<OtherComponent>("../OtherComponent");
}

// ✅ 正确：使用依赖注入
[ComponentDependency(typeof(OtherComponent))]
public partial class MyComponent : Node
{
    public void OnEntityReady()
    {
        // otherComponent 自动可用
    }
}
```

---

## 🎓 最佳实践

### 1. 保持组件单一职责
每个组件只做一件事，易于理解和测试。

### 2. 优先使用事件通信
避免组件之间的紧耦合。

### 3. 使用 `[Export]` 暴露参数
让组件可以在编辑器中配置。

### 4. 添加 `[GlobalClass]` 标签
让组件在整个项目中可见。

### 5. 编写清晰的注释
说明组件的职责、依赖和事件。

---

## 📚 参考资源

- **Godot.Composition GitHub**: https://github.com/MysteriousMilk/Godot.Composition
- **本项目示例**: `Examples/` 目录
- **迁移指南**: `.kiro/TempFolder/GodotComposition_Migration_Guide.md`

---

## 🤝 贡献

欢迎添加新的可复用组件到这个库！

**添加新组件的步骤：**
1. 在对应的子目录中创建组件脚本
2. 添加 `[GlobalClass]` 标签
3. 在 `Examples/` 中添加使用示例
4. 更新本 README

---

## 📝 更新日志

### v1.0.0 (2026-03-08)
- ✅ 初始版本
- ✅ Player3D 完整实现
- ✅ 5 个核心组件
- ✅ 示例和文档
