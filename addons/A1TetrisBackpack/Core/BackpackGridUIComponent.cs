using Godot;
using System.Collections.Generic;

/// 背包网格 UI 组件 - 视图层坐标转换器 + GridCellUI 工厂
/// 
/// 职责：
/// - 桥接逻辑层（网格坐标）与 UI 层（像素坐标）
/// - 提供双向坐标转换方法
/// - 自动调整 UI 尺寸匹配逻辑网格
/// - 生成静态背景网格（使用 GridCellUI 实例，统一视觉语言）
/// 
/// 架构定位：
/// - 纯视图层：不包含业务逻辑（放置/移除物品）
/// - 工具类：提供坐标映射服务
/// - 工厂模式：生成背景 GridCellUI 网格
/// - 可视化容器：作为物品 UI 的父节点
/// 
/// 使用场景：
/// ```
/// BackpackPanel (BackpackGridUIComponent)
/// ├── GridContainer (MouseFilter=Ignore, 包含背景 GridCellUI)
/// │   ├── GridCellUI (0,0)
/// │   ├── GridCellUI (0,1)
/// │   └── ...
/// ├── Item1 (Control) - 通过 GridToLocalPosition 定位
/// ├── Item2 (Control)
/// └── ...
/// ```
/// 
/// 坐标系统：
/// - 网格坐标：整数 (x, y)，范围 [0, Width) x [0, Height)
/// - 局部像素坐标：浮点 (px, py)，相对于此 Control 左上角
/// - 全局像素坐标：浮点 (gx, gy)，屏幕绝对位置
/// 
/// 使用示例：
/// ```csharp
/// // 鼠标点击转网格坐标
/// var gridPos = backpackUI.GlobalToGridPosition(GetGlobalMousePosition());
/// 
/// // 物品吸附到网格
/// item.Position = backpackUI.GridToLocalPosition(new Vector2I(2, 3));
/// 
/// // 获取格子中心（用于居中显示）
/// item.Position = backpackUI.GetCellCenterPosition(new Vector2I(2, 3));
/// ```
[Tool]
[GlobalClass]
public partial class BackpackGridUIComponent : Control
{
	#region Export Properties
	
	/// 逻辑网格组件路径
	[Export] public NodePath BackpackGridComponentPath { get; set; } = "%BackpackGridComponent";
	
	/// 逻辑网格组件引用
	public BackpackGridComponent BackpackGridComp { get; private set; }
	
	/// 单个网格的像素尺寸
	[Export] public Vector2 CellSize { get; set; } = new Vector2(64, 64);
	
	#endregion
	
	#region Private Fields
	
	/// 背景网格容器引用
	private Control _backgroundCanvas;
	
	/// 跟踪所有生成的背景 GridCellUI 实例
	private readonly List<GridCellUI> _backgroundCells = new();
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _EnterTree()
	{
		// 设置鼠标过滤为Ignore，防止阻挡物品输入
		this.MouseFilter = MouseFilterEnum.Ignore;
		
		// 自动创建 BackgroundCanvas 容器
		_backgroundCanvas = GetNodeOrNull<Control>("BackgroundCanvas");
		if (_backgroundCanvas == null)
		{
			_backgroundCanvas = new Control
			{
				Name = "BackgroundCanvas",
				MouseFilter = MouseFilterEnum.Ignore
			};
			AddChild(_backgroundCanvas);
			// 【Ghost Node 防护】编辑器运行 [Tool] 脚本时，Owner=null 防止序列化到 .tscn
			if (Engine.IsEditorHint())
			{
				_backgroundCanvas.Owner = null;
			}
		}
		else
		{
			_backgroundCanvas.MouseFilter = MouseFilterEnum.Ignore;
		}
	}
	
	public override void _Ready()
	{
		// 只在有效配置时初始化
		if (BackpackGridComponentPath == null || BackpackGridComponentPath.IsEmpty)
		{
			return;
		}
		
		// NodePath 在 _Ready 时已解析完成，直接初始化
		InitializeComponent();
	}
	
	/// 延迟初始化组件（在 NodePath 解析完成后）
	private void InitializeComponent()
	{
		BackpackGridComp = GetNodeOrNull<BackpackGridComponent>(BackpackGridComponentPath);
		if (BackpackGridComp == null)
		{
			GD.PushError($"[{Name}] BackpackGridComponent not found: {BackpackGridComponentPath}");
			return;
		}
		
		// 根据逻辑网格尺寸自动调整 UI 大小
		UpdateUISize();
		
		// 生成背景网格
		GenerateBackgroundGrid();
		
		GD.Print($"BackpackGridUIComponent: 初始化完成 ({BackpackGridComp.Width}x{BackpackGridComp.Height} 网格，{CellSize} 像素/格)");
	}
	
	#endregion
	
	#region Coordinate Conversion
	
	/// 全局像素坐标 → 网格坐标
	/// 目的：将鼠标点击位置转换为逻辑网格坐标
	/// 示例：鼠标点击 (320, 192) → 网格坐标 (5, 3)（假设 CellSize=64）
	/// 算法：1. 转为局部坐标 → 2. 除以格子尺寸 → 3. 向下取整 → 4. 边界检查
	public Vector2I GlobalToGridPosition(Vector2 globalPos)
	{
		// 1. 转换为相对于此 Control 的局部坐标
		Vector2 localPos = globalPos - GlobalPosition;
		
		// 2. 除以格子尺寸并向下取整
		int gridX = Mathf.FloorToInt(localPos.X / CellSize.X);
		int gridY = Mathf.FloorToInt(localPos.Y / CellSize.Y);
		
		// 3. 边界限制（确保在有效范围内）
		gridX = Mathf.Clamp(gridX, 0, BackpackGridComp.Width - 1);
		gridY = Mathf.Clamp(gridY, 0, BackpackGridComp.Height - 1);
		
		return new Vector2I(gridX, gridY);
	}
	
