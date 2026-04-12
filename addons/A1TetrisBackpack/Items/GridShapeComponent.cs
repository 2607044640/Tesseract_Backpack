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
	
	[Export] public ItemDataResource Data { get; set; }
	
	#endregion
	
	#region Public Properties
	
	// 当前运行时占用的局部格子数组（只读）
	public Vector2I[] CurrentLocalCells { get; private set; }
	
	#endregion
	
	#region R3 Reactive Streams
	
	public Subject<Unit> OnShapeChangedAsObservable { get; private set; }
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		OnShapeChangedAsObservable = new Subject<Unit>();
		InitializeShape();
		GD.Print($"GridShapeComponent 初始化完成：{CurrentLocalCells?.Length ?? 0} 个格子");
	}
	
	public override void _ExitTree()
	{
		OnShapeChangedAsObservable?.Dispose();
	}
	
	#endregion
	
	#region Shape Management
	
	private void InitializeShape()
	{
		if (Data == null)
		{
			GD.PushWarning("GridShapeComponent: Data 未设置，使用默认 1x1 形状");
			CurrentLocalCells = new Vector2I[] { Vector2I.Zero };
			return;
		}
		
		if (Data.BaseShape == null || Data.BaseShape.Count == 0)
		{
			GD.PushError($"GridShapeComponent: ItemDataResource [{Data.ItemID}] 的 BaseShape 为空");
			CurrentLocalCells = new Vector2I[] { Vector2I.Zero };
			return;
		}
		
		CurrentLocalCells = Data.BaseShape.ToArray();
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
		OnShapeChangedAsObservable.OnNext(Unit.Default);
		GD.Print("形状已重置为初始状态");
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
