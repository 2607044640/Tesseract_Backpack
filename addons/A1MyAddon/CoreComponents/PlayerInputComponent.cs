using Godot;

/// <summary>
/// 玩家输入组件 - 读取玩家键盘/手柄输入并通过事件向外广播
/// 继承 BaseInputComponent，实现玩家特定的输入逻辑
/// 
/// 【架构原则 - 事件驱动】
/// 1. 只负责读取输入并触发事件
/// 2. 不判断当前状态（如是否在地面/飞行）
/// 3. 所有状态判断由 StateChart 处理
/// </summary>
[GlobalClass]
public partial class PlayerInputComponent : BaseInputComponent
{
    #region Godot Lifecycle
    
    public override void _Ready()
    {
        base._Ready(); // 调用基类的 InitializeComponent
        GD.Print($"PlayerInputComponent: 已初始化，InputEnabled={InputEnabled}");
    }
    
    public override void _Process(double delta)
    {
        if (!InputEnabled) return;
        
        // 读取移动输入
        Vector2 inputDir = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_backward"
        );
        
        // 触发移动输入事件
        TriggerMovementInput(inputDir);
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!InputEnabled) return;
        
        // 【事件驱动】跳跃/上升输入
        // 不判断当前是地面还是飞行模式
        // 地面模式：GroundMovementComponent 会处理跳跃
        // 飞行模式：FlyMovementComponent 会处理上升
        if (Input.IsActionJustPressed("jump"))
        {
            TriggerJumpInput();
        }

        // 【事件驱动】切换飞行模式（F 键）
        // 无脑发送 "toggle_fly" 事件给 StateChart
        // StateChart 会根据当前状态决定是否切换
        if (Input.IsActionJustPressed("toggle_fly"))
        {
            GetParent().SendStateEvent("toggle_fly");
            GD.Print("PlayerInputComponent: 发送状态事件 'toggle_fly'");
        }

        // 【事件驱动】交互键（E 键）
        if (Input.IsActionJustPressed("interact"))
        {
            GetParent().SendStateEvent("interact");
            GD.Print("PlayerInputComponent: 发送状态事件 'interact'");
        }
    }
    
    #endregion
}
