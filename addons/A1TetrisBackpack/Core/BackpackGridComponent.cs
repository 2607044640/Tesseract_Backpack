using Godot;
using R3;
using System;

// 背包网格核心组件 - 纯数据逻辑层
// 管理二维网格的物品占用状态，提供放置/移除逻辑，通过 R3 发布响应式事件
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
	
	public override void _EnterTree()
	{
		// 【架构修正】在 _EnterTree 中初始化核心数据和 Subjects
		_gridData = new ItemData[Width * Height];
		OnItemPlacedAsObservable = new Subject<(ItemData, Vector2I)>();
		OnItemRemovedAsObservable = new Subject<(ItemData, Vector2I)>();
	}
	
	public override void _Ready()
	{
		GD.Print($"BackpackGridComponent 初始化完成：{Width}x{Height} = {_gridData.Length} 格子");
	}
	
	public override void _ExitTree()
	{
		OnItemPlacedAsObservable?.Dispose();
		OnItemRemovedAsObservable?.Dispose();
	}
	
	#endregion
	
	#region Core Grid Logic
	
	// 检测是否可以放置物品：遍历物品形状 -> 计算世界坐标 -> 检查越界 -> 检查占用状态
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
	
	// 评估放置预览：返回每个格子的详细状态，不短路
	public System.Collections.Generic.List<(Vector2I GridPos, GridCellUI.CellState State)> EvaluatePlacementPreview(Vector2I[] localShape, Vector2I targetPos)
	{
		var result = new System.Collections.Generic.List<(Vector2I, GridCellUI.CellState)>();
		
		if (localShape == null || localShape.Length == 0)
		{
			return result;
		}
		
		foreach (var localOffset in localShape)
		{
			Vector2I worldPos = targetPos + localOffset;
			
			// 越界检查
			if (worldPos.X < 0 || worldPos.X >= Width || worldPos.Y < 0 || worldPos.Y >= Height)
			{
				result.Add((worldPos, GridCellUI.CellState.Invalid));
			}
			// 占用检查
			else
			{
				int index = GetIndex(worldPos);
				if (_gridData[index] != null)
				{
					result.Add((worldPos, GridCellUI.CellState.Invalid));
				}
				else
				{
					result.Add((worldPos, GridCellUI.CellState.Valid));
				}
			}
		}
		
		return result;
	}
	
	#endregion
	
	#region Helper Methods
	
	// 二维坐标转一维索引：index = y * Width + x
	private int GetIndex(Vector2I position)
	{
		return position.Y * Width + position.X;
	}
	
	private Vector2I GetPosition(int index)
	{
		return new Vector2I(index % Width, index / Width);
	}
	
	#endregion
}

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
