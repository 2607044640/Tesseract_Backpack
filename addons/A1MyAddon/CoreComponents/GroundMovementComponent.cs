using Godot;
using Godot.Composition;
using R3;

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
    private readonly CompositeDisposable _disposables = new();

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
        
        // 直接获取InputComponent并订阅R3流
        _inputComponent = _entity.GetRequiredComponentInChildren<BaseInputComponent>();
        if (_inputComponent != null)
        {
            _inputComponent.MovementInput
                .Subscribe(direction => _currentInputDirection = direction)
                .AddTo(_disposables);
            
            _inputComponent.JumpPressed
                .Subscribe(_ => _jumpRequested = true)
                .AddTo(_disposables);
            
            GD.Print($"✓ GroundMovementComponent 已订阅 {_inputComponent.GetType().Name} R3流");
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
        _disposables.Dispose();
    }

    #endregion

    #region Physics Logic

    // 地面物理处理 - 重力、跳跃、地面移动
    // 算法：应用重力 -> 处理跳跃输入 -> 计算水平移动（相机方向） -> 应用速度
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
