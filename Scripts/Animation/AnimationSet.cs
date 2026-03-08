using Godot;

/// <summary>
/// 动画合集 - 类似UE的Data Asset
/// 集中管理角色的所有动画
/// </summary>
[GlobalClass]
public partial class AnimationSet : Resource
{
    [Export] public new string SetName = "默认动画集"; // 支持日文
    
    // 基础移动动画
    [Export] public Animation IdleAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float IdleAnimationSpeed = 1.0f;
    
    [Export] public Animation WalkAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float WalkAnimationSpeed = 1.0f;
    
    [Export] public Animation RunAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float RunAnimationSpeed = 1.0f;
    
    [Export] public Animation SprintAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float SprintAnimationSpeed = 1.0f;
    
    // 跳跃和空中动画
    [Export] public Animation JumpStartAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float JumpStartAnimationSpeed = 1.0f;
    
    [Export] public Animation JumpLoopAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float JumpLoopAnimationSpeed = 1.0f;
    
    [Export] public Animation FallAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float FallAnimationSpeed = 1.0f;
    
    [Export] public Animation LandAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float LandAnimationSpeed = 1.0f;
    
    // 战斗动画
    [Export] public Animation Attack1Animation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float Attack1AnimationSpeed = 1.0f;
    
    [Export] public Animation Attack2Animation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float Attack2AnimationSpeed = 1.0f;
    
    [Export] public Animation Attack3Animation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float Attack3AnimationSpeed = 1.0f;
    
    // 其他动画
    [Export] public Animation DodgeAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float DodgeAnimationSpeed = 1.0f;
    
    [Export] public Animation HitAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float HitAnimationSpeed = 1.0f;
    
    [Export] public Animation DeathAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float DeathAnimationSpeed = 1.0f;
    
    /// <summary>
    /// 自动设置所有动画的循环模式
    /// 移动动画（Idle/Walk/Run/Sprint/Fall/JumpLoop）会循环
    /// 一次性动画（Jump/Attack/Hit/Death等）不循环
    /// </summary>
    public void SetupLoopModes()
    {
        // 应该循环的动画
        SetLoopMode(IdleAnimation, true);
        SetLoopMode(WalkAnimation, true);
        SetLoopMode(RunAnimation, true);
        SetLoopMode(SprintAnimation, true);
        SetLoopMode(JumpLoopAnimation, true);
        SetLoopMode(FallAnimation, true);
        
        // 不应该循环的动画
        SetLoopMode(JumpStartAnimation, false);
        SetLoopMode(LandAnimation, false);
        SetLoopMode(Attack1Animation, false);
        SetLoopMode(Attack2Animation, false);
        SetLoopMode(Attack3Animation, false);
        SetLoopMode(DodgeAnimation, false);
        SetLoopMode(HitAnimation, false);
        SetLoopMode(DeathAnimation, false);
    }
    
    /// <summary>
    /// 根据动画名称获取动画速度
    /// </summary>
    public float GetAnimationSpeed(string animationName)
    {
        return animationName switch
        {
            "Idle" => IdleAnimationSpeed,
            "Walk" => WalkAnimationSpeed,
            "Run" => RunAnimationSpeed,
            "Sprint" => SprintAnimationSpeed,
            "JumpStart" => JumpStartAnimationSpeed,
            "JumpLoop" => JumpLoopAnimationSpeed,
            "Fall" => FallAnimationSpeed,
            "Land" => LandAnimationSpeed,
            "Attack1" => Attack1AnimationSpeed,
            "Attack2" => Attack2AnimationSpeed,
            "Attack3" => Attack3AnimationSpeed,
            "Dodge" => DodgeAnimationSpeed,
            "Hit" => HitAnimationSpeed,
            "Death" => DeathAnimationSpeed,
            _ => 1.0f // 默认速度
        };
    }
    
    private void SetLoopMode(Animation anim, bool shouldLoop)
    {
        if (anim != null)
        {
            anim.LoopMode = shouldLoop ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;
        }
    }
}
