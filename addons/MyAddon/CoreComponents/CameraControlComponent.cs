using Godot;
using Godot.Composition;
using PhantomCamera;

/// <summary>
/// 相机控制组件 - 使用 PhantomCamera3D 处理第三人称相机旋转和鼠标输入
/// 纯净版：只负责相机旋转，不处理其他逻辑
/// </summary>
[GlobalClass]
[Component(typeof(CharacterBody3D))]
public partial class CameraControlComponent : Node
{
    #region Export Properties
    
    /// <summary>
    /// PhantomCamera3D 节点路径
    /// </summary>
    [Export] public NodePath PCamPath { get; set; } = "PhantomCamera3D";
    
    /// <summary>
    /// 鼠标灵敏度
    /// </summary>
    [Export] public float MouseSensitivity { get; set; } = 0.05f;
    
    /// <summary>
    /// 最小 Pitch 角度（上下视角限制，度数）
    /// </summary>
    [Export] public float MinPitch { get; set; } = -89.9f;
    
    /// <summary>
    /// 最大 Pitch 角度（上下视角限制，度数）
    /// </summary>
    [Export] public float MaxPitch { get; set; } = 50f;
    
    /// <summary>
    /// 是否覆盖场景中的 FollowOffset（默认 false 使用场景配置）
    /// </summary>
    [Export] public bool OverrideFollowOffset { get; set; } = false;
    
    /// <summary>
    /// 相机跟随偏移（仅在 OverrideFollowOffset=true 时生效）
    /// 推荐 Y=0.6 避免相机贴地穿模
    /// </summary>
    [Export] public Vector3 FollowOffset { get; set; } = new Vector3(0, 0.5f, 0);
    
    #endregion

    #region Private Fields
    
    private PhantomCamera3D _pCam;
    
    #endregion

    #region Godot Lifecycle
    
    public override void _Ready()
    {
        InitializeComponent();
        
        // 锁定鼠标
        Input.MouseMode = Input.MouseModeEnum.Captured;
        
        // 获取 PhantomCamera3D 节点并转换为 C# 包装类
        Node3D pcamNode = parent.GetNodeOrNull<Node3D>(PCamPath);
        if (pcamNode == null)
        {
            GD.PushError($"CameraControlComponent: 未找到 PhantomCamera3D 节点 (路径: {PCamPath})");
            return;
        }
        
        _pCam = pcamNode.AsPhantomCamera3D();
        if (_pCam == null)
        {
            GD.PushError("CameraControlComponent: 无法转换为 PhantomCamera3D 包装类");
            return;
        }
        
        // 如果启用了覆盖，则设置 FollowOffset
        if (OverrideFollowOffset)
        {
            _pCam.FollowOffset = FollowOffset;
            GD.Print($"CameraControlComponent: 覆盖 FollowOffset = {FollowOffset}");
        }
        
        GD.Print("CameraControlComponent: 初始化完成 ✓");
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        // 处理鼠标移动
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            HandleMouseMovement(mouseMotion);
        }
        
        // ESC 键释放鼠标，(todo user:可能会有bug，UI按esc不一定到UI上）
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }
    
    #endregion

    #region Camera Control
    
    /// <summary>
    /// 处理鼠标移动，更新相机旋转
    /// </summary>
    private void HandleMouseMovement(InputEventMouseMotion mouseMotion)
    {
        if (_pCam == null) return;
        
        // 1. 获取当前第三人称旋转角度（弧度）
        Vector3 currentRot = _pCam.GetThirdPersonRotation();
        
        // 2. 根据鼠标移动调整旋转
        // Y 轴（Yaw）：左右旋转
        currentRot.Y -= mouseMotion.Relative.X * MouseSensitivity * 0.01f;
        
        // X 轴（Pitch）：上下旋转
        currentRot.X -= mouseMotion.Relative.Y * MouseSensitivity * 0.01f;
        
        // 3. 限制上下视角范围（转换为弧度）
        float minPitchRad = Mathf.DegToRad(MinPitch);
        float maxPitchRad = Mathf.DegToRad(MaxPitch);
        currentRot.X = Mathf.Clamp(currentRot.X, minPitchRad, maxPitchRad);
        
        // 4. 应用新的旋转
        _pCam.SetThirdPersonRotation(currentRot);
    }
    
    #endregion
}
