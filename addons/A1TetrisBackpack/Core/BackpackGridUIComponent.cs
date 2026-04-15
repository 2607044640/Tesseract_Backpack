using Godot;

/// <summary>
/// 背包网格 UI 组件 - 视图层坐标转换器
/// 
/// 职责：
/// - 桥接逻辑层（网格坐标）与 UI 层（像素坐标）
/// - 提供双向坐标转换方法
/// - 自动调整 UI 尺寸匹配逻辑网格
/// - 绘制调试网格线
/// 
/// 架构定位：
/// - 纯视图层：不包含业务逻辑（放置/移除物品）
/// - 工具类：提供坐标映射服务
/// - 可视化容器：作为物品 UI 的父节点
/// 
/// 使用场景：
/// ```
/// BackpackPanel (BackpackGridUIComponent)
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
/// </summary>
[GlobalClass]
public partial class BackpackGridUIComponent : Control
{
	#region Export Properties
	
	/// <summary>
	/// 逻辑网格组件引用
	/// </summary>
	[Export] public BackpackGridComponent LogicGrid { get; set; }
	
	/// <summary>
	/// 单个网格的像素尺寸
	/// </summary>
	[Export] public Vector2 CellSize { get; set; } = new Vector2(64, 64);
	
	/// <summary>
	/// 是否绘制调试网格线
	/// </summary>
	[Export] public bool DrawDebugLines { get; set; } = true;
	
