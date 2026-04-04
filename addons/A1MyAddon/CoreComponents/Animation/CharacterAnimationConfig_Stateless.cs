using Godot;
using System.Collections.Generic;
using Stateless;

/// <summary>
/// 【实验组 B】角色动画配置 - 使用 Stateless 状态机重构
/// 
/// 重构目标：
/// - 消除 GetGroundAnimation/GetFlyAnimation 中的 if/else 瀑布流
/// - 用强类型状态机替代优先级判断（空中 > 冲刺 > 移动 > 静止）
/// - 保持与原版相同的 API 接口（GetAnimationForState）
/// 
/// 架构优势：
/// - 状态流转规则可视化（.Configure().Permit()）
/// - 编译时检查，零拼写错误
/// - 状态优先级通过状态机结构定义，不是 if/else 顺序
/// - 易于扩展新状态（如 Crouch, Slide）
/// </summary>
[GlobalClass]
public partial class CharacterAnimationConfig_Stateless : Resource
{
    #region State Machine Definition
    
    /// <summary>
    /// 【地面动画状态】强类型枚举
    /// </summary>
    private enum GroundAnimState
    {
        Idle,       // 静止
        Running,    // 跑步
        Sprinting,  // 冲刺
        Airborne    // 空中
    }
    
    /// <summary>
    /// 【飞行动画状态】强类型枚举
    /// </summary>
    private enum FlyAnimState
    {
        Idle,       // 悬停
        Moving,     // 移动
        Fast        // 快速飞行
    }
    
    /// <summary>
    /// 【地面状态触发器】
    /// </summary>
    private enum GroundTrigger
    {
        StartMove,      // 开始移动
        StartSprint,    // 开始冲刺
        SlowDown,       // 减速（从冲刺到跑步）
        StopMove,       // 停止移动
        LeaveGround,    // 离开地面
        TouchGround     // 接触地面
    }
    
    /// <summary>
    /// 【飞行状态触发器】
    /// </summary>
    private enum FlyTrigger
    {
        StartMove,      // 开始移动
        StartFast,      // 开始快速飞行
        SlowDown,       // 减速
        Stop            // 停止
    }
    
    private StateMachine<GroundAnimState, GroundTrigger> _groundMachine;
    private StateMachine<FlyAnimState, FlyTrigger> _flyMachine;
    
    #endregion

    [Export] public string CharacterName = "默认角色";
    
    #region 动画配置 (与原版完全相同)
    
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
    
    #region 公共API (与原版接口兼容)
    
    /// <summary>
    /// 将配置应用到 AnimationPlayer 并完成初始化
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
        
        // 3. 注册所有动画
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
        
        // 5. 【新增】初始化状态机
        InitializeStateMachines();
        
