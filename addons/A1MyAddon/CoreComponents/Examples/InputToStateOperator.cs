using Godot;
using Godot.Composition;

/// <summary>
/// 输入到状态的"接线员"组件
/// 演示如何将输入事件转换为状态机事件，实现完美解耦
/// 
/// 架构模式：
/// Input → InputComponent → InputToStateOperator → StateChart → Components
/// 
/// 优势：
/// 1. 输入组件不知道状态机的存在
/// 2. 状态机不知道输入的来源
/// 3. 其他组件只关心状态，不关心输入
/// </summary>
[GlobalClass]
[Component(typeof(CharacterBody3D))]
[ComponentDependency(typeof(BaseInputComponent))]
public partial class InputToStateOperator : Node
{
    #region Private Fields

    private BaseInputComponent _inputComponent;

    #endregion

    #region Godot Lifecycle

    public override void _Ready()
    {
        InitializeComponent();
        GD.Print("InputToStateOperator: 已初始化");
    }

    /// <summary>
    /// Entity 初始化完成后自动调用
    /// 在这里订阅输入事件并转发到状态机
    /// </summary>
    public void OnEntityReady()
    {
        // 订阅输入事件
        if (_inputComponent != null)
        {
            _inputComponent.OnJumpJustPressed += HandleJumpInput;
            _inputComponent.OnMovementInput += HandleMovementInput;
            GD.Print("✓ InputToStateOperator: 已订阅输入事件");
        }
    }

    public override void _ExitTree()
    {
        // 取消订阅防止内存泄漏
        if (_inputComponent != null)
        {
            _inputComponent.OnJumpJustPressed -= HandleJumpInput;
            _inputComponent.OnMovementInput -= HandleMovementInput;
        }
    }

    #endregion

    #region Input to State Event Mapping

    /// <summary>
    /// 跳跃输入 → 发送 "jump_pressed" 事件到状态机
    /// </summary>
    private void HandleJumpInput()
    {
        // 使用扩展方法，一行代码发送状态事件！
        parent.SendStateEvent("jump_pressed");
        GD.Print("→ 状态事件: jump_pressed");
    }

    /// <summary>
    /// 移动输入 → 根据输入方向发送状态事件
    /// 演示如何根据输入数据触发不同的状态转换
    /// </summary>
    private void HandleMovementInput(Vector2 inputDir)
    {
        if (inputDir.Length() > 0.1f)
        {
            // 有移动输入 → 进入移动状态
            parent.SendStateEvent("start_moving");
        }
        else
        {
            // 无移动输入 → 进入待机状态
            parent.SendStateEvent("stop_moving");
        }
    }

    #endregion
}
