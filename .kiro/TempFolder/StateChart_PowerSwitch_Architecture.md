# StateChart 电源开关架构 - 极致解耦指南

## 核心理念

**状态机 = 组件的电源开关**

- 进入状态 → 唤醒组件（SetProcess = true）
- 退出状态 → 休眠组件（SetProcess = false）
- 组件内部 → 零状态判断，纯粹执行逻辑

## 重构完成的工作

### 1. ComponentExtensions.cs - 只保留两个核心方法

```csharp
// 方法1：黑盒路由 - 发送状态事件
parent.SendStateEvent("start_minesweeper");

// 方法2：电源开关 - 绑定组件生命周期到状态
this.BindComponentToState(parent, "StateChart/Root/GameFlow/Exploration");
```

**删除的方法（不符合黑盒原则）：**
- ❌ `GetStateChart()` - 组件不应该直接访问状态机
- ❌ `ConnectToState()` - 不应该让组件监听状态并设置布尔值
- ❌ `GetState()` - 破坏封装
- ❌ `SetStateProperty()` - 组件不应该操作状态机内部

### 2. MovementComponent.cs - 极致纯粹

**删除的代码：**
```csharp
// ❌ 删除状态布尔变量
private bool _canMove = true;

// ❌ 删除状态监听
parent.ConnectToState("Movement", isActive => {
    _canMove = isActive;
});

// ❌ 删除状态判断
if (_canMove) {
    // 移动逻辑
} else {
    // 停止逻辑
}
```

**新增的代码：**
```csharp
public void OnEntityReady()
{
    // 订阅输入
    _inputComponent = parent.FindAndSubscribeInput(
        HandleMovementInput,
        HandleJumpInput
    );

    // 【唯一的状态相关代码】绑定生命周期
    this.BindComponentToState(parent, "StateChart/Root/GameFlow/Exploration");
}

// ProcessPhysics 中只有纯粹的物理计算，零状态判断！
```

### 3. PlayerInputComponent.cs - 事件触发器

```csharp
// 输入组件只管发送事件，不关心状态机如何处理
if (Input.IsActionJustPressed("interact"))
{
    GetParent().SendStateEvent("start_minesweeper");
}
```

## 架构对比

### 传统方式（啰嗦且耦合）

```csharp
// 组件内部充满状态判断
public partial class MovementComponent : Node
{
    private bool _canMove = true;
    private bool _isStunned = false;
    private bool _isDead = false;
    private bool _isInCutscene = false;

    public void OnEntityReady()
    {
        parent.ConnectToState("Movement", isActive => _canMove = isActive);
        parent.ConnectToState("Stunned", isActive => _isStunned = isActive);
        parent.ConnectToState("Dead", isActive => _isDead = isActive);
        parent.ConnectToState("Cutscene", isActive => _isInCutscene = isActive);
    }

    public override void _PhysicsProcess(double delta)
    {
        // 啰嗦的判断
        if (_isDead) return;
        if (_isInCutscene) return;
        if (_isStunned) return;
        if (!_canMove) return;

        // 真正的逻辑
        ProcessMovement(delta);
    }
}
```

### 电源开关方式（极致纯粹）

```csharp
// 组件内部零状态判断
public partial class MovementComponent : Node
{
    public void OnEntityReady()
    {
        _inputComponent = parent.FindAndSubscribeInput(
            HandleMovementInput,
            HandleJumpInput
        );

        // 一行代码搞定！
        this.BindComponentToState(parent, "StateChart/Root/GameFlow/Exploration");
    }

    public override void _PhysicsProcess(double delta)
    {
        // 纯粹的逻辑，无需任何判断
        // 因为组件默认休眠，只有状态机激活时才会执行
        ProcessMovement(delta);
    }
}
```

## StateChart 场景结构示例

```
Player3D (CharacterBody3D)
├── StateChart
│   └── Root (ParallelState)
│       ├── GameFlow (CompoundState)
│       │   ├── Exploration (AtomicState) [Initial]
│       │   │   ← MovementComponent 绑定到这里
│       │   │   ← CameraControlComponent 绑定到这里
│       │   ├── Minesweeper (AtomicState)
│       │   │   ← MinesweeperUIComponent 绑定到这里
│       │   └── MatchThree (AtomicState)
│       │       ← MatchThreeComponent 绑定到这里
│       └── CharacterState (CompoundState)
│           ├── Alive (AtomicState) [Initial]
│           └── Dead (AtomicState)
│               ← 所有组件在这个状态下都会休眠
├── PlayerInputComponent
├── MovementComponent
├── CameraControlComponent
├── MinesweeperUIComponent
└── MatchThreeComponent
```

**Transitions 配置：**
- Exploration → Minesweeper: Event="start_minesweeper"
- Minesweeper → Exploration: Event="exit_minesweeper"
- Exploration → MatchThree: Event="start_match_three"
- MatchThree → Exploration: Event="exit_match_three"

## 工作流程

### 场景1：进入扫雷模式

