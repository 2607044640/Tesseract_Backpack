# 快速开始指南

## 🚀 5 分钟创建新实体

### 步骤 1：创建 Entity 脚本（10 秒）

```csharp
using Godot;
using Godot.Composition;

[Entity]
public partial class MyEntity : CharacterBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}
```

### 步骤 2：创建场景（1 分钟）

1. 在 Godot 编辑器中创建新场景
2. 根节点选择 `CharacterBody3D`
3. 附加 `MyEntity.cs` 脚本
4. 添加组件节点（见下方）

### 步骤 3：添加组件（2 分钟）

根据需要添加以下组件节点：

**基础移动：**
```
MyEntity
├── PlayerInputComponent (或 AIInputComponent)
└── MovementComponent
```

**完整角色：**
```
MyEntity
├── PlayerInputComponent
├── MovementComponent
├── CharacterRotationComponent
├── AnimationControllerComponent
└── HealthComponent
```

**第三人称玩家：**
```
MyEntity
├── PlayerInputComponent
├── MovementComponent
├── CharacterRotationComponent
├── CameraControlComponent
├── AnimationControllerComponent
└── CameraPivot
    └── SpringArm3D
        └── Camera3D
```

### 步骤 4：配置组件（2 分钟）

在 Inspector 中设置组件参数：

**MovementComponent：**
- Speed = 5.0
- JumpVelocity = 4.5
- Gravity = 9.8

**CharacterRotationComponent：**
- CharacterModelPath = "YourModel"
- RotationSpeed = 10.0

**AnimationControllerComponent：**
- CharacterModelPath = "YourModel"
- AnimationPlayerPath = "AnimationPlayer"
- AnimConfig = 你的动画配置资源

### 步骤 5：运行测试（10 秒）

按 F5 运行游戏，测试功能！

---

## 📋 组件速查表

### 输入组件

| 组件 | 用途 | 事件 |
|------|------|------|
| `PlayerInputComponent` | 玩家输入 | `OnMovementInput`, `OnJumpJustPressed` |
| `AIInputComponent` | AI 输入 | `OnMovementInput`, `OnJumpJustPressed` |

### 移动组件

| 组件 | 用途 | 依赖 |
|------|------|------|
| `MovementComponent` | 物理移动 | 输入组件 |
| `CharacterRotationComponent` | 角色朝向 | 输入组件 |

### 相机组件

| 组件 | 用途 | 依赖 |
|------|------|------|
| `CameraControlComponent` | 第三人称相机 | 无 |

### 动画组件

| 组件 | 用途 | 依赖 |
|------|------|------|
| `AnimationControllerComponent` | 动画状态机 | 无 |

### 生命值组件

| 组件 | 用途 | 依赖 |
|------|------|------|
| `HealthComponent` | 生命值系统 | 无 |
| `BreakableComponent` | 破碎效果 | HealthComponent |

### 物理组件

| 组件 | 用途 | 依赖 |
|------|------|------|
| `PushableComponent` | 可推动 | 无 |

---

## 🎯 常见实体配方

### 配方 1：玩家角色
```
Player3D (CharacterBody3D)
├── PlayerInputComponent
├── MovementComponent
├── CharacterRotationComponent
├── CameraControlComponent
├── AnimationControllerComponent
└── HealthComponent
```

### 配方 2：AI 敌人
```
Enemy (CharacterBody3D)
├── AIInputComponent
├── MovementComponent
├── CharacterRotationComponent
├── AnimationControllerComponent
└── HealthComponent
```

### 配方 3：可破坏箱子
```
Box (RigidBody3D)
├── PushableComponent
├── HealthComponent
└── BreakableComponent
```

### 配方 4：NPC（不移动）
```
NPC (CharacterBody3D)
├── AnimationControllerComponent
└── HealthComponent
```

### 配方 5：移动平台
```
MovingPlatform (AnimatableBody3D)
└── PathFollowComponent (待实现)
```

---

## 💡 组件组合技巧

### 技巧 1：共享输入接口
让 `AIInputComponent` 和 `PlayerInputComponent` 发出相同的事件，这样其他组件就可以无缝切换。

```csharp
// 两个组件都有这些事件
public event Action<Vector2> OnMovementInput;
public event Action OnJumpJustPressed;
```

### 技巧 2：组件复用
同一个组件可以用在不同类型的实体上：

- `HealthComponent` → Player, Enemy, Box
- `MovementComponent` → Player, Enemy
- `AnimationControllerComponent` → Player, Enemy, NPC

### 技巧 3：可选组件
不是所有实体都需要所有组件：

- Enemy 不需要 `CameraControlComponent`
- Box 不需要 `MovementComponent`
- NPC 不需要输入组件

### 技巧 4：组件通信
组件通过事件通信，保持解耦：

```csharp
// 组件 A 发出事件
public event Action<float> OnDamaged;

// 组件 B 订阅事件
[ComponentDependency(typeof(HealthComponent))]
public partial class DamageIndicator : Node
{
    public void OnEntityReady()
    {
        healthComponent.OnDamaged += ShowDamageEffect;
    }
}
```

---

## 🔧 调试技巧

### 技巧 1：检查组件初始化
在组件的 `_Ready()` 中添加日志：

```csharp
public override void _Ready()
{
    InitializeComponent();
    GD.Print($"{GetType().Name}: 已初始化 ✓");
}
```

### 技巧 2：检查依赖注入
在 `OnEntityReady()` 中检查依赖：

```csharp
public void OnEntityReady()
{
    if (playerInputComponent == null)
    {
        GD.PushError("依赖注入失败！");
        return;
    }
    GD.Print("依赖注入成功 ✓");
}
```

### 技巧 3：检查事件订阅
在事件处理器中添加日志：

```csharp
private void HandleMovementInput(Vector2 input)
{
    GD.Print($"收到输入: {input}");
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

### 错误 2：忘记 `InitializeComponent()`
```csharp
// ❌ 错误
public override void _Ready()
{
    // 忘记调用
}

// ✅ 正确
public override void _Ready()
{
    InitializeComponent();
}
```

### 错误 3：在 `_Ready()` 中访问依赖
```csharp
// ❌ 错误
public override void _Ready()
{
    InitializeComponent();
    otherComponent.DoSomething(); // 可能为 null
}

// ✅ 正确
public void OnEntityReady()
{
    otherComponent.DoSomething(); // 保证已初始化
}
```

### 错误 4：场景中缺少组件节点
确保场景中添加了所有依赖的组件节点！

---

## 📚 下一步

- 阅读 [README.md](README.md) 了解详细文档
- 查看 [Examples/](Examples/) 目录的示例代码
- 尝试创建自己的组件
- 参考 [Godot.Composition GitHub](https://github.com/MysteriousMilk/Godot.Composition)

---

## 🎉 开始创建吧！

现在你已经掌握了基础，可以快速创建各种实体了。记住：

1. Entity 只是容器
2. 组件做所有工作
3. 通过事件通信
4. 复用，复用，复用！

Happy Coding! 🚀
