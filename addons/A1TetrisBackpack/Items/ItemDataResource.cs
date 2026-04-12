using Godot;
using Godot.Collections;

/// <summary>
/// 物品静态数据资源 - Godot Resource 配置
/// 目的：在编辑器中创建可复用的物品配置（ID、名称、图标、形状），支持多实例共享
/// 示例：创建 sword_item.tres 配置剑的属性 -> 多个剑实例引用同一配置
/// 算法：1. 编辑器创建 .tres 资源 -> 2. 配置属性 -> 3. 场景引用资源 -> 4. Godot 自动序列化/反序列化
/// </summary>
[GlobalClass]
public partial class ItemDataResource : Resource
{
	#region Export Properties
	
	[Export] public string ItemID { get; set; } = "item_default";
	[Export] public string ItemName { get; set; } = "未命名物品";
	[Export] public Texture2D Icon { get; set; }
	
	// 物品形状：坐标系原点(0,0)为左上角，X向右Y向下
	// 示例：[(0,0), (1,0)] 表示 2x1 横条 ■■
	[Export] public Array<Vector2I> BaseShape { get; set; } = new Array<Vector2I> { Vector2I.Zero };
	
	#endregion
	
	#region Constructor
	
	public ItemDataResource()
	{
		if (BaseShape == null || BaseShape.Count == 0)
		{
			BaseShape = new Array<Vector2I> { Vector2I.Zero };
		}
	}
	
	#endregion
	
	#region Helper Methods
	
	public int GetCellCount()
	{
		return BaseShape?.Count ?? 1;
	}
	
	/// <summary>
	/// 获取物品边界框尺寸
	/// 目的：计算物品占用的最小矩形区域，用于 UI 预览和碰撞检测
	/// 示例：L形物品 [(0,0), (0,1), (1,1)] -> 返回 (2, 2)
	/// 算法：1. 遍历所有坐标找最小/最大 X/Y -> 2. 计算宽高 (maxX-minX+1, maxY-minY+1)
	/// </summary>
	public Vector2I GetBoundingSize()
	{
		if (BaseShape == null || BaseShape.Count == 0)
			return new Vector2I(1, 1);
		
		int minX = int.MaxValue, minY = int.MaxValue;
		int maxX = int.MinValue, maxY = int.MinValue;
		
		foreach (var cell in BaseShape)
		{
			minX = Mathf.Min(minX, cell.X);
			minY = Mathf.Min(minY, cell.Y);
			maxX = Mathf.Max(maxX, cell.X);
			maxY = Mathf.Max(maxY, cell.Y);
		}
		
		return new Vector2I(maxX - minX + 1, maxY - minY + 1);
	}
	
	/// <summary>
	/// 验证形状数据有效性
	/// 目的：调试时检查形状配置是否合法（非空、无重复坐标）
	/// 示例：形状 [(0,0), (0,0)] -> 返回 false 并输出警告
	/// 算法：1. 检查是否为空 -> 2. 使用 HashSet 检测重复坐标 -> 3. 返回验证结果
	/// </summary>
	public bool IsShapeValid()
	{
		if (BaseShape == null || BaseShape.Count == 0)
		{
			GD.PushError($"ItemDataResource [{ItemID}]: BaseShape 为空");
			return false;
		}
		
		var uniqueCells = new System.Collections.Generic.HashSet<Vector2I>();
		foreach (var cell in BaseShape)
		{
			if (!uniqueCells.Add(cell))
			{
				GD.PushWarning($"ItemDataResource [{ItemID}]: 形状包含重复坐标 {cell}");
				return false;
			}
		}
		
		return true;
	}
	
	#endregion
}
