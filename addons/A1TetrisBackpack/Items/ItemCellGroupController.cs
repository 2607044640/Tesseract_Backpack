using Godot;
using R3;
using System.Collections.Generic;

/// 物品单元格组控制器 - GridCellUI集合管理器 + 事件聚合器
/// 目的：管理物品形状对应的GridCellUI实例集合，并将所有单元格的输入事件聚合到单一R3流
/// 示例：L形 [(0,0), (0,1), (1,1)] -> 生成3个GridCellUI，聚合它们的GuiInput事件
/// 算法：1. 监听GridShapeComponent形状变化 -> 2. 重建GridCellUI集合 -> 3. 聚合事件流
///
/// 架构模式：Event Aggregator + Controller
/// 关键突破：完全绕过AABB问题，每个GridCellUI独立响应，无需_HasPoint重写
/// 测试：验证 addons/A1 文件夹的变更追踪
[GlobalClass]
public partial class ItemCellGroupController : Node
{
	#region Export Properties

	/// 逻辑数据源组件路径（提供局部坐标数组和形状变化事件）
	[Export] public NodePath GridShapeComp_Path { get; set; } = "%GridShapeComponent";

	/// 交互区域容器路径（用于放置生成的单元格）
	[Export] public NodePath InteractionArea_Path { get; set; } = "%InteractionArea";

	/// 单个格子的像素尺寸（默认64x64）
	[Export] public float CellSize { get; set; } = 64f;

	#endregion

	#region Private Fields

	/// GridShapeComponent引用
	private GridShapeComponent _gridShapeComp;

	/// InteractionArea引用
	private Control _interactionArea;

	/// 跟踪所有生成的GridCellUI实例
	private readonly List<GridCellUI> _cells = new();

	/// R3 Subject - 聚合所有单元格的输入事件
	private readonly Subject<InputEvent> _aggregatedInputSubject = new();

	/// CompositeDisposable - 管理所有订阅
	private readonly CompositeDisposable _disposables = new();

	#endregion

	#region Public Interface

	/// 暴露聚合后的输入事件流（供DraggableItemComponent订阅）
	public Observable<InputEvent> OnGroupInputAsObservable => _aggregatedInputSubject;

	#endregion

	#region Godot Lifecycle

	public override void _Ready()
	{
		// 解析NodePath引用
		_gridShapeComp = GetNodeOrNull<GridShapeComponent>(GridShapeComp_Path);
		_interactionArea = GetNodeOrNull<Control>(InteractionArea_Path);

		// 验证引用有效性
		if (_gridShapeComp == null)
		{
			GD.PushError($"[{Name}] GridShapeComponent not found at path: {GridShapeComp_Path}");
			return;
		}

		if (_interactionArea == null)
		{
			GD.PushError($"[{Name}] InteractionArea not found at path: {InteractionArea_Path}");
			return;
		}

		// 【关键架构设计】父容器忽略鼠标，让子GridCellUI独立接收事件
		_interactionArea.MouseFilter = Control.MouseFilterEnum.Ignore;

		// 订阅GridShapeComponent的形状变化事件
		_gridShapeComp.OnShapeChangedAsObservable
			.Subscribe(_ => RebuildCells())
			.AddTo(_disposables);

		// 【架构修正】仅在数据已存在时立即构建
		if (_gridShapeComp.CurrentLocalCells != null)
		{
			RebuildCells();
		}
	}

	public override void _ExitTree()
	{
		// 清理所有订阅
		_disposables?.Dispose();

		// 清理聚合Subject
		_aggregatedInputSubject?.Dispose();
	}

	#endregion

	#region Cell Management

	/// 重建单元格集合
	/// 目的：根据GridShapeComponent的当前形状生成对应的GridCellUI实例
	/// 示例：L形 [(0,0), (0,1), (1,1)] -> 生成3个GridCellUI，位置分别为(0,0), (0,64), (64,64)
	/// 算法：1. 清理旧单元格 -> 2. 遍历局部坐标 -> 3. 实例化GridCellUI并绑定事件 -> 4. 添加到容器
	private void RebuildCells()
	{
		// 1. 清理旧单元格
		foreach (var cell in _cells)
		{
			cell.QueueFree();
		}
		_cells.Clear();

		// 2. 验证数据源有效性
		if (_gridShapeComp.CurrentLocalCells == null)
		{
			GD.PushWarning($"[{Name}] CurrentLocalCells is null, skipping cell generation.");
			return;
		}

		// 3. 遍历局部坐标生成GridCellUI
		int cellIndex = 0;
		foreach (Vector2I cellPos in _gridShapeComp.CurrentLocalCells)
		{
			// 实例化GridCellUI
			var gridCellUI = new GridCellUI
			{
				Size = new Vector2(CellSize, CellSize),
				Position = new Vector2(cellPos.X * CellSize, cellPos.Y * CellSize),
				Name = $"GridCell_{cellIndex}"
			};

			// 【关键修复】先添加到场景树，再订阅事件
			_interactionArea.AddChild(gridCellUI);
			_cells.Add(gridCellUI);

			// 【事件聚合】将此单元格的输入事件推送到聚合Subject
			gridCellUI.OnCellInputAsObservable
				.Subscribe(inputEvent => _aggregatedInputSubject.OnNext(inputEvent))
				.AddTo(gridCellUI);

			// 【Hover 效果】订阅鼠标进入/离开事件
			gridCellUI.MouseEntered += () => SetGroupState(GridCellUI.CellState.Hover);
			gridCellUI.MouseExited += () => ResetGroupState();

			cellIndex++;
		}
	}

	#endregion

	#region Public API

	/// 设置整组单元格的状态
	/// 目的：统一控制所有单元格的视觉状态（用于拖拽反馈）
	/// 示例：SetGroupState(GridCellUI.CellState.Valid) -> 所有单元格显示绿色发光边框
	/// 算法：遍历_cells列表，调用每个GridCellUI的SetState方法
	public void SetGroupState(GridCellUI.CellState state)
	{
		foreach (var cell in _cells)
		{
			cell.SetState(state);
		}
	}

	/// 重置整组单元格状态为Normal
	public void ResetGroupState()
	{
		SetGroupState(GridCellUI.CellState.Normal);
	}

	#endregion
}
