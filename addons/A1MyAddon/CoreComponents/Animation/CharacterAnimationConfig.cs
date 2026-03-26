using Godot;
using System.Collections.Generic;

/// <summary>
/// 动画名称常量（合并在同一个文件顶部，方便查看）
/// </summary>
public static class AnimationNames
{
    #region 基础移动动画
    
    public const string Idle = "Idle";
    public const string Walk = "Walk";
    public const string Run = "Run";
    public const string Sprint = "Sprint";
    
    #endregion

    #region 跳跃和空中动画
    
    public const string JumpStart = "JumpStart";
    public const string JumpLoop = "JumpLoop";
    public const string Fall = "Fall";
    public const string Land = "Land";
    
    #endregion

    #region 飞行动画
    
    public const string FlyIdle = "FlyIdle";
    public const string FlyMove = "FlyMove";
    public const string FlyFast = "FlyFast";
    
    #endregion

    #region 战斗动画
    
    public const string Attack1 = "Attack1";
    public const string Attack2 = "Attack2";
    public const string Attack3 = "Attack3";
    
    #endregion

    #region 其他动画
    
    public const string Dodge = "Dodge";
    public const string Hit = "Hit";
    public const string Death = "Death";
    
    #endregion
}

/// <summary>
/// 角色动画配置中心 - 极简架构
/// 合并了原来的 AnimationNames, AnimationSet, CharacterAnimationConfig 三个文件
/// 
/// 添加新动画只需2步：
/// 1. 声明 [Export] 变量
/// 2. 在 ApplyAndInitialize 中调用 RegisterAnim
/// </summary>
[GlobalClass]
public partial class CharacterAnimationConfig : Resource
{
    [Export] public string CharacterName = "默认角色";
    
    #region 动画配置 (在 Inspector 中暴露)
    
    [ExportGroup("基础移动动画")]
    [Export] public Animation IdleAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float IdleSpeed = 1.0f;
    
    [Export] public Animation WalkAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float WalkSpeed = 1.0f;
    
    [Export] public Animation RunAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float RunSpeed = 1.0f;
    
    [Export] public Animation SprintAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float SprintSpeed = 1.0f;
    
    [ExportGroup("跳跃和空中动画")]
    [Export] public Animation JumpStartAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float JumpStartSpeed = 1.0f;
    
    [Export] public Animation JumpLoopAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float JumpLoopSpeed = 1.0f;
    
    [Export] public Animation FallAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float FallSpeed = 1.0f;
    
    [Export] public Animation LandAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float LandSpeed = 1.0f;
    
    [ExportGroup("飞行动画")]
    [Export] public Animation FlyIdleAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float FlyIdleSpeed = 1.0f;
    
    [Export] public Animation FlyMoveAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float FlyMoveSpeed = 1.0f;
    
    [Export] public Animation FlyFastAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float FlyFastSpeed = 1.0f;
    
    [ExportGroup("战斗动画")]
    [Export] public Animation Attack1Animation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float Attack1Speed = 1.0f;
    
    [Export] public Animation Attack2Animation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float Attack2Speed = 1.0f;
    
    [Export] public Animation Attack3Animation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float Attack3Speed = 1.0f;
    
    [ExportGroup("其他动画")]
    [Export] public Animation DodgeAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float DodgeSpeed = 1.0f;
    
    [Export] public Animation HitAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float HitSpeed = 1.0f;
    
    [Export] public Animation DeathAnimation;
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float DeathSpeed = 1.0f;
    
    [ExportGroup("速度阈值")]
    [Export(PropertyHint.Range, "0.0,2.0,0.1")]
    public float MoveThreshold = 0.1f;
    
    [Export(PropertyHint.Range, "0.0,20.0,0.5")]
    public float SprintThreshold = 6.0f;
    
    [Export(PropertyHint.Range, "0.0,20.0,0.5")]
    public float FlyFastThreshold = 10.0f;
    
    #endregion
    
