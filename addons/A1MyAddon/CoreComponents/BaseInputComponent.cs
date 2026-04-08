using Godot;
using Godot.Composition;
using System;

/// <summary>
/// 输入组件抽象基类 - 实现依赖倒置原则
/// MovementComponent 等执行组件可以复用于玩家和 AI
/// </summary>
[GlobalClass]
[Component(typeof(CharacterBody3D))]
public abstract partial class BaseInputComponent : Node
{
    #region Events (输入事件接口)
    
    // 移动输入事件 (WASD/方向键 或 AI 决策)
    // Vector2: X = 左右 (-1 到 1), Y = 前后 (-1 到 1)
    public event Action<Vector2> OnMovementInput;
    
    // 跳跃按键刚按下事件（或 AI 决定跳跃）
    public event Action OnJumpJustPressed;
    
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
    
    #endregion

    #region Protected Methods (供子类触发事件)
    
    protected void TriggerMovementInput(Vector2 direction)
    {
        OnMovementInput?.Invoke(direction);
    }
    
    protected void TriggerJumpInput()
    {
        OnJumpJustPressed?.Invoke();
    }
    
    #endregion
}
