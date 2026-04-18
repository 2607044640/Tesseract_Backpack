using Godot;
using R3;
using System.Linq;

/// <summary>
/// 网格形状组件 - 管理物品运行时形状
/// 目的：从 ItemDataResource 加载形状，支持运行时旋转，通过 R3 发布形状变化事件
/// 示例：L 形物品 [(0,0), (0,1), (1,1)] 旋转 90° -> [(0,0), (1,0), (1,1)]
/// 算法：1. 加载初始形状 -> 2. 应用旋转矩阵 (x,y) -> (-y,x) -> 3. 归一化坐标 -> 4. 发布 R3 事件
/// </summary>
[GlobalClass]
public partial class GridShapeComponent : Node
{
	#region Export Properties
	
	/// <summary>
	/// 单个格子的像素尺寸（用于 UI 尺寸计算）
	/// </summary>
	[Export] public Vector2 CellSize { get; set; } = new Vector2(64, 64);
	
	/// <summary>
	/// 是否自动调整父 Control 节点的尺寸
	/// </summary>
	[Export] public bool AutoResizeParent { get; set; } = true;
	
	/// <summary>
	/// 可选的 VisualContainer 节点路径（如果需要同时调整视觉容器尺寸）
	/// </summary>
	[Export] public NodePath VisualContainerPath { get; set; } = "%VisualContainer";
	
	#endregion
	
	#region Private Fields
	
	private ItemDataResource _data;
	
	#endregion
	
	#region Public Properties
	
	// 当前运行时占用的局部格子数组（只读）
	public Vector2I[] CurrentLocalCells { get; private set; }
	
	/// <summary>
	/// 获取当前的 ItemDataResource（只读）
	/// </summary>
	public ItemDataResource Data => _data;
	
	#endregion
	
	#region R3 Reactive Streams
	
