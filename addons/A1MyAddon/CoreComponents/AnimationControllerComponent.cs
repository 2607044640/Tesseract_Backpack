using Godot;
using Godot.Composition;

/// <summary>
/// 动画控制器组件 - 极简信号驱动架构
/// 
/// 架构原则：
/// - 宏观状态由外部信号控制（StateChart 在编辑器中连接信号到公开方法）
/// - 微观状态由 Velocity 数值驱动（无 Input 依赖）
/// - 动画可用性在 _Ready 中一次性缓存（零每帧查询）
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
    
    [Export] public NodePath CharacterModelPath { get; set; } = "KunoSkin";
    [Export] public NodePath AnimationPlayerPath { get; set; } = "AnimationPlayer";
    
    [Export(PropertyHint.Range, "0.0,1.0,0.05")]
    public float AnimationBlendTime { get; set; } = 0.2f;
    
    [Export] public CharacterAnimationConfig AnimConfig { get; set; }
    
    /// <summary>
    /// 速度阈值：超过此值播放冲刺动画
    /// </summary>
    [Export] public float SprintThreshold { get; set; } = 6.0f;
    
    /// <summary>
    /// 速度阈值：超过此值播放移动动画
    /// </summary>
    [Export] public float MoveThreshold { get; set; } = 0.1f;
    
    #endregion

    #region Private Fields
    
    private AnimationPlayer _animPlayer;
    private AnimationSet _animSet;
    private string _currentAnimation = "";
    private string _currentMode = "Ground";
    
    // 缓存动画可用性（避免每帧查询）
    private string _cachedIdleAnim = "";
    private string _cachedRunAnim = "";
    private string _cachedSprintAnim = "";
    private string _cachedJumpAnim = "";
    
    #endregion

    #region Godot Lifecycle
    
    public override void _Ready()
    {
        InitializeComponent();
        InitializeAnimation();
        CacheAvailableAnimations();
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
            AnimConfig.ApplyToAnimationPlayer(_animPlayer);
            _animSet = AnimConfig.AnimationSet;
        }
    }
    
    /// <summary>
    /// 缓存可用动画（避免每帧调用 HasAnimation）
    /// </summary>
    private void CacheAvailableAnimations()
    {
        if (_animPlayer == null) return;
        
        // 缓存 Idle 动画
        _cachedIdleAnim = FindFirstAvailableAnimation(
            AnimationNames.Idle,
            AnimationNames.IdleAlt
        );
        
        // 缓存 Run 动画
        _cachedRunAnim = FindFirstAvailableAnimation(
            AnimationNames.Run,
            AnimationNames.RunAlt
        );
        
        // 缓存 Sprint 动画
        _cachedSprintAnim = FindFirstAvailableAnimation(
            AnimationNames.Sprint,
            AnimationNames.SprintAlt,
            AnimationNames.FastRun
        );
        
        // 缓存 Jump 动画
        _cachedJumpAnim = FindFirstAvailableAnimation(
            AnimationNames.JumpStart,
            AnimationNames.JumpStartAlt,
            AnimationNames.JumpLoop,
            AnimationNames.JumpLoopAlt,
            AnimationNames.Jump
        );
        
        GD.Print($"[AnimationController] 缓存动画: Idle={_cachedIdleAnim}, Run={_cachedRunAnim}, Sprint={_cachedSprintAnim}, Jump={_cachedJumpAnim}");
    }
    
    /// <summary>
    /// 查找第一个可用的动画（仅在初始化时调用一次）
    /// </summary>
    private string FindFirstAvailableAnimation(params string[] animNames)
    {
        foreach (var name in animNames)
        {
            if (_animPlayer.HasAnimation(name))
                return name;
        }
        return "";
    }
    
    #endregion

    #region Animation Logic
    
    /// <summary>
    /// 更新动画（基于当前模式和物理状态）
    /// </summary>
    private void UpdateAnimation()
    {
        if (_animPlayer == null) return;
        
        string targetAnim = "";
        
        switch (_currentMode)
        {
            case "Ground":
                targetAnim = SelectGroundAnimation();
                break;
            
            case "Fly":
                targetAnim = SelectFlyAnimation();
                break;
            
            default:
                targetAnim = _cachedIdleAnim;
                break;
        }
        
        PlayAnimation(targetAnim);
    }
    
    /// <summary>
    /// 选择地面模式动画（基于 Velocity，无 Input 依赖）
    /// </summary>
    private string SelectGroundAnimation()
    {
        Vector3 velocity = parent.Velocity;
        float horizontalSpeed = new Vector2(velocity.X, velocity.Z).Length();
        
        // 优先级：空中 > 冲刺 > 移动 > 静止
        if (!parent.IsOnFloor())
        {
            return _cachedJumpAnim;
        }
        
        if (horizontalSpeed > SprintThreshold && !string.IsNullOrEmpty(_cachedSprintAnim))
        {
            return _cachedSprintAnim;
        }
        
        if (horizontalSpeed > MoveThreshold && !string.IsNullOrEmpty(_cachedRunAnim))
        {
            return _cachedRunAnim;
        }
        
        return _cachedIdleAnim;
    }
    
    /// <summary>
    /// 选择飞行模式动画（基于 Velocity）
    /// </summary>
    private string SelectFlyAnimation()
    {
        Vector3 velocity = parent.Velocity;
        float speed = velocity.Length();
        
        if (speed > MoveThreshold && !string.IsNullOrEmpty(_cachedRunAnim))
        {
            return _cachedRunAnim;
        }
        
        return _cachedIdleAnim;
    }
    
    /// <summary>
    /// 播放动画（带过渡和速度控制）
    /// </summary>
    private void PlayAnimation(string targetAnim)
    {
        if (string.IsNullOrEmpty(targetAnim) || _currentAnimation == targetAnim)
            return;
        
        float animSpeed = 1.0f;
        if (_animSet != null)
        {
            animSpeed = _animSet.GetAnimationSpeed(targetAnim);
        }
        
        _animPlayer.Play(targetAnim, customBlend: AnimationBlendTime, customSpeed: animSpeed);
        _currentAnimation = targetAnim;
    }
    
    #endregion
}
