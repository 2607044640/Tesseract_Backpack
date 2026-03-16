# Godot State Charts 集成指南

## 完成的工作

### 1. ComponentExtensions.cs - 新增 StateChart 扩展方法

已添加 5 个扩展方法，让 StateChart 使用起来像 Godot 原生 API 一样简洁：

```csharp
// 1. 获取 StateChart
var stateChart = this.GetStateChart();

// 2. 发送状态事件（最常用）
this.SendStateEvent("jump_pressed");

// 3. 连接到状态的进入/退出（核心功能）
this.ConnectToState("Movement", isActive => {
    _canMove = isActive;
    GD.Print($"Movement {(isActive ? "ENABLED" : "DISABLED")}");
});

// 4. 获取特定状态节点
var state = this.GetState("Movement");

// 5. 设置表达式属性（用于 Expression Guards）
this.SetStateProperty("player_health", 100);
```

### 2. MovementComponent.cs - 重构为状态驱动

**改动前：** 直接响应输入，无条件处理移动
**改动后：** 由状态机控制是否允许移动

```csharp
// 新增状态标志
private bool _canMove = true;

// OnEntityReady 中连接状态
parent.ConnectToState("Movement", isActive => {
    _canMove = isActive;
});

// ProcessPhysics 中检查状态
if (_canMove) {
    // 处理移动逻辑
} else {
    // 快速停止
}
```

### 3. InputToStateOperator.cs - 输入到状态的"接线员"

新建示例组件，演示如何将输入事件转换为状态机事件：

```csharp
// 输入事件 → 状态机事件
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

## 架构优势

### 传统方式（紧耦合）
```
Input → Component → 直接执行逻辑
```
问题：组件需要自己判断"什么时候能做什么"，导致大量 if-else

### StateChart 方式（解耦）
```
Input → InputComponent → InputToStateOperator → StateChart
                                                      ↓
                                                   States
                                                      ↓
                                              Components (监听状态)
```
优势：
1. 输入组件不知道状态机存在
2. 状态机不知道输入来源
3. 逻辑组件只关心"当前状态"，不关心"如何进入状态"

## 使用场景示例

### 场景 1：战斗中禁用移动

**StateChart 结构：**
```
StateChart
├── Parallel (根状态)
│   ├── Locomotion (复合状态)
│   │   ├── Idle
│   │   └── Movement ← MovementComponent 监听这个
│   └── Combat (复合状态)
│       ├── Normal
│       └── Attacking ← 攻击时自动禁用 Movement
```

**Transition 配置：**
- Attacking 进入时 → 自动触发 Locomotion 切换到 Idle
- Attacking 退出时 → 允许切换回 Movement

**结果：** MovementComponent 无需修改任何代码，自动在攻击时禁用！

### 场景 2：受伤硬直

```csharp
// 在 HealthComponent 中
private void OnTakeDamage(float damage)
{
    parent.SendStateEvent("take_damage");
    // StateChart 自动切换到 "Stunned" 状态
    // MovementComponent 监听到 Movement 状态退出，自动停止移动
}
```

### 场景 3：技能冷却

使用 Expression Guards + Delayed Transitions：

```csharp
// 设置冷却时间
parent.SetStateProperty("skill_cooldown", 5.0f);

// StateChart 中配置 Transition：
// - Event: "use_skill"
// - Guard: "skill_cooldown <= 0"
// - Delay: 使用表达式 "skill_cooldown"
```

## 场景配置示例

### 基础 StateChart 结构

```
Player3D (CharacterBody3D)
├── StateChart (script: state_chart.gd)
│   └── Root (CompoundState)
│       ├── Idle (AtomicState) [Initial State]
│       └── Movement (AtomicState)
│           ← Transition: Event="start_moving"
│           → Transition: Event="stop_moving"
├── PlayerInputComponent
├── InputToStateOperator (可选，用于集中管理输入→状态映射)
├── MovementComponent
└── AnimationControllerComponent
```

### 高级 Parallel 结构

```
StateChart
└── Root (ParallelState)
    ├── Locomotion (CompoundState)
    │   ├── Idle
    │   └── Movement
    ├── VerticalMovement (CompoundState)
    │   ├── Grounded
    │   ├── Jumping
    │   └── Falling
    └── Combat (CompoundState)
        ├── Normal
        └── Attacking
```

## 调试技巧

### 1. 使用 StateChartDebugger

在场景中添加：
```
UI Layer
└── StateChartDebugger (Control)
    - Initial node to watch: %Player3D
```

### 2. 在编辑器中调试

StateChart 节点属性：
- ✓ Track in Editor

运行游戏后，在编辑器底部查看 "State Charts" 面板

### 3. 代码中添加历史记录

```csharp
var debugger = StateChartDebugger.Of(GetNode("StateChartDebugger"));
debugger.AddHistoryEntry("Player took damage: 50");
```

## 下一步建议

1. **创建状态常量类**（类似 AnimationNames.cs）
   ```csharp
   public static class StateEvents
   {
       public const string JumpPressed = "jump_pressed";
       public const string StartMoving = "start_moving";
       public const string StopMoving = "stop_moving";
   }
   ```

2. **为 AnimationControllerComponent 添加状态支持**
   - 监听 Locomotion 状态变化
   - 自动切换动画，无需手动判断

3. **创建通用的 StateAwareComponent 基类**
   ```csharp
   public abstract class StateAwareComponent : Node
   {
       protected bool IsStateActive(string stateName) { ... }
       protected void OnStateChanged(string stateName, bool isActive) { ... }
   }
   ```

## 参考资料

- 研究笔记：`.kiro/TempFolder/StateCharts_Research.md`
- 官方文档：https://derkork.github.io/godot-statecharts/
- 示例组件：`addons/CoreComponents/Examples/InputToStateOperator.cs`
