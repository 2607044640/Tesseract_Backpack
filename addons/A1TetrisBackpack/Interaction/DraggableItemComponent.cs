using Godot;
using R3;

/// <summary>
/// 可拖拽物品组件 - 桥接 UI 输入与状态机
/// 
/// 职责：
/// - 监听 Control 节点的鼠标输入事件
/// - 将输入转换为状态机事件（drag_start / drag_end）
/// - 通过 R3 Subject 向外广播交互事件（供 UI 层订阅）
/// 
/// 架构定位：
/// - 输入层：接收原始鼠标事件
/// - 转换层：翻译为语义化的业务事件
/// - 广播层：通知状态机和 UI 系统
/// 
/// 与 StateChart 配合使用：
/// 
/// 推荐的 StateChart 结构：
/// ```
/// StateChart
/// └── Root (CompoundState, initial="Idle")
///     ├── Idle (AtomicState)
///     │   └── Transition: event="drag_start" → Dragging
///     └── Dragging (AtomicState)
///         ├── Transition: event="drag_end" → Idle
///         └── [可选] 拖拽相关组件（如 FollowMouseComponent）
/// ```
/// 
/// 状态流转：
/// 1. 用户按下左键 → 组件发送 "drag_start" → StateChart 切换到 Dragging 状态
/// 2. Dragging 状态激活 → 拖拽组件开始工作（跟随鼠标）
/// 3. 用户松开左键 → 组件发送 "drag_end" → StateChart 切换回 Idle 状态
/// 4. 用户在拖拽中按右键 → 组件发送旋转请求 → GridShapeComponent 执行旋转
/// 
/// 使用示例：
/// ```csharp
/// // 在 UI 控制器中订阅事件
/// var draggable = item.GetNode<DraggableItemComponent>("Draggable");
/// 
/// draggable.OnDragStartedAsObservable
///     .Subscribe(_ => {
///         GD.Print("开始拖拽");
///         ShowDragPreview();
///     })
///     .AddTo(disposables);
/// 
/// draggable.OnRotateRequestedAsObservable
///     .Subscribe(_ => {
///         var shape = item.GetNode<GridShapeComponent>("Shape");
///         shape.Rotate90();
///     })
///     .AddTo(disposables);
/// ```
/// </summary>
[GlobalClass]
public partial class DraggableItemComponent : Node
{
	#region Export Properties
	
	/// <summary>
	/// 单元格组控制器路径（提供聚合后的输入事件流）
	/// </summary>
	[Export] public NodePath ItemCellGroupController_Path { get; set; } = "%ItemCellGroupController";
	
	/// <summary>
	/// ItemCellGroupController引用
	/// </summary>
	private ItemCellGroupController _itemCellGroupController;
	
	/// <summary>
	/// 状态机节点路径
	/// </summary>
	[Export] public NodePath StateChartPath { get; set; } = "%StateChart";
	
	/// <summary>
	/// 状态机节点引用
	/// </summary>
	public Node StateChart { get; private set; }
	
	/// <summary>
	/// 拖拽开始事件名称（发送给 StateChart）
	/// 可在编辑器中配置，避免硬编码
	/// </summary>
	[Export] public string DragStartEventName { get; set; } = "drag_start";
	
	/// <summary>
	/// 拖拽结束事件名称（发送给 StateChart）
	/// 可在编辑器中配置，避免硬编码
	/// </summary>
	[Export] public string DragEndEventName { get; set; } = "drag_end";
	
	#endregion
	
	#region R3 Reactive Streams
	
	/// <summary>
	/// 拖拽开始事件流
	/// 
	/// 触发时机：鼠标左键按下
	/// 
	/// 典型用途：
	/// - 显示拖拽预览（半透明物品跟随鼠标）
	/// - 高亮可放置区域
	/// - 播放拾取音效
	/// - 记录拖拽起始位置
	/// </summary>
	public Subject<Unit> OnDragStartedAsObservable { get; private set; }
	
	/// <summary>
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
	/// </summary>
	public Subject<Unit> OnDragEndedAsObservable { get; private set; }
	
	/// <summary>
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
	/// </summary>
	public Subject<Unit> OnRotateRequestedAsObservable { get; private set; }
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		// 初始化 R3 Subjects
		OnDragStartedAsObservable = new Subject<Unit>();
		OnDragEndedAsObservable = new Subject<Unit>();
		OnRotateRequestedAsObservable = new Subject<Unit>();
		
		// 延迟初始化以等待 Godot 解析 NodePath
		CallDeferred(MethodName.InitializeComponent);
	}
	
	/// <summary>
	/// 延迟初始化组件（在 NodePath 解析完成后）
	/// </summary>
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
		
		GD.Print($"[{Name}] ItemCellGroupController 引用有效: {_itemCellGroupController.Name}");
		
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
		
		GD.Print($"[{Name}] DraggableItemComponent: 已订阅 ItemCellGroupController 的输入事件流");
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
	
	/// <summary>
	/// 处理 GUI 输入事件
	/// 
	/// 目的：将原始鼠标事件转换为语义化的业务事件
	/// 
	/// 算法：
	/// 1. 判断事件类型（只处理 InputEventMouseButton）
	/// 2. 根据按键类型和状态分发到不同的处理逻辑
	/// 3. 触发对应的 R3 Subject 和 StateChart 事件
	/// </summary>
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
	
	/// <summary>
	/// 处理拖拽开始
	/// </summary>
	private void HandleDragStart()
	{
		// 【R3 响应式】通知订阅者
		OnDragStartedAsObservable.OnNext(Unit.Default);
		
		// 【StateChart】发送状态机事件
		if (StateChart != null)
		{
			// 使用可配置的事件名称，避免硬编码
			StateChart.Call("send_event", DragStartEventName);
			GD.Print($"DraggableItemComponent: 发送状态事件 '{DragStartEventName}'");
		}
		
		GD.Print("DraggableItemComponent: 拖拽开始");
	}
	
	/// <summary>
	/// 处理拖拽结束
	/// </summary>
	private void HandleDragEnd()
	{
		// 【R3 响应式】通知订阅者
		OnDragEndedAsObservable.OnNext(Unit.Default);
		
		// 【StateChart】发送状态机事件
		if (StateChart != null)
		{
			// 使用可配置的事件名称，避免硬编码
			StateChart.Call("send_event", DragEndEventName);
			GD.Print($"DraggableItemComponent: 发送状态事件 '{DragEndEventName}'");
		}
		
		GD.Print("DraggableItemComponent: 拖拽结束");
	}
	
	/// <summary>
	/// 处理旋转请求
	/// </summary>
	private void HandleRotateRequest()
	{
		// 【R3 响应式】通知订阅者
		// 注意：旋转是纯动作，不需要通知 StateChart
		OnRotateRequestedAsObservable.OnNext(Unit.Default);
		
		GD.Print("DraggableItemComponent: 请求旋转");
	}
	
	#endregion
	
	#region Helper Methods
	
	/// <summary>
	/// 手动触发拖拽开始（用于程序化控制）
	/// </summary>
	public void TriggerDragStart()
	{
		HandleDragStart();
	}
	
	/// <summary>
	/// 手动触发拖拽结束（用于程序化控制）
	/// </summary>
	public void TriggerDragEnd()
	{
		HandleDragEnd();
	}
	
	#endregion
}
