using Godot;
using Godot.Composition;

/// <summary>
/// 飞行移动组件 - 三维全向移动，无重力
/// 绑定到 StateChart 的 "FlyMode" 状态
/// 只有在飞行模式下才会被激活（Power Switch 模式）
/// </summary>
[GlobalClass]
[Component(typeof(CharacterBody3D))]
[ComponentDependency(typeof(BaseInputComponent))]
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

    #endregion

    #region Godot Lifecycle

    public override void _Ready()
    {
        InitializeComponent();
        _phantomCamera = parent.GetNodeOrNull<Node3D>(PhantomCameraPath);
        
        if (_phantomCamera == null)
        {
            GD.PushWarning("FlyMovementComponent: PhantomCamera not found");
        }
    }

    public void OnEntityReady()
    {
        // 订阅输入事件
        _inputComponent = parent.FindAndSubscribeInput(
            HandleMovementInput,
            null // 飞行模式下跳跃键用于上升，不需要单独的跳跃处理
        );

        // 【Power Switch】绑定到 FlyMode 状态
        // 只有在飞行模式下，此组件才会被唤醒执行
        this.BindComponentToState(parent, "StateChart/Root/Movement/FlyMode");
        
        GD.Print("FlyMovementComponent: 已绑定到 FlyMode 状态");
    }

    public override void _Process(double delta)
    {
        // 读取上升/下降输入（飞行模式特有）
        _ascendPressed = Input.IsActionPressed("jump");      // 空格上升
        _descendPressed = Input.IsActionPressed("crouch");   // Ctrl下降
    }

    public override void _PhysicsProcess(double delta)
    {
        // 【纯粹的飞行物理】无需状态判断
        // 因为组件默认休眠，只有 StateChart 激活 FlyMode 时才会执行
        ProcessFlyPhysics(delta);
    }

    public override void _ExitTree()
    {
        _inputComponent?.UnsubscribeInput(HandleMovementInput, null);
    }

    #endregion

    #region Event Handlers

    private void HandleMovementInput(Vector2 inputDir)
    {
        _currentInputDirection = inputDir;
    }

    #endregion

    #region Physics Logic

    private void ProcessFlyPhysics(double delta)
    {
        Vector3 velocity = parent.Velocity;
        Vector3 targetVelocity = CalculateFlyTargetVelocity();

        // 平滑加速/减速
        float acceleration = targetVelocity.Length() > 0.1f ? FlyAcceleration : FlyDeceleration;
        velocity = velocity.Lerp(targetVelocity, acceleration * (float)delta);

        // 应用速度并移动
        parent.Velocity = velocity;
        parent.MoveAndSlide();
    }

    private Vector3 CalculateFlyTargetVelocity()
    {
        Vector3 targetVelocity = Vector3.Zero;

        if (_phantomCamera != null)
        {
            // 根据相机方向计算三维移动
            Vector3 forward = -_phantomCamera.GlobalTransform.Basis.Z; // 相机前方
            Vector3 right = _phantomCamera.GlobalTransform.Basis.X;    // 相机右方
            Vector3 up = Vector3.Up;                                    // 世界上方

            // 水平移动（WASD）
            Vector3 horizontalMove = Vector3.Zero;
            if (_currentInputDirection != Vector2.Zero)
            {
                // 只取相机的水平分量
                Vector3 forwardFlat = forward;
                forwardFlat.Y = 0;
                forwardFlat = forwardFlat.Normalized();
                
                Vector3 rightFlat = right;
                rightFlat.Y = 0;
                rightFlat = rightFlat.Normalized();

                horizontalMove = (rightFlat * _currentInputDirection.X + 
                                 forwardFlat * _currentInputDirection.Y).Normalized();
            }

            // 垂直移动（空格/Ctrl）
            float verticalMove = 0;
            if (_ascendPressed)
                verticalMove += 1.0f;
            if (_descendPressed)
                verticalMove -= 1.0f;

            // 合成目标速度
            targetVelocity = horizontalMove * FlySpeed + up * verticalMove * FlySpeed;
        }
        else
        {
            // 无相机时使用角色本地坐标系
            Vector3 horizontalMove = new Vector3(_currentInputDirection.X, 0, _currentInputDirection.Y);
            float verticalMove = 0;
            if (_ascendPressed) verticalMove += 1.0f;
            if (_descendPressed) verticalMove -= 1.0f;

            targetVelocity = (parent.Transform.Basis * horizontalMove).Normalized() * FlySpeed +
                            Vector3.Up * verticalMove * FlySpeed;
        }

        return targetVelocity;
    }

    #endregion
}
