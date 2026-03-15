using Godot;
using Godot.Composition;
using R3;

/// <summary>
/// 移动组件 - 仅负责物理计算与位移
/// 遵循单一职责原则：只处理移动，不处理输入或动画
/// 使用 R3 响应式编程实现现代化数据流管理
/// </summary>
[GlobalClass]
[Component(typeof(CharacterBody3D))]
public partial class MovementComponent : Node
{
    #region Export Properties

    /// <summary>
    /// PhantomCamera3D 引用（用于计算相对于相机的移动方向）
    /// </summary>
    [Export]
    public NodePath PhantomCameraPath { get; set; } = "PhantomCamera3D";

    /// <summary>
    /// 移动速度 (米/秒)
    /// </summary>
    [Export]
    public float Speed { get; set; } = 5.0f;

    /// <summary>
    /// 跳跃初速度 (米/秒)
    /// </summary>
    [Export]
    public float JumpVelocity { get; set; } = 4.5f;

    /// <summary>
    /// 重力加速度
    /// </summary>
    [Export]
    public float Gravity { get; set; } = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    #endregion

    #region Reactive Properties (响应式状态)

    /// <summary>
    /// 当前是否在移动（有水平速度）
    /// </summary>
    public ReactiveProperty<bool> IsMoving { get; } = new(false);

    /// <summary>
    /// 当前是否在地面上
    /// </summary>
    public ReactiveProperty<bool> IsGrounded { get; } = new(true);

    /// <summary>
    /// 当前速度向量
    /// </summary>
    public ReactiveProperty<Vector3> CurrentVelocity { get; } = new(Vector3.Zero);

    #endregion

    #region Private State

    // 当前移动方向（从输入流更新）
    private Vector2 _currentInputDirection = Vector2.Zero;

    // 输入组件引用
    private BaseInputComponent _inputComponent;

    // PhantomCamera3D 引用
    private Node3D _phantomCamera;
    
    // R3 订阅管理
    private readonly CompositeDisposable _disposables = new();

    #endregion

    #region Godot Lifecycle

    public override void _Ready()
    {
        InitializeComponent();

        // 获取 PhantomCamera3D 引用
        _phantomCamera = parent.GetNodeOrNull<Node3D>(PhantomCameraPath);

        GD.Print($"MovementComponent: 已连接到 Body: {parent.Name}");
        GD.Print($"MovementComponent: Speed={Speed}, JumpVelocity={JumpVelocity}, Gravity={Gravity}");

        if (_phantomCamera == null)
        {
            GD.PushWarning("MovementComponent: 未找到 PhantomCamera3D，将使用角色本地坐标系移动。");
        }
        else
        {
            GD.Print("MovementComponent: PhantomCamera3D 已连接 ✓");
        }
    }

    /// <summary>
    /// Entity 初始化完成后自动调用
    /// 在这里订阅 R3 响应式流
    /// </summary>
    public void OnEntityReady()
    {
        // 查找输入组件（使用扩展方法）
        _inputComponent = parent.GetRequiredComponentInChildren<BaseInputComponent>();
        
        if (_inputComponent == null)
        {
            GD.PushWarning("MovementComponent: 未找到 BaseInputComponent");
            return;
        }

        // 订阅移动输入流
        _inputComponent.MoveStream
            .Subscribe(direction =>
            {
                _currentInputDirection = direction;
            })
            .AddTo(_disposables);

        // 订阅跳跃输入流 - 直接处理跳跃逻辑
        _inputComponent.JumpStream
            .Where(_ => parent.IsOnFloor()) // 只在地面上时才能跳跃
            .Subscribe(_ =>
            {
                var velocity = parent.Velocity;
                velocity.Y = JumpVelocity;
                parent.Velocity = velocity;
                GD.Print("MovementComponent: 跳跃！");
            })
            .AddTo(_disposables);

        GD.Print("MovementComponent: R3 响应式流已订阅 ✓");
    }

    public override void _PhysicsProcess(double delta)
    {
        ProcessPhysics(delta);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposables?.Dispose();
            IsMoving?.Dispose();
            IsGrounded?.Dispose();
            CurrentVelocity?.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion

    #region Physics Logic

    /// <summary>
    /// 物理帧处理
    /// </summary>
    private void ProcessPhysics(double delta)
    {
        Vector3 velocity = parent.Velocity;

        // 1. 应用重力
        if (!parent.IsOnFloor())
        {
            velocity.Y -= Gravity * (float)delta;
        }

        // 2. 处理水平移动
        Vector3 direction;

        if (_phantomCamera != null)
        {
            // 使用 PhantomCamera 的全局变换计算移动方向
            Vector3 forward = _phantomCamera.GlobalTransform.Basis.Z;
            Vector3 right = _phantomCamera.GlobalTransform.Basis.X;
            forward.Y = 0;
            right.Y = 0;
            forward = forward.Normalized();
            right = right.Normalized();

            direction = (right * _currentInputDirection.X + forward * _currentInputDirection.Y).Normalized();
        }
        else
        {
            // 没有相机时使用角色本地坐标系
            direction = (parent.Transform.Basis * new Vector3(_currentInputDirection.X, 0, _currentInputDirection.Y))
                .Normalized();
        }

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

        // 3. 应用速度并移动
        parent.Velocity = velocity;
        parent.MoveAndSlide();

        // 4. 更新响应式状态（R3 会自动判断值是否改变）
        Vector3 horizontalVelocity = new Vector3(velocity.X, 0, velocity.Z);
        IsMoving.Value = horizontalVelocity.Length() > 0.1f;
        IsGrounded.Value = parent.IsOnFloor();
        CurrentVelocity.Value = velocity;
    }

    #endregion
}
