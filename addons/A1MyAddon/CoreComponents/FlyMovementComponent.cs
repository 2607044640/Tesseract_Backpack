using Godot;
using Godot.Composition;
using R3;

[GlobalClass]
public partial class FlyMovementComponent : Node
{
    #region Export Properties

    [Export] public float FlySpeed { get; set; } = 8.0f;
    [Export] public float FlyAcceleration { get; set; } = 20.0f;
    [Export] public float FlyDeceleration { get; set; } = 15.0f;
    [Export] public NodePath PhantomCameraPath { get; set; } = "PhantomCamera3D";

    #endregion

    #region Private State

    private Vector2 _currentInputDirection = Vector2.Zero;
    private bool _ascendPressed = false;  // 上升（空格）
    private bool _descendPressed = false; // 下降（Ctrl）
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
            GD.PushError("FlyMovementComponent: 无法找到 CharacterBody3D 实体！");
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
            
            GD.Print($"✓ FlyMovementComponent 已订阅 {_inputComponent.GetType().Name} R3流");
        }
        
        // 【Power Switch】自动绑定到父状态节点
        this.AutoBindToParentState();
        
        GD.Print("FlyMovementComponent: 已完成初始化");
    }

    public override void _Process(double delta)
    {
        _ascendPressed = Input.IsActionPressed("jump");
        _descendPressed = Input.IsActionPressed("crouch");
    }

    public override void _PhysicsProcess(double delta)
    {
        ProcessFlyPhysics(delta);
    }

    public override void _ExitTree()
    {
        _disposables.Dispose();
    }

    #endregion

    #region Physics Logic

    // 飞行物理处理 - 三维全向移动，无重力
    // 算法：计算目标速度（水平+垂直） -> 平滑插值（Lerp加速/减速） -> 应用速度
    private void ProcessFlyPhysics(double delta)
    {
        Vector3 velocity = _entity.Velocity;
        Vector3 targetVelocity = CalculateFlyTargetVelocity();

        float acceleration = targetVelocity.Length() > 0.1f ? FlyAcceleration : FlyDeceleration;
        velocity = velocity.Lerp(targetVelocity, acceleration * (float)delta);

        _entity.Velocity = velocity;
        _entity.MoveAndSlide();
    }

    private Vector3 CalculateFlyTargetVelocity()
    {
        Vector3 targetVelocity = Vector3.Zero;

        if (_phantomCamera != null)
        {
            Vector3 forward = -_phantomCamera.GlobalTransform.Basis.Z;
            Vector3 right = _phantomCamera.GlobalTransform.Basis.X;
            Vector3 up = Vector3.Up;

            Vector3 horizontalMove = Vector3.Zero;
            if (_currentInputDirection != Vector2.Zero)
            {
                Vector3 forwardFlat = forward;
                forwardFlat.Y = 0;
                forwardFlat = forwardFlat.Normalized();
                
                Vector3 rightFlat = right;
                rightFlat.Y = 0;
                rightFlat = rightFlat.Normalized();

                horizontalMove = (rightFlat * _currentInputDirection.X - 
                                 forwardFlat * _currentInputDirection.Y).Normalized();
            }

            float verticalMove = 0;
            if (_ascendPressed)
                verticalMove += 1.0f;
            if (_descendPressed)
                verticalMove -= 1.0f;

            targetVelocity = horizontalMove * FlySpeed + up * verticalMove * FlySpeed;
        }
        else
        {
            Vector3 horizontalMove = new Vector3(_currentInputDirection.X, 0, -_currentInputDirection.Y);
            float verticalMove = 0;
            if (_ascendPressed) verticalMove += 1.0f;
            if (_descendPressed) verticalMove -= 1.0f;

            targetVelocity = (_entity.Transform.Basis * horizontalMove).Normalized() * FlySpeed +
                            Vector3.Up * verticalMove * FlySpeed;
        }

        return targetVelocity;
    }

    #endregion
}
