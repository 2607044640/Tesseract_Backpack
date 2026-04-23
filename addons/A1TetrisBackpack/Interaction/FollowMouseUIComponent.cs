using Godot;
using GodotStateCharts;

/// UI 跟随鼠标组件 - 拖拽状态下的物理跟随
/// 
/// 职责：
/// - 让目标 UI 节点实时跟随鼠标位置
/// - 管理拖拽时的 ZIndex 提升（确保在最上层显示）
/// - 通过 StateChart 生命周期自动控制启停
/// 
/// 架构定位：
/// - 挂载位置：StateChart 的 Dragging 状态节点下（AtomicState）
/// - 生命周期：由父状态控制（Power Switch 模式）
/// - 处理时机：仅在 Dragging 状态激活时运行 _Process
/// 
/// StateChart 集成（Power Switch 模式）：
/// 
/// 推荐的节点结构：
/// ```
/// ItemEntity (Control)
/// ├── StateChart
/// │   └── Root (CompoundState, initial="Idle")
/// │       ├── Idle (AtomicState)
/// │       │   └── Transition: event="drag_start" → Dragging
/// │       └── Dragging (AtomicState)
/// │           ├── FollowMouseUIComponent ← 本组件
/// │           └── Transition: event="drag_end" → Idle
/// └── [其他 UI 子节点]
/// ```
/// 
/// 工作流程：
/// 1. 初始状态：组件处于休眠（_Process 禁用）
/// 2. 用户按下左键 → DraggableItemComponent 发送 "drag_start"
/// 3. StateChart 切换到 Dragging 状态
/// 4. Dragging.state_entered 信号触发 → AutoBindToParentState 自动启用 _Process
/// 5. 组件开始工作：提升 ZIndex + 每帧跟随鼠标
/// 6. 用户松开左键 → 发送 "drag_end" → 切换回 Idle
/// 7. Dragging.state_exited 信号触发 → 自动禁用 _Process + 恢复 ZIndex
/// 
/// 关键优势：
/// - 零状态判断：无需 `if (isDragging)` 检查
/// - 自动生命周期：状态机控制组件启停
/// - 解耦设计：组件不知道何时被激活，只负责"跟随鼠标"这一件事
/// 
/// 使用示例：
/// ```csharp
/// // 在编辑器中：
/// // 1. 将 FollowMouseUIComponent 添加为 Dragging 状态的子节点
/// // 2. 设置 TargetUI 为物品的根 Control 节点
/// // 3. 可选：调整 GrabOffset 以优化抓取手感
/// 
/// // 无需任何代码！组件会自动工作。
/// ```
[GlobalClass]
public partial class FollowMouseUIComponent : Node
{
	#region Export Properties
	
	/// 目标 UI 节点路径
	[Export] public NodePath TargetUIPath { get; set; } = "%TargetUI";
	
	/// 目标 UI 节点引用
	public Control TargetUI { get; private set; }
	
	/// 抓取偏移量（鼠标相对于物品左上角的偏移）
	/// 
	/// 用途：
	/// - 保持鼠标在点击位置（而不是跳到左上角）
	/// - 提升拖拽手感和精确度
	/// 
	/// 计算方式：
	/// 在 DraggableItemComponent.HandleDragStart() 中：
	/// ```csharp
	/// var localMousePos = TargetUI.GetLocalMousePosition();
	/// followComponent.GrabOffset = -localMousePos;
	/// ```
	/// 
	/// 示例：
	/// - 鼠标点击物品中心 → GrabOffset = (-width/2, -height/2)
	/// - 鼠标点击左上角 → GrabOffset = (0, 0)
	[Export] public Vector2 GrabOffset { get; set; } = Vector2.Zero;
	
	#endregion
	
	#region Private Fields
	
	/// 原始 ZIndex（用于退出拖拽时恢复）
	private int _originalZIndex;
	