    #region 内部运行时缓存
    
    private AnimationPlayer _player;
    private Dictionary<string, float> _animSpeeds = new();
    
    #endregion
    
    #region 公共API
    
    /// <summary>
    /// 将配置应用到 AnimationPlayer 并完成初始化（一键搞定）
    /// 替代原来的 ApplyToAnimationPlayer + AnimationSet.Initialize
    /// </summary>
    public void ApplyAndInitialize(AnimationPlayer player)
    {
        if (player == null) return;
        
        _player = player;
        _animSpeeds.Clear();
        
        // 1. 清理现有库
        var libraries = player.GetAnimationLibraryList();
        foreach (var libName in libraries)
        {
            player.RemoveAnimationLibrary(libName);
        }
        
        // 2. 创建新库
        var library = new AnimationLibrary();
        
        // 3. 一站式注册所有动画 (一行代码搞定：添加、设置循环、缓存速度)
        RegisterAnim(library, AnimationNames.Idle, IdleAnimation, IdleSpeed, isLoop: true);
        RegisterAnim(library, AnimationNames.Walk, WalkAnimation, WalkSpeed, isLoop: true);
        RegisterAnim(library, AnimationNames.Run, RunAnimation, RunSpeed, isLoop: true);
        RegisterAnim(library, AnimationNames.Sprint, SprintAnimation, SprintSpeed, isLoop: true);
        
        RegisterAnim(library, AnimationNames.JumpStart, JumpStartAnimation, JumpStartSpeed, isLoop: false);
        RegisterAnim(library, AnimationNames.JumpLoop, JumpLoopAnimation, JumpLoopSpeed, isLoop: true);
        RegisterAnim(library, AnimationNames.Fall, FallAnimation, FallSpeed, isLoop: true);
        RegisterAnim(library, AnimationNames.Land, LandAnimation, LandSpeed, isLoop: false);
        
        RegisterAnim(library, AnimationNames.FlyIdle, FlyIdleAnimation, FlyIdleSpeed, isLoop: true);
        RegisterAnim(library, AnimationNames.FlyMove, FlyMoveAnimation, FlyMoveSpeed, isLoop: true);
        RegisterAnim(library, AnimationNames.FlyFast, FlyFastAnimation, FlyFastSpeed, isLoop: true);
        
        RegisterAnim(library, AnimationNames.Attack1, Attack1Animation, Attack1Speed, isLoop: false);
        RegisterAnim(library, AnimationNames.Attack2, Attack2Animation, Attack2Speed, isLoop: false);
        RegisterAnim(library, AnimationNames.Attack3, Attack3Animation, Attack3Speed, isLoop: false);
        
        RegisterAnim(library, AnimationNames.Dodge, DodgeAnimation, DodgeSpeed, isLoop: false);
        RegisterAnim(library, AnimationNames.Hit, HitAnimation, HitSpeed, isLoop: false);
        RegisterAnim(library, AnimationNames.Death, DeathAnimation, DeathSpeed, isLoop: false);
        
        // 4. 应用库
        player.AddAnimationLibrary("", library);
        
        // 5. 打印初始化日志
        int loadedCount = _animSpeeds.Count;
        GD.Print($"[AnimationConfig] {CharacterName} 动画初始化完成 - 已加载 {loadedCount} 个动画");
        
        // 打印飞行动画状态
        bool hasFlyIdle = HasAnimation(AnimationNames.FlyIdle);
        bool hasFlyMove = HasAnimation(AnimationNames.FlyMove);
        bool hasFlyFast = HasAnimation(AnimationNames.FlyFast);
        GD.Print($"[AnimationConfig] 飞行动画: Idle={hasFlyIdle}, Move={hasFlyMove}, Fast={hasFlyFast}");
    }
    
    /// <summary>
    /// 检查动画是否存在
    /// </summary>
    public bool HasAnimation(string animName)
    {
        return _player != null && _player.HasAnimation(animName);
    }
    
