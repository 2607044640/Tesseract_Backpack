using Godot;
using Godot.Composition;
using R3;

/// 角色旋转组件 - 负责让角色模型面向移动方向
/// 依赖抽象的 BaseInputComponent，可复用于玩家和 AI
[GlobalClass]
[Component(typeof(CharacterBody3D))]
public partial class CharacterRotationComponent : Node
{
    #region Export Properties
    
    [Export] public NodePath CharacterModelPath { get; set; } = "KunoSkin";
    [Export] public NodePath PhantomCameraPath { get; set; } = "PhantomCamera3D";
    [Export] public float RotationSpeed { get; set; } = 10.0f;

    #endregion

    #region Private Fields

    private Node3D _characterModel;
    private Vector2 _currentInputDir = Vector2.Zero;
    private BaseInputComponent _inputComponent;
    private Node3D _phantomCamera;
    private readonly CompositeDisposable _disposables = new();

    #endregion

    #region Godot Lifecycle

    public override void _Ready()
    {
        InitializeComponent();

        // 初始化角色模型引用
        _characterModel = parent.GetNodeOrNull<Node3D>(CharacterModelPath);
        if (_characterModel == null)
        {
            GD.PushError($"[{Name}] Character model not found: {CharacterModelPath}");
            return;
        }

        // 获取 PhantomCamera3D 引用
        _phantomCamera = parent.GetNodeOrNull<Node3D>(PhantomCameraPath);
        if (_phantomCamera == null)
        {
            GD.PushError($"[{Name}] PhantomCamera3D not found: {PhantomCameraPath}");
            return;
        }
    }

    /// Entity 初始化完成后自动调用
    public void OnEntityReady()
    {
        // 直接获取InputComponent并订阅R3流
        _inputComponent = parent.GetRequiredComponentInChildren<BaseInputComponent>();
        if (_inputComponent != null)
        {
            _inputComponent.MovementInput
                .Subscribe(direction => _currentInputDir = direction)
                .AddTo(_disposables);
            
            GD.Print($"✓ CharacterRotationComponent 已订阅 {_inputComponent.GetType().Name} R3流");
        }
    }

    public override void _Process(double delta)
    {
        UpdateCharacterRotation(delta);
    }

    public override void _ExitTree()
    {
        _disposables.Dispose();
    }

    #endregion

    #region Rotation Logic

    /// 基于相机方向和输入计算角色朝向
    /// 目的：让角色面向移动方向，支持相机相对移动
    /// 示例：按W键时，角色面向相机前方；按D键时，角色面向相机右方
    /// 算法：1. 获取相机前/右向量 -> 2. 根据输入合成移动方向 -> 3. 平滑旋转角色
    private void UpdateCharacterRotation(double delta)
    {
        if (_characterModel == null || _phantomCamera == null) return;
        if (_currentInputDir == Vector2.Zero) return;

        // 基于 PhantomCamera 方向计算移动方向
        Vector3 forward = _phantomCamera.GlobalTransform.Basis.Z;
        Vector3 right = _phantomCamera.GlobalTransform.Basis.X;
        forward.Y = 0;
        right.Y = 0;
        forward = forward.Normalized();
        right = right.Normalized();

        Vector3 direction = (right * _currentInputDir.X + forward * _currentInputDir.Y).Normalized();

        if (direction != Vector3.Zero)
        {
            float targetAngle = Mathf.Atan2(direction.X, direction.Z);
            Quaternion targetRotation = new(Vector3.Up, targetAngle);
            _characterModel.Quaternion = _characterModel.Quaternion.Slerp(targetRotation, (float)delta * RotationSpeed);
        }
    }

    #endregion
}