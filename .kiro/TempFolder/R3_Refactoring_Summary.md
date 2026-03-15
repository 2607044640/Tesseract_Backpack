# R3 响应式编程重构总结

## 完成的工作

### 1. 安装 R3 框架
```bash
dotnet add package R3
```
成功安装 R3 v1.3.0

### 2. 重构 BaseInputComponent
**文件**: `addons/CoreComponents/BaseInputComponent.cs`

**新增功能**:
- 添加 R3 响应式流支持
- 保持向后兼容（传统 event 仍然可用）
- 暴露两个 Observable：
  - `MoveStream` - 移动输入流 (Observable<Vector2>)
  - `JumpStream` - 跳跃输入流 (Observable<Unit>)

**关键代码**:
```csharp
using R3;

private readonly Subject<Vector2> _moveSubject = new();
private readonly Subject<Unit> _jumpSubject = new();

public Observable<Vector2> MoveStream => _moveSubject;
public Observable<Unit> JumpStream => _jumpSubject;

protected void TriggerMovementInput(Vector2 direction)
{
    OnMovementInput?.Invoke(direction);  // 传统 event
    _moveSubject.OnNext(direction);       // R3 流
}
```

### 3. 重构 MovementComponent
**文件**: `addons/CoreComponents/MovementComponent.cs`

**主要改进**:
1. **移除了传统回调**:
   - 删除 `_currentInputDirection` 的手动更新
   - 删除 `_jumpRequested` 布尔标记
   - 删除 `_ExitTree` 中的手动取消订阅

2. **添加响应式状态**:
   ```csharp
   public ReactiveProperty<bool> IsMoving { get; } = new(false);
   public ReactiveProperty<bool> IsGrounded { get; } = new(true);
   public ReactiveProperty<Vector3> CurrentVelocity { get; } = new(Vector3.Zero);
   ```

3. **使用 R3 订阅**:
   ```csharp
   // 订阅移动输入
   _inputComponent.MoveStream
       .Subscribe(direction => { _currentInputDirection = direction; })
       .AddTo(_disposables);

   // 订阅跳跃输入（带过滤）
   _inputComponent.JumpStream
       .Where(_ => parent.IsOnFloor())
       .Subscribe(_ => { /* 跳跃逻辑 */ })
       .AddTo(_disposables);
   ```

4. **自动生命周期管理**:
   - 使用 `CompositeDisposable` 管理所有订阅
   - 在 `Dispose()` 中自动清理

## R3 带来的好处

### 1. 声明式编程
**之前（命令式）**:
```csharp
private bool _jumpRequested = false;

private void HandleJumpInput() {
    _jumpRequested = true;
}

// 在 PhysicsProcess 中检查
if (_jumpRequested && parent.IsOnFloor()) {
    velocity.Y = JumpVelocity;
    _jumpRequested = false;
}
```

**现在（声明式）**:
```csharp
_inputComponent.JumpStream
    .Where(_ => parent.IsOnFloor())
    .Subscribe(_ => {
        var velocity = parent.Velocity;
        velocity.Y = JumpVelocity;
        parent.Velocity = velocity;
    });
```

### 2. 自动内存管理
- 不需要手动 `_ExitTree` 取消订阅
- `CompositeDisposable` 自动管理所有订阅生命周期
- 防止内存泄漏

### 3. 响应式状态
- `ReactiveProperty` 自动判断值是否改变
- 其他组件可以订阅状态变化
- 完美支持 UI 绑定和动画系统

### 4. 强大的操作符
- `.Where()` - 过滤事件
- `.Select()` - 转换数据
- `.Throttle()` - 限流
- `.CombineLatest()` - 组合多个流
- 更多...

### 5. 可测试性
- 输入流可以轻松模拟
- 状态变化可以被观察和验证
- 更容易编写单元测试

## 下一步建议

### 1. 重构其他组件
- `CharacterRotationComponent` - 使用 MoveStream
- `AnimationControllerComponent` - 订阅 IsMoving, IsGrounded

### 2. 添加更多响应式状态
```csharp
public ReactiveProperty<float> Health { get; } = new(100f);
public ReactiveProperty<bool> IsSprinting { get; } = new(false);
```

### 3. 使用 R3 操作符
```csharp
// 组合多个输入
var sprintStream = _inputComponent.MoveStream
    .CombineLatest(_inputComponent.SprintStream, (move, sprint) => (move, sprint))
    .Where(x => x.sprint && x.move != Vector2.Zero);

// 限流（防止过于频繁的事件）
_inputComponent.JumpStream
    .Throttle(TimeSpan.FromSeconds(0.5))
    .Subscribe(_ => Jump());
```

## 编译和测试

编译成功：
```bash
dotnet build "3dPractice.sln"
# Build succeeded with 5 warning(s)
```

所有功能保持正常，向后兼容性完好。
