using Godot;
using R3;

[GlobalClass]
public partial class DraggableItemComponent : Node
{
	#region Export Properties
	
	[Export] public NodePath ItemCellGroupController_Path { get; set; } = "%ItemCellGroupController";
	
	private ItemCellGroupController _itemCellGroupController;
	
	[Export] public NodePath StateChartPath { get; set; } = "%StateChart";
	
	public Node StateChart { get; private set; }
	
	// 拖拽开始事件名称（发送给 StateChart），可在编辑器中配置
	[Export] public string DragStartEventName { get; set; } = "drag_start";
	
	// 拖拽结束事件名称（发送给 StateChart），可在编辑器中配置
	[Export] public string DragEndEventName { get; set; } = "drag_end";
	
	#endregion
	
	#region R3 Reactive Streams
	
	public Subject<Unit> OnDragStartedAsObservable { get; private set; }
	
	/// 拖拽结束事件流
	/// 
	/// 触发时机：鼠标左键松开
	/// 
	/// 典型用途：
	/// - 尝试将物品放置到目标位置
	/// - 隐藏拖拽预览
	/// - 取消高亮可放置区域
	/// - 播放放置音效
	/// - 验证放置是否合法（通过 BackpackGridComponent）
	public Subject<Unit> OnDragEndedAsObservable { get; private set; }
	
	/// 旋转请求事件流
	/// 
	/// 触发时机：鼠标右键按下（通常在拖拽状态下）
	/// 
	/// 典型用途：
	/// - 调用 GridShapeComponent.Rotate90() 旋转物品
	/// - 更新拖拽预览的显示
	/// - 播放旋转音效
	/// - 重新检测放置合法性
	/// 
	/// 注意：
	/// - 此事件不会通知 StateChart（旋转是纯动作，不改变状态）
	/// - 订阅者需要自行判断是否在拖拽状态下响应旋转
	public Subject<Unit> OnRotateRequestedAsObservable { get; private set; }
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _EnterTree()
	{
		// 【架构修正】在 _EnterTree 中初始化所有 Subjects
		OnDragStartedAsObservable = new Subject<Unit>();
		OnDragEndedAsObservable = new Subject<Unit>();
		OnRotateRequestedAsObservable = new Subject<Unit>();
	}
	
	public override void _Ready()
	{
		// NodePath 在 _Ready 时已解析完成，直接初始化
		InitializeComponent();
	}
	
	/// 延迟初始化组件（在 NodePath 解析完成后）
	private void InitializeComponent()
	{
		// 解析 ItemCellGroupController 引用
		_itemCellGroupController = GetNodeOrNull<ItemCellGroupController>(ItemCellGroupController_Path);
		
		// 验证 ItemCellGroupController 引用
		if (_itemCellGroupController == null)
		{
			GD.PushError($"[{Name}] ItemCellGroupController not found at path: {ItemCellGroupController_Path}");
			return;
		}
		
		// 订阅 ItemCellGroupController 的聚合输入事件流
		_itemCellGroupController.OnGroupInputAsObservable
			.Subscribe(HandleGuiInput)
			.AddTo(this);
		
		StateChart = GetNodeOrNull<Node>(StateChartPath);
		if (StateChart == null)
		{
			GD.PushError($"[{Name}] StateChart not found: {StateChartPath}");
			return;
		}
	}
	
	public override void _ExitTree()
	{
		// R3 规则：释放所有 Subjects
		OnDragStartedAsObservable?.Dispose();
		OnDragEndedAsObservable?.Dispose();
		OnRotateRequestedAsObservable?.Dispose();
	}
	
	#endregion
	
	#region Input Handling
	
	/// 处理 GUI 输入事件
	/// 
	/// 目的：将原始鼠标事件转换为语义化的业务事件
	/// 
	/// 算法：
	/// 1. 判断事件类型（只处理 InputEventMouseButton）
	/// 2. 根据按键类型和状态分发到不同的处理逻辑
	/// 3. 触发对应的 R3 Subject 和 StateChart 事件
	private void HandleGuiInput(InputEvent @event)
	{
		// 只处理鼠标按键事件
		if (@event is not InputEventMouseButton mouseEvent)
			return;
		
		// 左键按下 → 开始拖拽
		if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
		{
			HandleDragStart();
		}
		// 左键松开 → 结束拖拽
		else if (mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)
		{
			HandleDragEnd();
		}
		// 右键按下 → 请求旋转
		else if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
		{
			HandleRotateRequest();
		}
	}
	
	/// 处理拖拽开始
	private void HandleDragStart()
	{
		// 【R3 响应式】通知订阅者
		OnDragStartedAsObservable.OnNext(Unit.Default);
		
		// 【StateChart】发送状态机事件
		if (StateChart != null)
		{
			// 使用可配置的事件名称，避免硬编码
			StateChart.Call("send_event", DragStartEventName);
		}
	}
	
	/// 处理拖拽结束
	private void HandleDragEnd()
	{
		// 【R3 响应式】通知订阅者
		OnDragEndedAsObservable.OnNext(Unit.Default);
		
		// 【StateChart】发送状态机事件
		if (StateChart != null)
		{
			// 使用可配置的事件名称，避免硬编码
			StateChart.Call("send_event", DragEndEventName);
		}
	}
	
	/// 处理旋转请求
	private void HandleRotateRequest()
	{
		// 【R3 响应式】通知订阅者
		// 注意：旋转是纯动作，不需要通知 StateChart
		OnRotateRequestedAsObservable.OnNext(Unit.Default);
	}
	
	#endregion
	
	#region Helper Methods
	
	/// 手动触发拖拽开始（用于程序化控制）
	public void TriggerDragStart()
	{
		HandleDragStart();
	}
	
	/// 手动触发拖拽结束（用于程序化控制）
	public void TriggerDragEnd()
	{
		HandleDragEnd();
	}
	
	#endregion
}
