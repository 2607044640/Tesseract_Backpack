using Godot;
using Godot.Composition;
using PhantomCamera;

/// <summary>
/// 相机控制组件 - 使用 PhantomCamera3D 处理第三人称相机旋转和鼠标输入
/// </summary>
[GlobalClass]
[Component(typeof(CharacterBody3D))]
public partial class CameraControlComponent : Node
{
    #region Export Properties
    
    /// <summary>
    /// 鼠标灵敏度
    /// </summary>
    [Export] public float MouseSensitivity { get; set; } = 0.05f;
    
    /// <summary>
    /// PhantomCamera3D 节点路径
    /// </summary>
    [Export] public NodePath PCamPath { get; set; } = "PhantomCamera3D";
    
    /// <summary>
    /// 最小 Pitch 角度（度数）
    /// </summary>
    [Export] public float MinPitch { get; set; } = -89.9f;
    
    /// <summary>
    /// 最大 Pitch 角度（度数）
    /// </summary>
    [Export] public float MaxPitch { get; set; } = 50f;
    
    /// <summary>
    /// 最小 Yaw 角度（度数）
    /// </summary>
    [Export] public float MinYaw { get; set; } = 0f;
    
    /// <summary>
    /// 最大 Yaw 角度（度数）
    /// </summary>
    [Export] public float MaxYaw { get; set; } = 360f;
    
    #endregion

    #region Private Fields
    
    private PhantomCamera3D _pCam;
    
    #endregion

    #region Godot Lifecycle
    
    public override void _Ready()
    {
        InitializeComponent();
        
        // 先捕获鼠标
        Input.MouseMode = Input.MouseModeEnum.Captured;
        
        // 初始化 PhantomCamera3D 引用
        Node3D pcamNode = parent.GetNodeOrNull<Node3D>(PCamPath);
        if (pcamNode == null)
        {
            GD.PushError($"CameraControlComponent: PhantomCamera3D 节点未找到 (路径: {PCamPath})");
            GD.PushError("请确保 Player3D 场景中有 PhantomCamera3D 节点");
            return;
        }
        
        // 使用扩展方法转换为 C# 包装类
        _pCam = pcamNode.AsPhantomCamera3D();
        if (_pCam == null)
        {
            GD.PushError("CameraControlComponent: 无法将节点转换为 PhantomCamera3D");
            return;
        }
        
        GD.Print("CameraControlComponent: PhantomCamera 系统已初始化 ✓");
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        HandleCameraInput(@event);
    }
    
    #endregion

    #region Camera Control
    
    /// <summary>
    /// 处理相机输入
    /// </summary>
    private void HandleCameraInput(InputEvent @event)
    {
        if (_pCam == null) return;
        
        // 处理鼠标移动
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // 读取当前第三人称欧拉角（度数）
            Vector3 pcamRotationDegrees = _pCam.GetThirdPersonRotationDegrees();
            
            // 修改 X 轴旋转（Pitch - 上下）
            pcamRotationDegrees.X -= mouseMotion.Relative.Y * MouseSensitivity;
            
            // 限制上下视角范围
            pcamRotationDegrees.X = Mathf.Clamp(pcamRotationDegrees.X, MinPitch, MaxPitch);
            
            // 修改 Y 轴旋转（Yaw - 左右）
            pcamRotationDegrees.Y -= mouseMotion.Relative.X * MouseSensitivity;
            
            // 循环 Yaw 角度（0-360度）
            pcamRotationDegrees.Y = Mathf.Wrap(pcamRotationDegrees.Y, MinYaw, MaxYaw);
            
            // 应用新的旋转角度
            _pCam.SetThirdPersonRotationDegrees(pcamRotationDegrees);
        }
        
        // ESC 释放鼠标
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }
    
    #endregion
}
