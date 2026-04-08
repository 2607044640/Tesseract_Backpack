using Godot;
using Godot.Composition;

/// <summary>
/// 角色旋转组件 - 负责让角色模型面向移动方向
/// 依赖抽象的 BaseInputComponent，可复用于玩家和 AI
/// </summary>
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

    #endregion

    #region Godot Lifecycle

    public override void _Ready()
    {
        InitializeComponent();

        // 初始化角色模型引用
        _characterModel = parent.GetNodeOrNull<Node3D>(CharacterModelPath);
        if (_characterModel == null)
        {
            GD.PushWarning($"CharacterRotationComponent: 角色模型未找到: {CharacterModelPath}");
        }
        else
        {
            GD.Print("CharacterRotationComponent: 角色模型已连接 ✓");
        }

        // 获取 PhantomCamera3D 引用
        _phantomCamera = parent.GetNodeOrNull<Node3D>(PhantomCameraPath);

        if (_phantomCamera == null)
        {
            GD.PushWarning("CharacterRotationComponent: 未找到 PhantomCamera3D，旋转功能将不可用。");
        }
        else
        {
            GD.Print("CharacterRotationComponent: PhantomCamera3D 已连接 ✓");
        }
    }

    /// <summary>
    /// Entity 初始化完成后自动调用
    /// </summary>
    public void OnEntityReady()
    {
        // 直接获取InputComponent并订阅事件
        _inputComponent = parent.GetRequiredComponentInChildren<BaseInputComponent>();
        if (_inputComponent != null)
        {
            _inputComponent.OnMovementInput += HandleMovementInput;
            GD.Print($"✓ CharacterRotationComponent 已订阅 {_inputComponent.GetType().Name} 事件");
        }
    }

    public override void _Process(double delta)
    {
        UpdateCharacterRotation(delta);
    }

    public override void _ExitTree()
    {
        if (_inputComponent != null)
        {
            _inputComponent.OnMovementInput -= HandleMovementInput;
        }
    }

    #endregion

    #region Event Handlers

    private void HandleMovementInput(Vector2 inputDir)
    {
        _currentInputDir = inputDir;
    }

    #endregion

    #region Rotation Logic

    /// <summary>
    /// 基于相机方向和输入计算角色朝向
    /// 目的：让角色面向移动方向，支持相机相对移动
    /// 示例：按W键时，角色面向相机前方；按D键时，角色面向相机右方
    /// 算法：1. 获取相机前/右向量 -> 2. 根据输入合成移动方向 -> 3. 平滑旋转角色
    /// </summary>
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