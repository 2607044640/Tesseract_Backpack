using Godot;
using Godot.Composition;
using System;
using R3;

/// <summary>
/// 输入组件抽象基类 - 实现依赖倒置原则
/// MovementComponent 等执行组件可复用于玩家和 AI
/// 现已支持 R3 响应式编程
/// </summary>
[GlobalClass]
[Component(typeof(CharacterBody3D))]
public abstract partial class BaseInputComponent : Node
{
    #region Events (传统事件接口 - 向后兼容)
    
    /// <summary>
    /// 移动输入事件 (WASD/方向键 或 AI 决策)
    /// Vector2: X = 左右 (-1 到 1), Y = 前后 (-1 到 1)
    /// </summary>
    public event Action<Vector2> OnMovementInput;
    
    /// <summary>
    /// 跳跃按键刚按下事件（或 AI 决定跳跃）
    /// </summary>
    public event Action OnJumpJustPressed;
    
    #endregion

    #region R3 Reactive Streams (响应式数据流)
    
    private readonly Subject<Vector2> _moveSubject = new();
    private readonly Subject<Unit> _jumpSubject = new();
    
    /// <summary>
    /// 移动输入流 (R3 Observable)
    /// </summary>
    public Observable<Vector2> MoveStream => _moveSubject;
    
    /// <summary>
    /// 跳跃输入流 (R3 Observable)
    /// </summary>
    public Observable<Unit> JumpStream => _jumpSubject;
    
    #endregion

    #region Export Properties
    
    /// <summary>
    /// 是否启用输入处理
    /// </summary>
    [Export] public bool InputEnabled { get; set; } = true;
    
    #endregion

    #region Godot Lifecycle
    
    public override void _Ready()
    {
        InitializeComponent();
        GD.Print($"BaseInputComponent: 已初始化 ({GetType().Name}) - R3 支持已启用");
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _moveSubject?.Dispose();
            _jumpSubject?.Dispose();
        }
        base.Dispose(disposing);
    }
    
    #endregion

    #region Protected Methods (供子类触发事件)
    
    /// <summary>
    /// 触发移动输入事件
    /// 子类在检测到移动输入时调用此方法
    /// </summary>
    protected void TriggerMovementInput(Vector2 direction)
    {
        // 触发传统事件（向后兼容）
        OnMovementInput?.Invoke(direction);
        
        // 推送到 R3 流
        _moveSubject.OnNext(direction);
    }
    
    /// <summary>
    /// 触发跳跃输入事件
    /// 子类在检测到跳跃输入时调用此方法
    /// </summary>
    protected void TriggerJumpInput()
    {
        // 触发传统事件（向后兼容）
        OnJumpJustPressed?.Invoke();
        
        // 推送到 R3 流
        _jumpSubject.OnNext(Unit.Default);
    }
    
    #endregion
}
