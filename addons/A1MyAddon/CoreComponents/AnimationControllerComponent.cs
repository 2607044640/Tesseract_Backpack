using Godot;
using Godot.Composition;

/// <summary>
/// 动画控制器组件 - 极简数值驱动架构
/// 
/// 架构原则：
/// - 宏观状态由外部信号控制（StateChart 在编辑器中连接信号到公开方法）
/// - 微观状态由 Velocity 数值驱动（无 Input 依赖）
/// - 所有动画选择逻辑封装在 AnimationSet 中
/// - 控制器只负责"问配置要什么动画，然后播放"
/// 
/// 使用方法：
/// 1. 在 Godot 编辑器中，将 StateChart 的 state_entered 信号连接到：
///    - GroundMode.state_entered → AnimationControllerComponent.EnterGroundMode()
///    - FlyMode.state_entered → AnimationControllerComponent.EnterFlyMode()
/// 2. 组件会根据 parent.Velocity 自动选择合适的动画
/// </summary>
[GlobalClass]
[Component(typeof(CharacterBody3D))]
public partial class AnimationControllerComponent : Node
{
    #region Export Properties
    
    [ExportGroup("基础配置")]
    [Export] public NodePath CharacterModelPath { get; set; } = "KunoSkin";
    
    [Export] public NodePath AnimationPlayerPath { get; set; } = "AnimationPlayer";
    
    [Export(PropertyHint.Range, "0.0,1.0,0.05")]
    public float AnimationBlendTime { get; set; } = 0.2f;
    
    [Export] public CharacterAnimationConfig AnimConfig { get; set; }
    
    #endregion

    #region Private Fields
    
    private AnimationPlayer _animPlayer;
    private string _currentAnimation = "";
    private string _currentMode = "Ground";
    
    #endregion

    #region Godot Lifecycle
    
    public override void _Ready()
    {
        InitializeComponent();
        InitializeAnimation();
    }
    
    public override void _Process(double delta)
    {
        UpdateAnimation();
    }
    
    #endregion

    #region Public Methods (For StateChart Signal Connections)
    
    /// <summary>
    /// 进入地面模式
    /// 在 Godot 编辑器中连接：GroundMode.state_entered → 此方法
    /// </summary>
    public void EnterGroundMode()
    {
        _currentMode = "Ground";
        GD.Print("[AnimationController] 进入地面模式");
    }
    
    /// <summary>
    /// 进入飞行模式
    /// 在 Godot 编辑器中连接：FlyMode.state_entered → 此方法
    /// </summary>
    public void EnterFlyMode()
    {
        _currentMode = "Fly";
        GD.Print("[AnimationController] 进入飞行模式");
    }
    
    #endregion

    #region Initialization
    
    private void InitializeAnimation()
    {
        var characterModel = parent.GetNodeOrNull<Node3D>(CharacterModelPath);
        if (characterModel == null)
        {
            GD.PushWarning($"AnimationControllerComponent: 角色模型未找到: {CharacterModelPath}");
            return;
        }

        var animPlayerFullPath = CharacterModelPath + "/" + AnimationPlayerPath;
        _animPlayer = parent.GetNodeOrNull<AnimationPlayer>(animPlayerFullPath);
        
        if (_animPlayer == null)
        {
            GD.PushWarning($"AnimationControllerComponent: AnimationPlayer 未找到: {animPlayerFullPath}");
            return;
        }

        if (AnimConfig != null)
        {
            // 使用新的一键初始化方法
            AnimConfig.ApplyAndInitialize(_animPlayer);
        }
    }
    
    #endregion

    #region Animation Logic
    
    /// <summary>
    /// 更新动画（极简逻辑）
    /// 只负责：1. 问配置要什么动画  2. 播放动画
    /// </summary>
    private void UpdateAnimation()
    {
        if (_animPlayer == null || AnimConfig == null) return;
        
        // 从配置类获取应该播放的动画（所有逻辑都在 CharacterAnimationConfig 中）
        var (animName, speed) = AnimConfig.GetAnimationForState(
            _currentMode,
            parent.Velocity,
            parent.IsOnFloor()
        );
        
        // 播放动画
        PlayAnimation(animName, speed);
    }
    
    /// <summary>
    /// 播放动画（带过渡）
    /// </summary>
    private void PlayAnimation(string targetAnim, float targetSpeed)
    {
        if (string.IsNullOrEmpty(targetAnim) || _currentAnimation == targetAnim)
            return;
        
        _animPlayer.Play(targetAnim, customBlend: AnimationBlendTime, customSpeed: targetSpeed);
        _currentAnimation = targetAnim;
    }
    
    #endregion
}