```
1. 玩家按下 E 键
   ↓
2. PlayerInputComponent 发送事件
   GetParent().SendStateEvent("start_minesweeper");
   ↓
3. StateChart 处理转换
   Exploration → Minesweeper
   ↓
4. 组件自动切换
   MovementComponent: 💤 休眠（SetPhysicsProcess(false)）
   CameraControlComponent: 💤 休眠
   MinesweeperUIComponent: ⚡ 唤醒（SetProcess(true)）
```

### 场景2：角色死亡

```
1. HealthComponent 检测到生命值 <= 0
   parent.SendStateEvent("die");
   ↓
2. StateChart 处理转换
   Alive → Dead
   ↓
3. 所有组件自动休眠
   MovementComponent: 💤 休眠
   CameraControlComponent: 💤 休眠
   AnimationController: 💤 休眠
   （因为它们都绑定到 Alive 状态的子状态）
```

## BindComponentToState 工作原理

```csharp
public static void BindComponentToState(this Node component, Node parentEntity, string stateNodePath)
{
    var state = StateChartState.Of(parentEntity.GetNode(stateNodePath));

    // 1. 默认休眠（关键！）
    component.SetProcess(false);
    component.SetPhysicsProcess(false);
    component.SetProcessInput(false);
    component.SetProcessUnhandledInput(false);

    // 2. 状态进入 → 通电唤醒
    state.Connect(StateChartState.SignalName.StateEntered, Callable.From(() => {
        component.SetProcess(true);
        component.SetPhysicsProcess(true);
        component.SetProcessInput(true);
        component.SetProcessUnhandledInput(true);
    }));

    // 3. 状态退出 → 断电休眠
    state.Connect(StateChartState.SignalName.StateExited, Callable.From(() => {
        component.SetProcess(false);
        component.SetPhysicsProcess(false);
        component.SetProcessInput(false);
        component.SetProcessUnhandledInput(false);
    }));
}
```

## 优势总结

### 代码层面
- ✅ 组件内部零状态判断
- ✅ 无需维护布尔状态变量
- ✅ 逻辑纯粹，易于测试
- ✅ 性能优化（休眠组件不执行）

### 架构层面
- ✅ 完美解耦：组件不知道状态机存在
- ✅ 黑盒路由：只需要 SendStateEvent
- ✅ 生命周期管理：状态机统一控制
- ✅ 易于扩展：新增状态不影响组件代码

### 维护层面
- ✅ 状态转换在 StateChart 场景中可视化
- ✅ 无需在代码中搜索状态判断
- ✅ 新增游戏模式只需添加状态和绑定
- ✅ 调试简单：StateChartDebugger 实时查看

## 最佳实践

### 1. 组件绑定原则

```csharp
// ✅ 好：绑定到具体的功能状态
this.BindComponentToState(parent, "StateChart/Root/GameFlow/Exploration");

// ❌ 坏：绑定到根状态（组件永远不会休眠）
this.BindComponentToState(parent, "StateChart/Root");
```

### 2. 状态事件命名

```csharp
// ✅ 好：动词+名词，清晰表达意图
parent.SendStateEvent("start_minesweeper");
parent.SendStateEvent("exit_match_three");
parent.SendStateEvent("player_died");

// ❌ 坏：模糊不清
parent.SendStateEvent("change");
parent.SendStateEvent("mode2");
```

### 3. 使用 Parallel States 分离关注点

```
Root (ParallelState)
├── GameMode (CompoundState)
│   ├── Exploration
│   ├── Minesweeper
│   └── MatchThree
├── CharacterState (CompoundState)
│   ├── Alive
│   └── Dead
└── UIState (CompoundState)
    ├── InGame
    └── Paused
```

这样不同维度的状态可以独立变化。

## 常见问题

### Q: 如果组件需要在多个状态下都激活怎么办？

A: 绑定到它们的共同父状态（CompoundState）

```csharp
// MovementComponent 在 Exploration 和 Combat 状态下都需要激活
// 绑定到它们的父状态 Locomotion
this.BindComponentToState(parent, "StateChart/Root/Locomotion");
```

### Q: 如果需要在状态切换时执行特殊逻辑怎么办？

A: 使用状态信号（但尽量避免）

```csharp
// 如果确实需要，可以手动连接信号
var state = StateChartState.Of(parent.GetNode("StateChart/Root/GameFlow/Minesweeper"));
state.Connect(StateChartState.SignalName.StateEntered, Callable.From(OnEnterMinesweeper));

private void OnEnterMinesweeper()
{
    // 特殊初始化逻辑
}
```

但大多数情况下，应该让组件的 `_Ready()` 或 `_Process()` 自然处理。

### Q: 如何调试状态切换？

A: 使用 StateChartDebugger

```
UI Layer
└── StateChartDebugger (Control)
    - Initial node to watch: %Player3D
```

运行游戏后可以实时看到：
- 当前激活的状态（绿色）
- 状态切换历史
- 待处理的转换

## 总结

**一句话总结：** 状态机管理组件的电源，组件只管纯粹执行逻辑。

**核心方法：**
1. `SendStateEvent(eventName)` - 触发状态转换
2. `BindComponentToState(parent, statePath)` - 绑定生命周期

**设计原则：**
- 组件内部零状态判断
- 状态机是黑盒路由器
- 生命周期由状态机统一管理

这就是 ECS + StateChart 的极致解耦！
