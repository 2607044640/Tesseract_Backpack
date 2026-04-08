using Godot;

/// <summary>
/// 玩家输入组件 - 读取键盘/手柄输入并通过事件广播
/// 继承 BaseInputComponent，实现玩家特定的输入逻辑
/// </summary>
[GlobalClass]
public partial class PlayerInputComponent : BaseInputComponent
{
    #region Godot Lifecycle
    
    public override void _Ready()
    {
        base._Ready();
        GD.Print($"PlayerInputComponent: 已初始化，InputEnabled={InputEnabled}");
    }
    
    public override void _Process(double delta)
    {
        if (!InputEnabled) return;
        
        Vector2 inputDir = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_backward"
        );
        
        TriggerMovementInput(inputDir);
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!InputEnabled) return;
        
        if (Input.IsActionJustPressed("jump"))
        {
            TriggerJumpInput();
        }

        if (Input.IsActionJustPressed("toggle_fly"))
        {
            GetParent().SendStateEvent("toggle_fly");
            GD.Print("PlayerInputComponent: 发送状态事件 'toggle_fly'");
        }

        if (Input.IsActionJustPressed("interact"))
        {
            GetParent().SendStateEvent("interact");
            GD.Print("PlayerInputComponent: 发送状态事件 'interact'");
        }
    }
    
    #endregion
}
