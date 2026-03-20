# StateChart 信号连接指南

## 架构说明

AnimationControllerComponent 现在使用**极简信号驱动架构**：

- ✅ 零硬编码路径（组件不知道 StateChart 在哪）
- ✅ 零每帧 GC 分配（动画可用性在 _Ready 中缓存）
- ✅ 零 Input 依赖（完全基于 Velocity 判断）
- ✅ 公开方法供 StateChart 信号连接

## 在 Godot 编辑器中连接信号

### 步骤 1: 打开 Player3D 场景

确保场景结构如下：

```
Player3D (CharacterBody3D)
├── StateChart
│   └── Root (ParallelState)
│       └── Movement (CompoundState)
│           ├── GroundMode (AtomicState) [Initial]
│           └── FlyMode (AtomicState)
├── AnimationControllerComponent
├── GroundMovementComponent
├── FlyMovementComponent
└── PlayerInputComponent
```

### 步骤 2: 连接 GroundMode 信号

1. 在场景树中选择 `StateChart/Root/Movement/GroundMode` 节点
2. 切换到 **Node** 面板（右侧，信号图标）
3. 找到 `state_entered()` 信号
4. 双击该信号
5. 在弹出的对话框中：
   - **Receiver Method**: 选择 `AnimationControllerComponent`
   - **Method**: 输入 `EnterGroundMode`
   - 点击 **Connect**

### 步骤 3: 连接 FlyMode 信号

1. 在场景树中选择 `StateChart/Root/Movement/FlyMode` 节点
2. 切换到 **Node** 面板
3. 找到 `state_entered()` 信号
4. 双击该信号
5. 在弹出的对话框中：
   - **Receiver Method**: 选择 `AnimationControllerComponent`
   - **Method**: 输入 `EnterFlyMode`
   - 点击 **Connect**

### 步骤 4: 验证连接

连接成功后，你会在场景树中看到：

- `GroundMode` 节点旁边有一个绿色的信号图标
- `FlyMode` 节点旁边有一个绿色的信号图标

点击这些图标可以查看已连接的信号。

## 工作原理

### 信号流程

```
1. 玩家按下 F 键
   ↓
2. PlayerInputComponent 发送事件
   parent.SendStateEvent("toggle_fly");
   ↓
3. StateChart 处理状态转换
   GroundMode → FlyMode
   ↓
4. FlyMode 发出 state_entered 信号
   ↓
5. AnimationControllerComponent.EnterFlyMode() 被调用
   _currentMode = "Fly";
   ↓
6. 下一帧 _Process() 执行
   switch (_currentMode) {
       case "Fly": targetAnim = SelectFlyAnimation(); break;
   }
   ↓
7. 根据 parent.Velocity 选择合适的动画
```

### 动画选择逻辑

**地面模式**:
```csharp
if (!parent.IsOnFloor())
    → 播放跳跃动画
else if (horizontalSpeed > SprintThreshold)  // 默认 6.0f
    → 播放冲刺动画
else if (horizontalSpeed > MoveThreshold)    // 默认 0.1f
    → 播放跑步动画
else
    → 播放待机动画
```

**飞行模式**:
```csharp
if (speed > MoveThreshold)
    → 播放跑步动画（占位符）
else
    → 播放待机动画
```

## 性能优化

### 1. 动画缓存（避免每帧查询）

```csharp
// ❌ 旧代码（每帧分配内存）
private string TryGetAnimation(params string[] animNames) {
    foreach (var name in animNames) {
        if (_animPlayer.HasAnimation(name)) return name;
    }
}

// ✅ 新代码（初始化时缓存）
private void CacheAvailableAnimations() {
    _cachedIdleAnim = FindFirstAvailableAnimation(
        AnimationNames.Idle,
        AnimationNames.IdleAlt
    );
    // 只在 _Ready() 中调用一次
}
```

### 2. 速度阈值判断（避免 Input 调用）

```csharp
// ❌ 旧代码（耦合 Input）
bool isSprinting = Input.IsActionPressed("sprint");
if (isSprinting && isMoving) {
    targetAnim = AnimationNames.Sprint;
}

// ✅ 新代码（纯粹基于物理）
if (horizontalSpeed > SprintThreshold) {
    return _cachedSprintAnim;
}
```

### 3. 简单 Switch（避免复杂 if/else）

```csharp
// ✅ 清晰的状态分发
switch (_currentMode) {
    case "Ground": targetAnim = SelectGroundAnimation(); break;
    case "Fly": targetAnim = SelectFlyAnimation(); break;
}
```

## Export 参数调整

在 Godot 编辑器中，你可以调整这些参数：

| 参数 | 默认值 | 说明 |
|------|--------|------|
| SprintThreshold | 6.0 | 超过此速度播放冲刺动画 |
| MoveThreshold | 0.1 | 超过此速度播放移动动画 |
| AnimationBlendTime | 0.2 | 动画过渡时间（秒）|

## 扩展：添加更多状态

如果未来需要添加 Attacked 或 Dead 状态：

1. 在 AnimationControllerComponent 中添加公开方法：
```csharp
public void EnterAttackedState() {
    _currentMode = "Attacked";
}

public void EnterDeadState() {
    _currentMode = "Dead";
}
```

2. 在 `SelectGroundAnimation()` 或新建方法中处理：
```csharp
case "Attacked":
    return _cachedAttackedAnim;
case "Dead":
    return _cachedDeadAnim;
```

3. 在编辑器中连接信号：
   - `AttackedState.state_entered` → `EnterAttackedState()`
   - `DeadState.state_entered` → `EnterDeadState()`

## 调试技巧

### 查看当前模式

在 AnimationControllerComponent 中添加：

```csharp
public override void _Process(double delta)
{
    GD.Print($"Mode: {_currentMode}, Anim: {_currentAnimation}");
    UpdateAnimation();
}
```

### 查看速度值

```csharp
private string SelectGroundAnimation()
{
    Vector3 velocity = parent.Velocity;
    float horizontalSpeed = new Vector2(velocity.X, velocity.Z).Length();
    GD.Print($"Speed: {horizontalSpeed:F2}, Threshold: {SprintThreshold}");
    // ...
}
```

## 总结

这个架构的优势：

1. **完全解耦**: 组件不知道 StateChart 的存在和路径
2. **高性能**: 零每帧内存分配，零每帧 HasAnimation 查询
3. **易维护**: 信号连接在编辑器中可视化，易于调试
4. **易扩展**: 添加新状态只需添加公开方法并连接信号

这就是真正的"信号驱动架构"！