        // 6. 打印初始化日志
        int loadedCount = _animSpeeds.Count;
        GD.Print($"[AnimationConfig_Stateless] {CharacterName} 动画初始化完成 - 已加载 {loadedCount} 个动画");
        GD.Print($"[AnimationConfig_Stateless] 状态机已初始化 - Ground: {_groundMachine.State}, Fly: {_flyMachine.State}");
    }
    
    /// <summary>
    /// 检查动画是否存在
    /// </summary>
    public bool HasAnimation(string animName)
    {
        return _player != null && _player.HasAnimation(animName);
    }
    
    /// <summary>
    /// 获取动画播放速度
    /// </summary>
    public float GetAnimationSpeed(string animName)
    {
        return _animSpeeds.TryGetValue(animName, out float speed) ? speed : 1.0f;
    }
    
    /// <summary>
    /// 【核心API】根据状态和速度获取应该播放的动画
    /// 与原版接口完全兼容，但内部使用状态机
    /// </summary>
    public (string animName, float speed) GetAnimationForState(string mode, Vector3 velocity, bool isOnFloor)
    {
        if (_player == null) return ("", 1.0f);
        
        return mode switch
        {
            "Ground" => GetGroundAnimation_Stateless(velocity, isOnFloor),
            "Fly" => GetFlyAnimation_Stateless(velocity),
            _ => ("", 1.0f)
        };
    }
    
    #endregion
    
    #region State Machine Configuration
    
    /// <summary>
    /// 【核心】初始化状态机
    /// </summary>
    private void InitializeStateMachines()
    {
        // === 地面动画状态机 ===
        _groundMachine = new StateMachine<GroundAnimState, GroundTrigger>(GroundAnimState.Idle);
        
        _groundMachine.Configure(GroundAnimState.Idle)
            .Permit(GroundTrigger.StartMove, GroundAnimState.Running)
            .Permit(GroundTrigger.StartSprint, GroundAnimState.Sprinting)
            .Permit(GroundTrigger.LeaveGround, GroundAnimState.Airborne);
        
        _groundMachine.Configure(GroundAnimState.Running)
            .Permit(GroundTrigger.StopMove, GroundAnimState.Idle)
            .Permit(GroundTrigger.StartSprint, GroundAnimState.Sprinting)
            .Permit(GroundTrigger.LeaveGround, GroundAnimState.Airborne);
        
        _groundMachine.Configure(GroundAnimState.Sprinting)
            .Permit(GroundTrigger.SlowDown, GroundAnimState.Running)
            .Permit(GroundTrigger.StopMove, GroundAnimState.Idle)
            .Permit(GroundTrigger.LeaveGround, GroundAnimState.Airborne);
        
        _groundMachine.Configure(GroundAnimState.Airborne)
            .Permit(GroundTrigger.TouchGround, GroundAnimState.Idle);
        
        // === 飞行动画状态机 ===
        _flyMachine = new StateMachine<FlyAnimState, FlyTrigger>(FlyAnimState.Idle);
        
        _flyMachine.Configure(FlyAnimState.Idle)
            .Permit(FlyTrigger.StartMove, FlyAnimState.Moving)
            .Permit(FlyTrigger.StartFast, FlyAnimState.Fast);
        
        _flyMachine.Configure(FlyAnimState.Moving)
            .Permit(FlyTrigger.Stop, FlyAnimState.Idle)
            .Permit(FlyTrigger.StartFast, FlyAnimState.Fast);
        
        _flyMachine.Configure(FlyAnimState.Fast)
            .Permit(FlyTrigger.SlowDown, FlyAnimState.Moving)
            .Permit(FlyTrigger.Stop, FlyAnimState.Idle);
    }
    
    #endregion
    
    #region Animation Selection (Stateless Version)
    
    /// <summary>
    /// 【重构版】选择地面模式动画 - 使用状态机替代 if/else
    /// </summary>
    private (string, float) GetGroundAnimation_Stateless(Vector3 velocity, bool isOnFloor)
    {
        float horizontalSpeed = new Vector2(velocity.X, velocity.Z).Length();
        
        // 【状态机更新】根据物理状态触发状态切换
        UpdateGroundStateMachine(horizontalSpeed, isOnFloor);
        
        // 【零 if/else】直接根据当前状态返回动画
        return _groundMachine.State switch
        {
            GroundAnimState.Airborne => GetAnimOrFallback(AnimationNames.JumpStart),
            GroundAnimState.Sprinting => GetAnimOrFallback(AnimationNames.Sprint),
            GroundAnimState.Running => GetAnimOrFallback(AnimationNames.Run),
            GroundAnimState.Idle => GetAnimOrFallback(AnimationNames.Idle),
            _ => ("", 1.0f)
        };
    }
    
    /// <summary>
    /// 【重构版】选择飞行模式动画 - 使用状态机替代 if/else
    /// </summary>
    private (string, float) GetFlyAnimation_Stateless(Vector3 velocity)
    {
        float speed = velocity.Length();
        
        // 【状态机更新】根据速度触发状态切换
        UpdateFlyStateMachine(speed);
        
        // 【零 if/else】直接根据当前状态返回动画
        return _flyMachine.State switch
        {
            FlyAnimState.Fast => GetAnimOrFallback(AnimationNames.FlyFast, AnimationNames.Run),
            FlyAnimState.Moving => GetAnimOrFallback(AnimationNames.FlyMove, AnimationNames.Run),
            FlyAnimState.Idle => GetAnimOrFallback(AnimationNames.FlyIdle, AnimationNames.Idle),
            _ => ("", 1.0f)
        };
    }
    
    /// <summary>
    /// 更新地面状态机
    /// </summary>
    private void UpdateGroundStateMachine(float horizontalSpeed, bool isOnFloor)
    {
        // 空中状态优先级最高
        if (!isOnFloor)
        {
            if (_groundMachine.CanFire(GroundTrigger.LeaveGround))
                _groundMachine.Fire(GroundTrigger.LeaveGround);
            return;
        }
        
        // 落地检测
        if (_groundMachine.State == GroundAnimState.Airborne)
        {
            if (_groundMachine.CanFire(GroundTrigger.TouchGround))
                _groundMachine.Fire(GroundTrigger.TouchGround);
        }
        
        // 速度状态切换
        if (horizontalSpeed > SprintThreshold)
        {
            if (_groundMachine.CanFire(GroundTrigger.StartSprint))
                _groundMachine.Fire(GroundTrigger.StartSprint);
        }
        else if (horizontalSpeed > MoveThreshold)
        {
            if (_groundMachine.State == GroundAnimState.Sprinting && _groundMachine.CanFire(GroundTrigger.SlowDown))
                _groundMachine.Fire(GroundTrigger.SlowDown);
            else if (_groundMachine.State == GroundAnimState.Idle && _groundMachine.CanFire(GroundTrigger.StartMove))
                _groundMachine.Fire(GroundTrigger.StartMove);
        }
        else
        {
            if (_groundMachine.CanFire(GroundTrigger.StopMove))
                _groundMachine.Fire(GroundTrigger.StopMove);
        }
    }
    
    /// <summary>
    /// 更新飞行状态机
    /// </summary>
    private void UpdateFlyStateMachine(float speed)
    {
        if (speed > FlyFastThreshold)
        {
            if (_flyMachine.CanFire(FlyTrigger.StartFast))
                _flyMachine.Fire(FlyTrigger.StartFast);
        }
        else if (speed > MoveThreshold)
        {
            if (_flyMachine.State == FlyAnimState.Fast && _flyMachine.CanFire(FlyTrigger.SlowDown))
                _flyMachine.Fire(FlyTrigger.SlowDown);
            else if (_flyMachine.State == FlyAnimState.Idle && _flyMachine.CanFire(FlyTrigger.StartMove))
                _flyMachine.Fire(FlyTrigger.StartMove);
        }
        else
        {
            if (_flyMachine.CanFire(FlyTrigger.Stop))
                _flyMachine.Fire(FlyTrigger.Stop);
        }
    }
    
    /// <summary>
    /// 获取动画或使用 fallback
    /// </summary>
    private (string, float) GetAnimOrFallback(string primaryAnim, string fallbackAnim = null)
    {
        if (HasAnimation(primaryAnim))
            return (primaryAnim, GetAnimationSpeed(primaryAnim));
        
        if (fallbackAnim != null && HasAnimation(fallbackAnim))
            return (fallbackAnim, GetAnimationSpeed(fallbackAnim));
        
        return ("", 1.0f);
    }
    
    #endregion
    
    #region 私有方法
    
    /// <summary>
    /// 注册动画
    /// </summary>
    private void RegisterAnim(AnimationLibrary lib, string name, Animation anim, float speed, bool isLoop)
    {
        if (anim == null) return;
        
        anim.LoopMode = isLoop ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;
        lib.AddAnimation(name, anim);
        _animSpeeds[name] = speed;
    }
    
    #endregion
    
    #region Debug Helpers
    
    /// <summary>
    /// 【调试工具】获取当前状态机状态
    /// </summary>
    public string GetStateMachineInfo()
    {
        return $"Ground: {_groundMachine?.State}, Fly: {_flyMachine?.State}";
    }
    
    #endregion
}
