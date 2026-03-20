# StateChart 使用示例

## 快速开始：3 步集成 StateChart

### 步骤 1：在场景中添加 StateChart 节点

```
Player3D (CharacterBody3D)
├── StateChart (添加节点: StateChart)
│   └── Root (CompoundState)
│       ├── Idle (AtomicState) [设为 Initial State]
│       └── Movement (AtomicState)
├── PlayerInputComponent
├── MovementComponent
└── ...其他组件
```

**配置 Transitions：**
- Idle → Movement: Event = "start_moving"
- Movement → Idle: Event = "stop_moving"

### 步骤 2：在组件中连接状态

```csharp
// MovementComponent.cs
public void OnEntityReady()
{
    // 订阅输入
    _inputComponent = parent.FindAndSubscribeInput(
        HandleMovementInput,
        HandleJumpInput
    );

    // 【新增】连接到状态 - 一行代码！
    parent.ConnectToState("Movement", isActive => {
        _canMove = isActive;
    });
}
```

### 步骤 3：发送状态事件

**方式 A：在输入组件中直接发送**
```csharp
// PlayerInputComponent.cs
public override void _UnhandledInput(InputEvent @event)
{
    if (Input.IsActionJustPressed("jump"))
    {
        // 直接发送状态事件
        GetParent().SendStateEvent("jump_pressed");
    }
}
```

**方式 B：使用专门的 Operator 组件（推荐）**
```csharp
// InputToStateOperator.cs
private void HandleJumpInput()
{
    parent.SendStateEvent("jump_pressed");
}

private void HandleMovementInput(Vector2 inputDir)
{
    if (inputDir.Length() > 0.1f)
        parent.SendStateEvent("start_moving");
    else
        parent.SendStateEvent("stop_moving");
}
```

## 完整示例：带跳跃的角色

### StateChart 结构

```
StateChart
└── Root (ParallelState)
    ├── Locomotion (CompoundState)
    │   ├── Idle (AtomicState) [Initial]
    │   │   → Transition: Event="start_moving" → Movement
    │   └── Movement (AtomicState)
    │       → Transition: Event="stop_moving" → Idle
    └── VerticalMovement (CompoundState)
        ├── Grounded (AtomicState) [Initial]
        │   → Transition: Event="jump_pressed" → Jumping
        ├── Jumping (AtomicState)
        │   → Transition: Automatic, Guard="is_falling" → Falling
        └── Falling (AtomicState)
            → Transition: Automatic, Guard="is_grounded" → Grounded
```

### MovementComponent 改造

```csharp
[GlobalClass]
[Component(typeof(CharacterBody3D))]
public partial class MovementComponent : Node
{
    private bool _canMove = true;
    private bool _canJump = true;

    public void OnEntityReady()
    {
        _inputComponent = parent.FindAndSubscribeInput(
            HandleMovementInput,
            HandleJumpInput
        );

        // 监听水平移动状态
        parent.ConnectToState("Movement", isActive => {
            _canMove = isActive;
        });

        // 监听垂直移动状态
        parent.ConnectToState("Grounded", isActive => {
            _canJump = isActive;
        });
    }

    private void ProcessPhysics(double delta)
    {
        // 重力和跳跃逻辑...

        // 水平移动受状态控制
        if (_canMove)
        {
            // 处理移动
        }
        else
        {
            // 快速停止
        }

        // 更新状态机的表达式属性（用于 Guards）
        parent.SetStateProperty("is_falling", velocity.Y < -0.1f);
        parent.SetStateProperty("is_grounded", parent.IsOnFloor());
    }

    private void HandleJumpInput()
    {
        if (_canJump)
        {
            parent.SendStateEvent("jump_pressed");
        }
    }
}
```

## 高级用法

### 1. Expression Guards 示例

**场景：** 技能冷却系统

```csharp
// SkillComponent.cs
public void OnEntityReady()
{
    // 初始化冷却时间
    parent.SetStateProperty("skill_cooldown", 0.0f);
}

public override void _Process(double delta)
{
    // 更新冷却时间
    float cooldown = (float)parent.GetStateChart()
        .GetExpressionProperty("skill_cooldown");
    
    if (cooldown > 0)
    {
        cooldown -= (float)delta;
        parent.SetStateProperty("skill_cooldown", Mathf.Max(0, cooldown));
    }
}

private void UseSkill()
{
    // 发送事件，StateChart 会检查 Guard
    parent.SendStateEvent("use_skill");
}
```