	/// 拖拽时的 ZIndex 提升值
	private const int DragZIndexBoost = 100;
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		// NodePath 在 _Ready 时已解析完成，直接初始化
		InitializeComponent();
	}
	
	/// 延迟初始化组件（在 NodePath 解析完成后）
	private void InitializeComponent()
	{
		TargetUI = GetNodeOrNull<Control>(TargetUIPath);
		if (TargetUI == null)
		{
			GD.PushError($"[{Name}] TargetUI not found: {TargetUIPath}");
			return;
		}
		
		// 保存原始 ZIndex
		_originalZIndex = TargetUI.ZIndex;
		
		// 【Power Switch 模式】自动绑定到父状态
		// 该方法会：
		// 1. 禁用所有处理（Process, PhysicsProcess, Input 等）
		// 2. 连接父状态的 state_entered 信号 → 启用处理
		// 3. 连接父状态的 state_exited 信号 → 禁用处理
		this.AutoBindToParentState();
		
		// 手动连接状态信号以管理 ZIndex
		var parentState = GetParent();
		if (parentState != null)
		{
			var state = StateChartState.Of(parentState);
			if (state != null)
			{
				// 状态进入 → 提升 ZIndex
				state.Connect(StateChartState.SignalName.StateEntered, Callable.From(OnDragStateEntered));
				
				// 状态退出 → 恢复 ZIndex
				state.Connect(StateChartState.SignalName.StateExited, Callable.From(OnDragStateExited));
			}
		}
		
		GD.Print($"FollowMouseUIComponent: 已初始化并绑定到父状态 '{GetParent()?.Name}'");
	}
	
	public override void _Process(double delta)
	{
		// 注意：此方法仅在 Dragging 状态激活时运行
		// AutoBindToParentState 会自动控制 SetProcess 的启停
		
		if (TargetUI == null)
			return;
		
		// 获取全局鼠标位置
		Vector2 mousePos = TargetUI.GetGlobalMousePosition();
		
		// 应用偏移量并更新位置
		TargetUI.GlobalPosition = mousePos + GrabOffset;
	}
	
	#endregion
	
	#region State Event Handlers
	
	/// 拖拽状态进入时的处理
	private void OnDragStateEntered()
	{
		if (TargetUI == null)
			return;
		
		// 提升 ZIndex 确保在最上层显示
		TargetUI.ZIndex = _originalZIndex + DragZIndexBoost;
		
		GD.Print($"FollowMouseUIComponent: 拖拽开始，ZIndex {_originalZIndex} → {TargetUI.ZIndex}");
	}
	
	/// 拖拽状态退出时的处理
	private void OnDragStateExited()
	{
		if (TargetUI == null)
			return;
		
		// 恢复原始 ZIndex
		TargetUI.ZIndex = _originalZIndex;
		
		GD.Print($"FollowMouseUIComponent: 拖拽结束，ZIndex 恢复为 {_originalZIndex}");
	}
	
	#endregion
	
	#region Helper Methods
	
	/// 设置抓取偏移量（通常在拖拽开始时调用）
	public void SetGrabOffset(Vector2 offset)
	{
		GrabOffset = offset;
	}
	
	/// 根据当前鼠标位置自动计算抓取偏移
	public void CalculateGrabOffsetFromMouse()
	{
		if (TargetUI == null)
			return;
		
		// 计算鼠标相对于 TargetUI 的局部位置
		Vector2 localMousePos = TargetUI.GetLocalMousePosition();
		
		// 设置偏移量（负值，因为我们要让鼠标保持在点击位置）
		GrabOffset = -localMousePos;
		
		GD.Print($"FollowMouseUIComponent: 自动计算偏移量 = {GrabOffset}");
	}
	
	/// 设置拖拽时的 ZIndex 提升值（可选，用于自定义层级）
	public void SetDragZIndexBoost(int boost)
	{
		// 注意：这是一个实例方法，但 DragZIndexBoost 是常量
		// 如果需要动态调整，应该将 DragZIndexBoost 改为实例字段
		GD.PushWarning("FollowMouseUIComponent: SetDragZIndexBoost 当前未实现（DragZIndexBoost 是常量）");
	}
	
	#endregion
}