	/// 网格坐标 → 局部像素坐标（左上角）
	/// 目的：将物品吸附到网格位置
	/// 示例：网格坐标 (2, 3) → 局部像素 (128, 192)（假设 CellSize=64）
	/// 算法：直接乘以格子尺寸
	public Vector2 GridToLocalPosition(Vector2I gridPos)
	{
		return new Vector2(gridPos.X * CellSize.X, gridPos.Y * CellSize.Y);
	}
	
	/// 网格坐标 → 局部像素坐标（中心点）
	public Vector2 GetCellCenterPosition(Vector2I gridPos)
	{
		Vector2 topLeft = GridToLocalPosition(gridPos);
		return topLeft + CellSize / 2;
	}
	
	/// 检查网格坐标是否在有效范围内
	public bool IsValidGridPosition(Vector2I gridPos)
	{
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
	
	/// 根据逻辑网格尺寸更新 UI 大小
	private void UpdateUISize()
	{
		if (BackpackGridComp == null)
			return;
		
		Vector2 totalSize = new Vector2(
			BackpackGridComp.Width * CellSize.X,
			BackpackGridComp.Height * CellSize.Y
		);
		
		CustomMinimumSize = totalSize;
		Size = totalSize;
		
		GD.Print($"BackpackGridUIComponent: UI 尺寸设置为 {totalSize}");
	}
	
	/// 生成背景网格（使用 GridCellUI 实例）
	private void GenerateBackgroundGrid()
	{
		if (BackpackGridComp == null || _backgroundCanvas == null)
			return;
		
		// 【CRITICAL】类型过滤清理：仅 QueueFree 已有的 GridCellUI 子节点
		// 防止 [Tool] 脚本多次执行累积 + 防止 .tscn 中残留的 Ghost Node
		foreach (Node child in _backgroundCanvas.GetChildren())
		{
			if (child is GridCellUI)
			{
				child.QueueFree();
			}
		}
		_backgroundCells.Clear();
		
		// 【安全检查】防止编辑器崩溃
		if (BackpackGridComp.Width <= 0 || BackpackGridComp.Height <= 0)
			return;
		
		// 2. 嵌套循环生成 GridCellUI
		for (int y = 0; y < BackpackGridComp.Height; y++)
		{
			for (int x = 0; x < BackpackGridComp.Width; x++)
			{
				var gridCellUI = new GridCellUI
				{
					Size = CellSize,
					Position = new Vector2(x * CellSize.X, y * CellSize.Y),
					Name = $"BackgroundCell_{x}_{y}",
					MouseFilter = MouseFilterEnum.Pass
				};
				
				_backgroundCanvas.AddChild(gridCellUI);
				// 【Ghost Node 防护】编辑器运行时 Owner=null，防止序列化到 .tscn
				if (Engine.IsEditorHint())
				{
					gridCellUI.Owner = null;
				}
				_backgroundCells.Add(gridCellUI);
				
				gridCellUI.SetState(GridCellUI.CellState.Normal);
			}
		}
		
		GD.Print($"BackpackGridUIComponent: 生成背景网格 {BackpackGridComp.Width}x{BackpackGridComp.Height} = {_backgroundCells.Count} 个 GridCellUI");
	}
	
	/// 刷新网格显示（属性变化后调用）
	public void RefreshGrid()
	{
		UpdateUISize();
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
		// 先清除所有预览
		ClearPreview();
		
		// 应用新预览数据
		foreach (var item in previewData)
		{
			// 只处理在网格范围内的位置
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
	
	/// 获取指定网格位置的矩形区域（用于高亮显示）
	public Rect2 GetCellRect(Vector2I gridPos)
	{
		Vector2 topLeft = GridToLocalPosition(gridPos);
		return new Rect2(topLeft, CellSize);
	}
	
	/// 获取多个格子组成的矩形区域（用于物品形状高亮）
	public Rect2 GetShapeRect(Vector2I startPos, Vector2I[] shape)
	{
		if (shape == null || shape.Length == 0)
			return GetCellRect(startPos);
		
		// 计算边界框
		int minX = int.MaxValue, minY = int.MaxValue;
		int maxX = int.MinValue, maxY = int.MinValue;
		
		foreach (var offset in shape)
		{
			Vector2I worldPos = startPos + offset;
			minX = Mathf.Min(minX, worldPos.X);
			minY = Mathf.Min(minY, worldPos.Y);
			maxX = Mathf.Max(maxX, worldPos.X);
			maxY = Mathf.Max(maxY, worldPos.Y);
		}
		
		Vector2 topLeft = GridToLocalPosition(new Vector2I(minX, minY));
		Vector2 size = new Vector2(
			(maxX - minX + 1) * CellSize.X,
			(maxY - minY + 1) * CellSize.Y
		);
		
		return new Rect2(topLeft, size);
	}
	
	/// 设置格子尺寸并刷新显示
	public void SetCellSize(Vector2 newSize)
	{
		CellSize = newSize;
		RefreshGrid();
	}
	
	#endregion
}
