using Godot;
using R3;
using System;

/// <summary>
/// 背包网格核心组件 - 纯数据逻辑层
/// 目的：管理二维网格的物品占用状态，提供放置/移除逻辑，通过 R3 发布响应式事件
/// 示例：10x6 网格，放置 L 形物品到 (2,3) -> 占用 (2,3), (2,4), (3,4) 三个格子
/// 算法：1. 一维数组模拟二维网格 -> 2. 检测碰撞 -> 3. 更新占用状态 -> 4. 发布 R3 事件
/// </summary>
[GlobalClass]
public partial class BackpackGridComponent : Node
{
	#region Export Properties
	
	[Export] public int Width { get; set; } = 10;
	[Export] public int Height { get; set; } = 6;
	
	#endregion
	
	#region Private Fields
	
	// 一维数组模拟二维网格，索引计算：index = y * Width + x
	private ItemData[] _gridData;
	
	#endregion
	
	#region R3 Reactive Streams
	
	public Subject<(ItemData item, Vector2I position)> OnItemPlacedAsObservable { get; private set; }
	public Subject<(ItemData item, Vector2I position)> OnItemRemovedAsObservable { get; private set; }
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		_gridData = new ItemData[Width * Height];
		OnItemPlacedAsObservable = new Subject<(ItemData, Vector2I)>();
		OnItemRemovedAsObservable = new Subject<(ItemData, Vector2I)>();
		
		GD.Print($"BackpackGridComponent 初始化完成：{Width}x{Height} = {_gridData.Length} 格子");
	}
	
	public override void _ExitTree()
	{
		OnItemPlacedAsObservable?.Dispose();
		OnItemRemovedAsObservable?.Dispose();
	}
	
	#endregion
	
	#region Core Grid Logic
	
	/// <summary>
	/// 检测是否可以放置物品
	/// 目的：验证目标位置是否有足够空间且不越界
	/// 示例：L 形物品 [(0,0), (0,1), (1,1)] 放置到 (2,3) -> 检查 (2,3), (2,4), (3,4) 是否都为空
	/// 算法：1. 遍历物品形状 -> 2. 计算世界坐标 -> 3. 检查越界 -> 4. 检查占用状态
	/// </summary>
	public bool CanPlaceItem(ItemData item, Vector2I[] localShape, Vector2I targetPos)
	{
		if (item == null || localShape == null || localShape.Length == 0)
		{
			GD.PushWarning("CanPlaceItem: 物品或形状数据无效");
			return false;
		}
		
		foreach (var localOffset in localShape)
		{
			Vector2I worldPos = targetPos + localOffset;
			
			if (worldPos.X < 0 || worldPos.X >= Width || worldPos.Y < 0 || worldPos.Y >= Height)
			{
				return false;
			}
			
			int index = GetIndex(worldPos);
			if (_gridData[index] != null)
			{
				return false;
			}
		}
		
		return true;
	}
	
	public bool TryPlaceItem(ItemData item, Vector2I[] localShape, Vector2I targetPos)
	{
		if (!CanPlaceItem(item, localShape, targetPos))
		{
			return false;
		}
		
		foreach (var localOffset in localShape)
		{
			Vector2I worldPos = targetPos + localOffset;
			int index = GetIndex(worldPos);
			_gridData[index] = item;
		}
		
		OnItemPlacedAsObservable.OnNext((item, targetPos));
		GD.Print($"物品 {item.ItemID} 已放置在 {targetPos}");
		return true;
	}
	
	public void RemoveItem(ItemData item, Vector2I[] localShape, Vector2I position)
	{
		if (item == null || localShape == null)
		{
			GD.PushWarning("RemoveItem: 物品或形状数据无效");
			return;
		}
		
		foreach (var localOffset in localShape)
		{
			Vector2I worldPos = position + localOffset;
			
			if (worldPos.X >= 0 && worldPos.X < Width && worldPos.Y >= 0 && worldPos.Y < Height)
			{
				int index = GetIndex(worldPos);
				_gridData[index] = null;
			}
		}
		
		OnItemRemovedAsObservable.OnNext((item, position));
		GD.Print($"物品 {item.ItemID} 已从 {position} 移除");
	}
	
	public ItemData GetItemAt(Vector2I position)
	{
		if (position.X < 0 || position.X >= Width || position.Y < 0 || position.Y >= Height)
		{
			return null;
		}
		
		int index = GetIndex(position);
		return _gridData[index];
	}
	
	public void ClearGrid()
	{
		for (int i = 0; i < _gridData.Length; i++)
		{
			_gridData[i] = null;
		}
		
		GD.Print("网格已清空");
	}
	
	#endregion
	
	#region Helper Methods
	
	// 二维坐标转一维索引：index = y * Width + x
	private int GetIndex(Vector2I position)
	{
		return position.Y * Width + position.X;
	}
	
	// 一维索引转二维坐标（调试用）
	private Vector2I GetPosition(int index)
	{
		return new Vector2I(index % Width, index / Width);
	}
	
	#endregion
}

// 物品数据占位符类（实际项目中应扩展添加名称、图标、形状等属性）
public partial class ItemData : RefCounted
{
	public string ItemID { get; set; } = "item_unknown";
	public int InstanceID { get; set; } = -1;
	
	public ItemData()
	{
	}
	
	public ItemData(string itemId)
	{
		ItemID = itemId;
		InstanceID = (int)(GD.Randi() % 100000);
	}
}
