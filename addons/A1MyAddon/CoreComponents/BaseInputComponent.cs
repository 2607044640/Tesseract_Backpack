using Godot;
using Godot.Composition;
using System;
using R3;

/// <summary>
/// 输入组件抽象基类 - 实现依赖倒置原则
/// MovementComponent 等执行组件可以复用于玩家和 AI
/// R3 增强版：使用 Subject 替代 event，支持响应式编程
/// </summary>
[GlobalClass]
[Component(typeof(CharacterBody3D))]
public abstract partial class BaseInputComponent : Node
{
    #region R3 Reactive Streams (输入事件流)
    
    /// <summary>
    /// 移动输入流 (WASD/方向键 或 AI 决策)
    /// Vector2: X = 左右 (-1 到 1), Y = 前后 (-1 到 1)
    /// </summary>
    public Subject<Vector2> MovementInput { get; } = new();
    
    /// <summary>
    /// 跳跃输入流（按键刚按下或 AI 决定跳跃）
    /// </summary>
    public Subject<Unit> JumpPressed { get; } = new();
    
    #endregion

    #region Export Properties
    
    [Export] public bool InputEnabled { get; set; } = true;
    
    #endregion

    #region Godot Lifecycle
    
    public override void _Ready()
    {
        InitializeComponent();
        GD.Print($"BaseInputComponent: 已初始化 ({GetType().Name})");
    }
    
    public override void _ExitTree()
    {
        // 清理 R3 资源
        MovementInput.Dispose();
        JumpPressed.Dispose();
    }
    
    #endregion

    #region Protected Methods (供子类触发输入流)
    
    /// <summary>
    /// 触发移动输入流
    /// </summary>
    protected void TriggerMovementInput(Vector2 direction)
    {
        MovementInput.OnNext(direction);
    }
    
    /// <summary>
    /// 触发跳跃输入流
    /// </summary>
    protected void TriggerJumpInput()
    {
        JumpPressed.OnNext(Unit.Default);
    }
    
    #endregion
}