	public Subject<Unit> OnShapeChangedAsObservable { get; private set; }
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		// 订阅父节点的 DataInitialized 事件（通过接口解耦）
		var parent = GetParent();
		if (parent is IItemDataProvider provider)
		{
			provider.DataInitialized += OnDataReceived;
			GD.Print($"[{Name}] GridShapeComponent: 已订阅父节点的 DataInitialized 事件（接口模式）");
		}
		else
		{
			GD.PushWarning($"[{Name}] GridShapeComponent: 父节点未实现 IItemDataProvider 接口，使用默认初始化");
			InitializeShape();
			
			if (AutoResizeParent)
			{
				CallDeferred(MethodName.UpdateParentSize);
			}
		}
	}
	
	public override void _ExitTree()
	{
		// 取消订阅事件，防止内存泄漏（通过接口解耦）
		var parent = GetParent();
		if (parent is IItemDataProvider provider)
		{
			provider.DataInitialized -= OnDataReceived;
		}
		
		OnShapeChangedAsObservable?.Dispose();
	}
	
	#endregion
	
	#region Event Handlers
	
	/// <summary>
	/// 接收父节点传递的 ItemDataResource 并初始化形状
	/// </summary>
	private void OnDataReceived(ItemDataResource data)
	{
		SetData(data);
	}
	
	/// <summary>
	/// 设置 ItemDataResource 并初始化形状（供外部直接调用或事件触发）
	/// </summary>
	public void SetData(ItemDataResource data)
	{
		_data = data;
		GD.Print($"[{Name}] GridShapeComponent.SetData: 收到 Data = {data?.ItemID ?? "null"}");
		
		InitializeShape();
		
		// 延迟调整父节点尺寸（确保所有节点初始化完成）
		if (AutoResizeParent)
		{
			CallDeferred(MethodName.UpdateParentSize);
		}
		
		GD.Print($"GridShapeComponent 初始化完成：{CurrentLocalCells?.Length ?? 0} 个格子");
	}
	
	#endregion
	
	#region Shape Management
	
	private void InitializeShape()
	{
		if (_data == null)
		{
			GD.PushWarning("GridShapeComponent: Data 未设置，使用默认 1x1 形状");
			CurrentLocalCells = new Vector2I[] { Vector2I.Zero };
			return;
		}
		
		if (_data.BaseShape == null || _data.BaseShape.Count == 0)
		{
			GD.PushError($"GridShapeComponent: ItemDataResource [{_data.ItemID}] 的 BaseShape 为空");
			CurrentLocalCells = new Vector2I[] { Vector2I.Zero };
			return;
		}
		
		CurrentLocalCells = _data.BaseShape.ToArray();
		NormalizeShape();
	}
	
	/// <summary>
	/// 顺时针旋转 90 度
	/// 目的：应用旋转矩阵实现形状旋转，保持坐标归一化
	/// 示例：2x1 横条 [(0,0), (1,0)] 旋转 -> 1x2 竖条 [(0,0), (0,1)]
	/// 算法：1. 应用旋转矩阵 (x,y) -> (-y,x) -> 2. 归一化坐标（确保左上角为原点）-> 3. 发布 R3 事件
	/// </summary>
	public void Rotate90()
	{
		if (CurrentLocalCells == null || CurrentLocalCells.Length == 0)
		{
			GD.PushWarning("GridShapeComponent: 无法旋转空形状");
			return;
		}
		
		for (int i = 0; i < CurrentLocalCells.Length; i++)
		{
			Vector2I oldCell = CurrentLocalCells[i];
			CurrentLocalCells[i] = new Vector2I(-oldCell.Y, oldCell.X);
		}
		
		NormalizeShape();
		
		// 旋转后更新父节点尺寸
		if (AutoResizeParent)
		{
			UpdateParentSize();
		}
		
		OnShapeChangedAsObservable.OnNext(Unit.Default);
		GD.Print($"物品已旋转 90°，当前形状：{string.Join(", ", CurrentLocalCells.Select(c => c.ToString()))}");
	}
	
	/// <summary>
	/// 归一化形状坐标
	/// 目的：确保形状左上角始终为 (0,0)，避免负坐标
	/// 示例：旋转后坐标 [(-1,0), (-1,1), (0,1)] -> 归一化后 [(0,0), (0,1), (1,1)]
	/// 算法：1. 找到最小 X 和最小 Y -> 2. 所有坐标减去 (minX, minY)
	/// </summary>
	private void NormalizeShape()
	{
		if (CurrentLocalCells == null || CurrentLocalCells.Length == 0)
			return;
		
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		
		foreach (var cell in CurrentLocalCells)
		{
			minX = Mathf.Min(minX, cell.X);
			minY = Mathf.Min(minY, cell.Y);
		}
		
		if (minX == 0 && minY == 0)
			return;
		
		Vector2I offset = new Vector2I(minX, minY);
		for (int i = 0; i < CurrentLocalCells.Length; i++)
		{
			CurrentLocalCells[i] -= offset;
		}
	}
	
	public void ResetShape()
	{
		InitializeShape();
		
		if (AutoResizeParent)
		{
			UpdateParentSize();
		}
		
		OnShapeChangedAsObservable.OnNext(Unit.Default);
		GD.Print("形状已重置为初始状态");
	}
	
	#endregion
	
	#region UI Size Management
	
	/// <summary>
	/// 根据形状自动调整父 Control 节点的尺寸
	/// 目的：让物品 UI 自动匹配形状大小
	/// 示例：L 形 2x2 -> Control 尺寸设置为 128x128 (假设 CellSize=64)
	/// 算法：1. 获取边界尺寸 -> 2. 计算像素尺寸 -> 3. 更新父节点 Size 和 CustomMinimumSize
	/// </summary>
	private void UpdateParentSize()
	{
		var parent = GetParent();
		if (parent is not Control parentControl)
		{
			GD.PushWarning($"GridShapeComponent: 父节点不是 Control，无法自动调整尺寸");
			return;
		}
		
		// 获取形状边界尺寸
		Vector2I boundingSize = GetBoundingSize();
		
		// 计算总像素尺寸
		Vector2 totalSize = new Vector2(
			boundingSize.X * CellSize.X,
			boundingSize.Y * CellSize.Y
		);
		
		// 更新父 Control 尺寸
		parentControl.CustomMinimumSize = totalSize;
		parentControl.Size = totalSize;
		
		// 如果配置了 VisualContainer 路径，同时更新它
		if (!string.IsNullOrEmpty(VisualContainerPath))
		{
			var visualContainer = parentControl.GetNodeOrNull<Control>(VisualContainerPath);
			if (visualContainer != null)
			{
				visualContainer.CustomMinimumSize = totalSize;
				visualContainer.Size = totalSize;
			}
		}
		
		GD.Print($"GridShapeComponent: 已更新父节点尺寸为 {totalSize} (形状: {boundingSize.X}x{boundingSize.Y})");
		GD.Print($"GridShapeComponent: 形状格子: {string.Join(", ", CurrentLocalCells)}");
	}
	
	/// <summary>
	/// 手动触发父节点尺寸更新（供外部调用）
	/// </summary>
	public void RefreshParentSize()
	{
		if (AutoResizeParent)
		{
			UpdateParentSize();
		}
	}
	
	#endregion
	
	#region Helper Methods
	
	public Vector2I GetBoundingSize()
	{
		if (CurrentLocalCells == null || CurrentLocalCells.Length == 0)
			return new Vector2I(1, 1);
		
		int maxX = 0, maxY = 0;
		foreach (var cell in CurrentLocalCells)
		{
			maxX = Mathf.Max(maxX, cell.X);
			maxY = Mathf.Max(maxY, cell.Y);
		}
		
		return new Vector2I(maxX + 1, maxY + 1);
	}
	
	public bool ContainsCell(Vector2I localCell)
	{
		if (CurrentLocalCells == null)
			return false;
		
		foreach (var cell in CurrentLocalCells)
		{
			if (cell == localCell)
				return true;
		}
		
		return false;
	}
	
	public Vector2 GetCenter()
	{
		if (CurrentLocalCells == null || CurrentLocalCells.Length == 0)
			return Vector2.Zero;
		
		Vector2 sum = Vector2.Zero;
		foreach (var cell in CurrentLocalCells)
		{
			sum += new Vector2(cell.X, cell.Y);
		}
		
		return sum / CurrentLocalCells.Length;
	}
	
	#endregion
}