    /// <summary>
    /// 获取动画播放速度（替代原来几百行的 switch 语句）
    /// </summary>
    public float GetAnimationSpeed(string animName)
    {
        return _animSpeeds.TryGetValue(animName, out float speed) ? speed : 1.0f;
    }
    
    /// <summary>
    /// 根据状态和速度获取应该播放的动画
    /// </summary>
    public (string animName, float speed) GetAnimationForState(string mode, Vector3 velocity, bool isOnFloor)
    {
        if (_player == null) return ("", 1.0f);
        
        return mode switch
        {
            "Ground" => GetGroundAnimation(velocity, isOnFloor),
            "Fly" => GetFlyAnimation(velocity),
            _ => ("", 1.0f)
        };
    }
    
    #endregion
    
    #region 私有方法
    
    /// <summary>
    /// 核心改进：内部辅助注册方法
    /// 一行代码完成：设置循环模式 + 添加到库 + 缓存速度
    /// </summary>
    private void RegisterAnim(AnimationLibrary lib, string name, Animation anim, float speed, bool isLoop)
    {
        if (anim == null) return;
        
        // 自动设置循环模式
        anim.LoopMode = isLoop ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;
        
        // 添加到动画库
        lib.AddAnimation(name, anim);
        
        // 存入速度字典（消除长长的 switch 语句）
        _animSpeeds[name] = speed;
    }
    
    /// <summary>
    /// 选择地面模式动画
    /// </summary>
    private (string, float) GetGroundAnimation(Vector3 velocity, bool isOnFloor)
    {
        float horizontalSpeed = new Vector2(velocity.X, velocity.Z).Length();
        
        // 优先级：空中 > 冲刺 > 移动 > 静止
        if (!isOnFloor && HasAnimation(AnimationNames.JumpStart))
        {
            return (AnimationNames.JumpStart, GetAnimationSpeed(AnimationNames.JumpStart));
        }
        
        if (horizontalSpeed > SprintThreshold && HasAnimation(AnimationNames.Sprint))
        {
            return (AnimationNames.Sprint, GetAnimationSpeed(AnimationNames.Sprint));
        }
        
        if (horizontalSpeed > MoveThreshold && HasAnimation(AnimationNames.Run))
        {
            return (AnimationNames.Run, GetAnimationSpeed(AnimationNames.Run));
        }
        
        if (HasAnimation(AnimationNames.Idle))
        {
            return (AnimationNames.Idle, GetAnimationSpeed(AnimationNames.Idle));
        }
        
        return ("", 1.0f);
    }
    
    /// <summary>
    /// 选择飞行模式动画
    /// </summary>
    private (string, float) GetFlyAnimation(Vector3 velocity)
    {
        float speed = velocity.Length();
        
        // 优先级：快速飞行 > 移动 > 静止
        if (speed > FlyFastThreshold && HasAnimation(AnimationNames.FlyFast))
        {
            return (AnimationNames.FlyFast, GetAnimationSpeed(AnimationNames.FlyFast));
        }
        
        if (speed > MoveThreshold && HasAnimation(AnimationNames.FlyMove))
        {
            return (AnimationNames.FlyMove, GetAnimationSpeed(AnimationNames.FlyMove));
        }
        
        if (HasAnimation(AnimationNames.FlyIdle))
        {
            return (AnimationNames.FlyIdle, GetAnimationSpeed(AnimationNames.FlyIdle));
        }
        
        // 如果没有飞行动画，使用地面动画作为fallback
        if (speed > MoveThreshold && HasAnimation(AnimationNames.Run))
        {
            return (AnimationNames.Run, GetAnimationSpeed(AnimationNames.Run));
        }
        
        if (HasAnimation(AnimationNames.Idle))
        {
            return (AnimationNames.Idle, GetAnimationSpeed(AnimationNames.Idle));
        }
        
        return ("", 1.0f);
    }
    
    #endregion
}
