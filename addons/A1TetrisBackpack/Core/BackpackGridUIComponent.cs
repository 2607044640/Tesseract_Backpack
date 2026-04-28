using Godot;
using System.Collections.Generic;

/// 背包网格 UI 逻辑组件 - 逻辑桥接器 + GridCellUI 工厂
///
/// 职责：
/// - 作为逻辑桥接器：处理外部注入的 Control 节点之间的坐标转换
/// - 工厂模式：在注入的 GridBackground 容器中生成背景格子
/// - 纯逻辑节点：不再继承 Control，不直接参与布局和绘制
///
/// 架构定位：
/// - Bridge 层：连接逻辑网格（BackpackGridComponent）与外部 UI 容器
/// - 无状态视图逻辑：所有空间属性（Size, Position）由注入的 Control 决定
[GlobalClass]
public partial class BackpackGridUIComponent : Node
{
	#region Export Properties

	/// 逻辑网格组件路径
	[Export] public NodePath BackpackGridComponentPath { get; set; } = "%BackpackGridComponent";

	/// 逻辑网格组件引用
	public BackpackGridComponent BackpackGridComp { get; private set; }

	/// 单个网格的像素尺寸
	[Export] public Vector2 CellSize { get; set; } = new Vector2(64, 64);

	/// 背景网格容器（外部注入，格子将生成在此节点下）
	[Export] public Control GridBackground { get; set; }

	/// 交互区域锚点（外部注入，用于坐标转换的基准位置）
	[Export] public Control InteractionArea { get; set; }

	#endregion

	#region Private Fields

	/// 跟踪所有生成的背景 GridCellUI 实例
	private readonly List<GridCellUI> _backgroundCells = new();

	#endregion

	#region Godot Lifecycle

	public override void _Ready()
	{
		// 只在有效配置时初始化
		if (BackpackGridComponentPath == null || BackpackGridComponentPath.IsEmpty)
		{
			return;
		}

		InitializeComponent();
	}

	/// 初始化逻辑组件
	private void InitializeComponent()
	{
		BackpackGridComp = GetNodeOrNull<BackpackGridComponent>(BackpackGridComponentPath);
		if (BackpackGridComp == null)
		{
			GD.PushError($"[{Name}] BackpackGridComponent not found: {BackpackGridComponentPath}");
			return;
		}

		if (GridBackground == null)
		{
			GD.PushError($"[{Name}] GridBackground is not assigned. Please assign it in the editor.");
			return;
		}

		if (InteractionArea == null)
		{
			GD.PushError($"[{Name}] InteractionArea is not assigned. This is required for coordinate conversion.");
			return;
		}

		// 确保背景层不阻挡输入（根据 Blueprint 要求）
		GridBackground.MouseFilter = Control.MouseFilterEnum.Ignore;

		// 运行时生成真实节点
		if (!Engine.IsEditorHint())
			GenerateBackgroundGrid();

		GD.Print($"BackpackGridUIComponent (Node): 初始化完成。基准锚点: {InteractionArea.Name}, 格子容器: {GridBackground.Name}");
	}

	#endregion

	#region Coordinate Conversion

	/// 全局像素坐标 → 网格坐标
	/// 锚点：使用注入的 InteractionArea 的全局位置
	public Vector2I GlobalToGridPosition(Vector2 globalPos)
	{
		if (InteractionArea == null) return Vector2I.Zero;

		// 使用 InteractionArea 作为坐标锚点
		Vector2 localPos = globalPos - InteractionArea.GlobalPosition;

		int gridX = Mathf.FloorToInt(localPos.X / CellSize.X);
		int gridY = Mathf.FloorToInt(localPos.Y / CellSize.Y);

		gridX = Mathf.Clamp(gridX, 0, BackpackGridComp.Width - 1);
		gridY = Mathf.Clamp(gridY, 0, BackpackGridComp.Height - 1);

		return new Vector2I(gridX, gridY);
	}

	/// 网格坐标 → 局部像素坐标（相对于 InteractionArea）
	public Vector2 GridToLocalPosition(Vector2I gridPos)
	{
		return new Vector2(gridPos.X * CellSize.X, gridPos.Y * CellSize.Y);
	}