	/// <summary>
	/// 网格线颜色
	/// </summary>
	[Export] public Color GridColor { get; set; } = new Color(1, 1, 1, 0.3f);
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		// 延迟初始化以等待 Godot 解析 NodePath
		CallDeferred(MethodName.InitializeComponent);
	}
	
	/// <summary>
	/// 延迟初始化组件（在 NodePath 解析完成后）
	/// </summary>
	private void InitializeComponent()
	{
		// 自动查找 LogicGrid（如果未手动设置）
		if (LogicGrid == null)
		{
			LogicGrid = GetNodeOrNull<BackpackGridComponent>("BackpackGridComponent");
			if (LogicGrid == null)
			{
				GD.PushError("BackpackGridUIComponent: 无法找到 BackpackGridComponent！");
				return;
			}
		}
		
		// 根据逻辑网格尺寸自动调整 UI 大小
		UpdateUISize();
		
		// 触发初始绘制
		QueueRedraw();
		
		GD.Print($"BackpackGridUIComponent: 初始化完成 ({LogicGrid.Width}x{LogicGrid.Height} 网格，{CellSize} 像素/格)");
	}
	
	public override void _Draw()
	{
		if (!DrawDebugLines || LogicGrid == null)
			return;
		
		DrawGridLines();
	}
	
	#endregion
	
	#region Coordinate Conversion
	
	/// <summary>
	/// 全局像素坐标 → 网格坐标
	/// 目的：将鼠标点击位置转换为逻辑网格坐标
	/// 示例：鼠标点击 (320, 192) → 网格坐标 (5, 3)（假设 CellSize=64）
	/// 算法：1. 转为局部坐标 → 2. 除以格子尺寸 → 3. 向下取整 → 4. 边界检查
	/// </summary>
	public Vector2I GlobalToGridPosition(Vector2 globalPos)
	{
		// 1. 转换为相对于此 Control 的局部坐标
		Vector2 localPos = globalPos - GlobalPosition;
		
		// 2. 除以格子尺寸并向下取整
		int gridX = Mathf.FloorToInt(localPos.X / CellSize.X);
		int gridY = Mathf.FloorToInt(localPos.Y / CellSize.Y);
		
		// 3. 边界限制（确保在有效范围内）
		gridX = Mathf.Clamp(gridX, 0, LogicGrid.Width - 1);
		gridY = Mathf.Clamp(gridY, 0, LogicGrid.Height - 1);
		
		return new Vector2I(gridX, gridY);
	}
	
	/// <summary>
	/// 网格坐标 → 局部像素坐标（左上角）
	/// 目的：将物品吸附到网格位置
	/// 示例：网格坐标 (2, 3) → 局部像素 (128, 192)（假设 CellSize=64）
	/// 算法：直接乘以格子尺寸
	/// </summary>
	public Vector2 GridToLocalPosition(Vector2I gridPos)
	{
		return new Vector2(gridPos.X * CellSize.X, gridPos.Y * CellSize.Y);
	}
	
	/// <summary>
	/// 网格坐标 → 局部像素坐标（中心点）
	/// </summary>
	public Vector2 GetCellCenterPosition(Vector2I gridPos)
	{
		Vector2 topLeft = GridToLocalPosition(gridPos);
		return topLeft + CellSize / 2;
	}
	
	/// <summary>
	/// 检查网格坐标是否在有效范围内
	/// </summary>
	public bool IsValidGridPosition(Vector2I gridPos)
	{
		return gridPos.X >= 0 && gridPos.X < LogicGrid.Width &&
		       gridPos.Y >= 0 && gridPos.Y < LogicGrid.Height;
	}
	
	/// <summary>
	/// 局部像素坐标 → 网格坐标
	/// </summary>
	public Vector2I LocalToGridPosition(Vector2 localPos)
	{
		int gridX = Mathf.FloorToInt(localPos.X / CellSize.X);
		int gridY = Mathf.FloorToInt(localPos.Y / CellSize.Y);
		
		gridX = Mathf.Clamp(gridX, 0, LogicGrid.Width - 1);
		gridY = Mathf.Clamp(gridY, 0, LogicGrid.Height - 1);
		
		return new Vector2I(gridX, gridY);
	}
	
	#endregion
	
	#region UI Management
	
	/// <summary>
	/// 根据逻辑网格尺寸更新 UI 大小
	/// </summary>
	private void UpdateUISize()
	{
		if (LogicGrid == null)
			return;
		
		Vector2 totalSize = new Vector2(
			LogicGrid.Width * CellSize.X,
			LogicGrid.Height * CellSize.Y
		);
		
		CustomMinimumSize = totalSize;
		Size = totalSize;
		
		GD.Print($"BackpackGridUIComponent: UI 尺寸设置为 {totalSize}");
	}
	
	/// <summary>
	/// 绘制调试网格线
	/// </summary>
	private void DrawGridLines()
	{
		if (LogicGrid == null)
			return;
		
		float totalWidth = LogicGrid.Width * CellSize.X;
		float totalHeight = LogicGrid.Height * CellSize.Y;
		
		// 绘制横线
		for (int y = 0; y <= LogicGrid.Height; y++)
		{
			float yPos = y * CellSize.Y;
			DrawLine(
				new Vector2(0, yPos),
				new Vector2(totalWidth, yPos),
				GridColor,
				1.0f
			);
		}
		
		// 绘制竖线
		for (int x = 0; x <= LogicGrid.Width; x++)
		{
			float xPos = x * CellSize.X;
			DrawLine(
				new Vector2(xPos, 0),
				new Vector2(xPos, totalHeight),
				GridColor,
				1.0f
			);
		}
	}
	
	/// <summary>
	/// 刷新网格显示（属性变化后调用）
	/// </summary>
	public void RefreshGrid()
	{
		UpdateUISize();
		QueueRedraw();
	}
	
	#endregion
	
	#region Helper Methods
	
	/// <summary>
	/// 获取指定网格位置的矩形区域（用于高亮显示）
	/// </summary>
	public Rect2 GetCellRect(Vector2I gridPos)
	{
		Vector2 topLeft = GridToLocalPosition(gridPos);
		return new Rect2(topLeft, CellSize);
	}
	
	/// <summary>
	/// 获取多个格子组成的矩形区域（用于物品形状高亮）
	/// </summary>
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
	
	/// <summary>
	/// 设置格子尺寸并刷新显示
	/// </summary>
	public void SetCellSize(Vector2 newSize)
	{
		CellSize = newSize;
		RefreshGrid();
	}
	
	/// <summary>
	/// 切换调试网格线显示
	/// </summary>
	public void ToggleDebugLines(bool enabled)
	{
		DrawDebugLines = enabled;
		QueueRedraw();
	}
	
	#endregion
}