**StateChart 配置：**
- Transition: Event="use_skill", Guard="skill_cooldown <= 0"
- 进入 SkillActive 状态时，设置 `skill_cooldown = 5.0`

### 2. Delayed Transitions 示例

**场景：** 攻击后硬直

```
Attacking (AtomicState)
→ Transition: Event="" (Automatic), Delay=0.5 → Recovery
```

无需代码！StateChart 自动在 0.5 秒后切换到 Recovery 状态。

### 3. History States 示例

**场景：** 暂停后恢复到之前的状态

```
Root (CompoundState)
├── History (HistoryState, Deep=true, Default=Idle)
├── Idle
├── Movement
└── Paused
    → Transition: Event="resume" → History
```

```csharp
// 暂停
parent.SendStateEvent("pause");

// 恢复（自动回到暂停前的状态）
parent.SendStateEvent("resume");
```

## 调试技巧

### 实时查看状态

```csharp
// 在任意组件中
public override void _Process(double delta)
{
    var state = parent.GetState("Movement");
    if (state != null)
    {
        // 连接到 state_processing 信号查看每帧状态
        state.Connect(StateChartState.SignalName.StateProcessing, 
            Callable.From<float>(OnMovementStateProcessing));
    }
}

private void OnMovementStateProcessing(float delta)
{
    GD.Print($"Movement state active, delta: {delta}");
}
```

### 使用 StateChartDebugger

在 UI 层添加：
```
CanvasLayer
└── StateChartDebugger
    - Initial node to watch: %Player3D
```

运行游戏后可以看到：
- 当前激活的状态（绿色高亮）
- 待处理的 Delayed Transitions
- 状态切换历史
- Expression Properties 的值

## 常见模式

### 模式 1：状态控制组件启用/禁用

```csharp
public void OnEntityReady()
{
    parent.ConnectToState("Combat", isActive => {
        SetProcess(isActive);  // 只在战斗状态下处理逻辑
    });
}
```

### 模式 2：状态驱动动画

```csharp
// AnimationControllerComponent.cs
public void OnEntityReady()
{
    parent.ConnectToState("Idle", isActive => {
        if (isActive) PlayAnimation(AnimationNames.Idle);
    });
    
    parent.ConnectToState("Movement", isActive => {
        if (isActive) PlayAnimation(AnimationNames.Run);
    });
}
```

### 模式 3：状态链式反应

```csharp
// HealthComponent.cs
private void OnTakeDamage(float damage)
{
    _health -= damage;
    
    if (_health <= 0)
    {
        parent.SendStateEvent("die");
        // StateChart 自动切换到 Dead 状态
        // MovementComponent 监听到 Movement 状态退出，自动停止
        // AnimationController 监听到 Dead 状态进入，播放死亡动画
    }
    else
    {
        parent.SendStateEvent("take_damage");
        // 进入 Stunned 状态，0.3 秒后自动恢复
    }
}
```

## 与现有架构的兼容性

StateChart 完全兼容 Godot.Composition 架构：

✅ 组件仍然通过事件通信
✅ 不破坏现有的 `[Component]` 和 `[ComponentDependency]`
✅ 扩展方法让 StateChart 感觉像原生 API
✅ 可以逐步迁移，不需要一次性重构所有组件

## 总结

**核心理念：** 状态控制行为，输入触发状态

**三个关键扩展方法：**
1. `parent.ConnectToState(stateName, callback)` - 监听状态
2. `parent.SendStateEvent(eventName)` - 触发转换
3. `parent.SetStateProperty(name, value)` - 更新属性

**最佳实践：**
- 使用 Parallel States 分离关注点（移动、战斗、动画）
- 使用 Expression Guards 实现条件转换
- 使用 Delayed Transitions 实现定时器
- 使用 History States 实现状态恢复
- 创建常量类避免魔法字符串
