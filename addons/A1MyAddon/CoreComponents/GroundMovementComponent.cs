using Godot;
using Godot.Composition;

/// <summary>
/// 地面移动组件 - 处理重力、跳跃、地面移动
/// 绑定到 StateChart 的 "GroundMode" 状态
/// 只有在地面模式下才会被激活（Power Switch 模式）
/// 
/// 注意：此组件作为 GroundMode 状态节点的子节点，使用 AutoBindToParentState() 自动绑定
/// </summary>
[GlobalClass]
public partial class GroundMovementComponent : Node
{
    #region Export Properties

    [Export] public float Speed { get; set; } = 5.0f;
    [Export] public float JumpVelocity { get; set; } = 4.5f;
    [Export] public float Gravity { get; set; } = 9.8f;
    [Export] public NodePath PhantomCameraPath { get; set; } = "PhantomCamera3D";

    #endregion

    #region Private State

    private Vector2 _currentInputDirection = Vector2.Zero;
    private bool _jumpRequested = false;
    private BaseInputComponent _inputComponent;
    private Node3D _phantomCamera;
    private CharacterBody3D _entity; // 真实的实体引用

    #endregion

    #region Godot Lifecycle

    public override void _Ready()
    {
        // 获取真实的实体（因为现在组件是State节点的子节点）
        _entity = this.GetEntity<CharacterBody3D>();
        if (_entity == null)
        {
            GD.PushError("GroundMovementComponent: 无法找到 CharacterBody3D 实体！");
            return;
        }

        _phantomCamera = _entity.GetNodeOrNull<Node3D>(PhantomCameraPath);
        if (_phantomCamera == null)
        {
            GD.PushError($"[{Name}] PhantomCamera not found: {PhantomCameraPath}");
            return;
        }
        
        // 直接获取InputComponent并订阅事件
        _inputComponent = _entity.GetRequiredComponentInChildren<BaseInputComponent>();
        if (_inputComponent != null)
        {
            _inputComponent.OnMovementInput += HandleMovementInput;
            _inputComponent.OnJumpJustPressed += HandleJumpInput;
            GD.Print($"✓ GroundMovementComponent 已订阅 {_inputComponent.GetType().Name} 事件");
        }
        
        // 【Power Switch】自动绑定到父状态节点
        this.AutoBindToParentState();
        
        GD.Print("GroundMovementComponent: 已完成初始化");
    }

    public override void _PhysicsProcess(double delta)
    {
        ProcessGroundPhysics(delta);
    }

    public override void _ExitTree()
    {
        if (_inputComponent != null)
        {
            _inputComponent.OnMovementInput -= HandleMovementInput;
            _inputComponent.OnJumpJustPressed -= HandleJumpInput;
        }
    }

    #endregion

    #region Event Handlers

    private void HandleMovementInput(Vector2 inputDir)
    {
        _currentInputDirection = inputDir;
    }

    private void HandleJumpInput()
    {
        _jumpRequested = true;
    }

    #endregion

    #region Physics Logic

    /// <summary>
    /// 地面物理处理
    /// 目的：实现重力、跳跃、地面移动的完整物理模拟
    /// 示例：角色在地面按空格跳起，空中受重力下落，WASD控制水平移动
    /// 算法：1. 应用重力 -> 2. 处理跳跃输入 -> 3. 计算水平移动 -> 4. 应用速度并移动
    /// </summary>
    private void ProcessGroundPhysics(double delta)
    {
        Vector3 velocity = _entity.Velocity;

        if (!_entity.IsOnFloor())
        {
            velocity.Y -= Gravity * (float)delta;
        }

        if (_jumpRequested && _entity.IsOnFloor())
        {
            velocity.Y = JumpVelocity;
            _jumpRequested = false;
            _entity.SendStateEvent("jumped");
        }
        else if (_jumpRequested)
        {
            _jumpRequested = false;
        }

        Vector3 direction = CalculateMovementDirection();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, Speed);
        }

        _entity.Velocity = velocity;
        _entity.MoveAndSlide();
    }

    private Vector3 CalculateMovementDirection()
    {
        if (_phantomCamera != null)
        {
            // 使用相机方向计算移动
            Vector3 forward = _phantomCamera.GlobalTransform.Basis.Z;
            Vector3 right = _phantomCamera.GlobalTransform.Basis.X;
            forward.Y = 0;
            right.Y = 0;
            forward = forward.Normalized();
            right = right.Normalized();

            return (right * _currentInputDirection.X + forward * _currentInputDirection.Y).Normalized();
        }
        else
        {
            // 使用角色本地坐标系
            return (_entity.Transform.Basis * new Vector3(_currentInputDirection.X, 0, _currentInputDirection.Y))
                .Normalized();
        }
    }

    #endregion
}