	/// 网格坐标 → 局部像素坐标（中心点，相对于 InteractionArea）
	public Vector2 GetCellCenterLocalPos(Vector2I gridPos)
	{
		Vector2 topLeft = GridToLocalPosition(gridPos);
		return topLeft + CellSize / 2;
	}

	/// 检查网格坐标是否在有效范围内
	public bool IsValidGridPosition(Vector2I gridPos)
	{
		if (BackpackGridComp == null) return false;
		return gridPos.X >= 0 && gridPos.X < BackpackGridComp.Width &&
		       gridPos.Y >= 0 && gridPos.Y < BackpackGridComp.Height;
	}

	/// 局部像素坐标 → 网格坐标
	public Vector2I LocalToGridPosition(Vector2 localPos)
	{
		int gridX = Mathf.FloorToInt(localPos.X / CellSize.X);
		int gridY = Mathf.FloorToInt(localPos.Y / CellSize.Y);

		gridX = Mathf.Clamp(gridX, 0, BackpackGridComp.Width - 1);
		gridY = Mathf.Clamp(gridY, 0, BackpackGridComp.Height - 1);

		return new Vector2I(gridX, gridY);
	}

	#endregion

	#region UI Management

	/// 生成背景网格（添加到 GridBackground）
	private void GenerateBackgroundGrid()
	{
		if (BackpackGridComp == null || GridBackground == null)
			return;

		foreach (Node child in GridBackground.GetChildren())
		{
			if (child is GridCellUI)
			{
				child.QueueFree();
			}
		}
		_backgroundCells.Clear();

		if (BackpackGridComp.Width <= 0 || BackpackGridComp.Height <= 0)
			return;

		for (int y = 0; y < BackpackGridComp.Height; y++)
		{
			for (int x = 0; x < BackpackGridComp.Width; x++)
			{
				var gridCellUI = new GridCellUI
				{
					Size = CellSize,
					Position = new Vector2(x * CellSize.X, y * CellSize.Y),
					Name = $"BackgroundCell_{x}_{y}",
					MouseFilter = Control.MouseFilterEnum.Pass // 格子需要接收鼠标进入/离开事件
				};

				GridBackground.AddChild(gridCellUI);
				_backgroundCells.Add(gridCellUI);

				gridCellUI.SetState(GridCellUI.CellState.Normal);
			}
		}
	}

	/// 刷新网格显示
	public void RefreshGrid()
	{
		GenerateBackgroundGrid();
	}

	/// 清除预览状态
	public void ClearPreview()
	{
		foreach (var cell in _backgroundCells)
		{
			cell.SetState(GridCellUI.CellState.Normal);
		}
	}

	/// 显示放置预览
	public void ShowPreview(System.Collections.Generic.List<(Vector2I GridPos, GridCellUI.CellState State)> previewData)
	{
		ClearPreview();

		foreach (var item in previewData)
		{
			if (item.GridPos.X >= 0 && item.GridPos.X < BackpackGridComp.Width &&
			    item.GridPos.Y >= 0 && item.GridPos.Y < BackpackGridComp.Height)
			{
				int index = item.GridPos.Y * BackpackGridComp.Width + item.GridPos.X;
				_backgroundCells[index].SetState(item.State);
			}
		}
	}

	#endregion

	#region Helper Methods

	public Vector2 GlobalPosition => InteractionArea?.GlobalPosition ?? Vector2.Zero;

	public Vector2 GetGlobalMousePosition() => InteractionArea?.GetGlobalMousePosition() ?? Vector2.Zero;

	/// 检查全局点是否在网格范围内
	public bool IsPointInside(Vector2 globalPos) => GetGlobalRect().HasPoint(globalPos);

	public Rect2 GetCellRect(Vector2I gridPos)
	{
		Vector2 topLeft = GridToLocalPosition(gridPos);
		return new Rect2(topLeft, CellSize);
	}

	public Rect2 GetGlobalRect()
	{
		if (InteractionArea == null) return new Rect2();
		return InteractionArea.GetGlobalRect();
	}

	#endregion
}
