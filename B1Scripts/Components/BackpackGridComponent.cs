using Godot;
using R3;
using System;

/// <summary>
/// 背包网格核心组件 - 纯数据逻辑层
/// 
/// 职责：
/// - 管理二维网格的物品占用状态（使用一维数组实现）
/// - 提供物品放置/移除的核心逻辑
/// - 通过 R3 Subject 发布响应式事件流
/// 
/// 不负责：
/// - 物品的视觉表现（UI 层负责）
/// - 物品的具体属性（由 ItemData 扩展）
/// - 拖拽交互逻辑（由 UI 控制器负责）
/// 
/// 设计原则：
/// - 高复用性：可用于玩家背包、箱子、商店等任何网格容器
/// - 响应式：使用 R3 Subject 替代传统 C# event，支持 Rx 操作符链式处理
/// - 单一职责：只关心"哪个格子被哪个物品占用"的纯逻辑
/// </summary>
[GlobalClass]
public partial class BackpackGridComponent : Node
{
	#region Export Properties
	
	/// <summary>
	/// 网格宽度（列数）
	/// </summary>
	[Export] public int Width { get; set; } = 10;
	
	/// <summary>
	/// 网格高度（行数）
	/// </summary>
	[Export] public int Height { get; set; } = 6;
	
	#endregion
	
	#region Private Fields
	
	/// <summary>
	/// 一维数组模拟二维网格
	/// 索引计算公式：index = y * Width + x
	/// 
	/// 示例（3x3 网格）：
	/// [0,0] [1,0] [2,0]  →  索引 0, 1, 2
	/// [0,1] [1,1] [2,1]  →  索引 3, 4, 5
	/// [0,2] [1,2] [2,2]  →  索引 6, 7, 8
	/// 
	/// null 表示该格子为空，非 null 表示被物品占用
	/// </summary>
	private ItemData[] _gridData;
	
	#endregion
	
	#region R3 Reactive Streams
	
	/// <summary>
	/// 物品放置事件流
	/// 发送数据：(放置的物品, 放置的起始位置)
	/// 
	/// 订阅示例：
	/// component.OnItemPlacedAsObservable
	///     .Subscribe(x => GD.Print($"物品 {x.item.ItemID} 放置在 {x.position}"))
	///     .AddTo(disposables);
	/// </summary>
	public Subject<(ItemData item, Vector2I position)> OnItemPlacedAsObservable { get; private set; }
	
	/// <summary>
	/// 物品移除事件流
	/// 发送数据：(移除的物品, 移除的起始位置)
	/// 
	/// 订阅示例：
	/// component.OnItemRemovedAsObservable
	///     .Subscribe(x => GD.Print($"物品 {x.item.ItemID} 从 {x.position} 移除"))
	///     .AddTo(disposables);
	/// </summary>
	public Subject<(ItemData item, Vector2I position)> OnItemRemovedAsObservable { get; private set; }
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		// 初始化网格数据数组
		_gridData = new ItemData[Width * Height];
		
		// 初始化 R3 Subject（响应式事件流）
		OnItemPlacedAsObservable = new Subject<(ItemData, Vector2I)>();
		OnItemRemovedAsObservable = new Subject<(ItemData, Vector2I)>();
		
		GD.Print($"BackpackGridComponent 初始化完成：{Width}x{Height} = {_gridData.Length} 格子");
	}
	
	public override void _ExitTree()
	{
		// R3 规则：必须 Dispose Subject 防止内存泄漏
		OnItemPlacedAsObservable?.Dispose();
		OnItemRemovedAsObservable?.Dispose();
	}
	
	#endregion
	
	#region Core Grid Logic
	
	/// <summary>
	/// 检测是否可以在目标位置放置物品
	/// </summary>
	/// <param name="item">要放置的物品数据</param>
	/// <param name="localShape">物品占用的局部坐标数组（相对于物品左上角）
	/// 示例：L 形物品 = [(0,0), (0,1), (1,1)]
	///       2x1 物品 = [(0,0), (1,0)]
	/// </param>
	/// <param name="targetPos">目标位置（物品左上角的网格坐标）</param>
	/// <returns>true = 可以放置，false = 越界或已被占用</returns>
	public bool CanPlaceItem(ItemData item, Vector2I[] localShape, Vector2I targetPos)
	{
		if (item == null || localShape == null || localShape.Length == 0)
		{
			GD.PushWarning("CanPlaceItem: 物品或形状数据无效");
			return false;
		}
		
		// 遍历物品占用的所有格子
		foreach (var localOffset in localShape)
		{
			// 计算世界坐标（网格中的实际位置）
			Vector2I worldPos = targetPos + localOffset;
			
			// 检查 1：是否越界
			if (worldPos.X < 0 || worldPos.X >= Width || worldPos.Y < 0 || worldPos.Y >= Height)
			{
				return false; // 超出网格边界
			}
			
			// 检查 2：是否已被占用
			int index = GetIndex(worldPos);
			if (_gridData[index] != null)
			{
				return false; // 该格子已有物品
			}
		}
		
		return true; // 所有格子都可用
	}
	
	/// <summary>
	/// 尝试放置物品到网格
	/// </summary>
	/// <param name="item">要放置的物品</param>
	/// <param name="localShape">物品形状（局部坐标数组）</param>
	/// <param name="targetPos">目标位置</param>
	/// <returns>true = 放置成功，false = 放置失败</returns>
	public bool TryPlaceItem(ItemData item, Vector2I[] localShape, Vector2I targetPos)
	{
		// 先检测是否可以放置
		if (!CanPlaceItem(item, localShape, targetPos))
		{
			return false;
		}
		
		// 占用所有相关格子
		foreach (var localOffset in localShape)
		{
			Vector2I worldPos = targetPos + localOffset;
			int index = GetIndex(worldPos);
			_gridData[index] = item; // 标记该格子被此物品占用
		}
		
		// 【R3 响应式】发送物品放置事件
		OnItemPlacedAsObservable.OnNext((item, targetPos));
		
		GD.Print($"物品 {item.ItemID} 已放置在 {targetPos}");
		return true;
	}
	
	/// <summary>
	/// 从网格中移除物品
	/// </summary>
	/// <param name="item">要移除的物品</param>
	/// <param name="localShape">物品形状</param>
	/// <param name="position">物品当前位置</param>
	public void RemoveItem(ItemData item, Vector2I[] localShape, Vector2I position)
	{
		if (item == null || localShape == null)
		{
			GD.PushWarning("RemoveItem: 物品或形状数据无效");
			return;
		}
		
		// 清空所有相关格子
		foreach (var localOffset in localShape)
		{
			Vector2I worldPos = position + localOffset;
			
			// 安全检查：确保坐标在范围内
			if (worldPos.X >= 0 && worldPos.X < Width && worldPos.Y >= 0 && worldPos.Y < Height)
			{
				int index = GetIndex(worldPos);
				_gridData[index] = null; // 清空格子
			}
		}
		
		// 【R3 响应式】发送物品移除事件
		OnItemRemovedAsObservable.OnNext((item, position));
		
		GD.Print($"物品 {item.ItemID} 已从 {position} 移除");
	}
	
	/// <summary>
	/// 获取指定位置的物品数据
	/// </summary>
	/// <param name="position">网格坐标</param>
	/// <returns>物品数据，如果为空则返回 null</returns>
	public ItemData GetItemAt(Vector2I position)
	{
		if (position.X < 0 || position.X >= Width || position.Y < 0 || position.Y >= Height)
		{
			return null; // 越界返回 null
		}
		
		int index = GetIndex(position);
		return _gridData[index];
	}
	
	/// <summary>
	/// 清空整个网格
	/// </summary>
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
	
	/// <summary>
	/// 将二维坐标转换为一维数组索引
	/// 
	/// 公式：index = y * Width + x
	/// 
	/// 原理：
	/// - 每一行有 Width 个元素
	/// - 第 y 行的起始索引 = y * Width
	/// - 加上 x 偏移量得到最终索引
	/// 
	/// 示例（Width=3）：
	/// (0,0) → 0*3+0 = 0
	/// (2,0) → 0*3+2 = 2
	/// (1,1) → 1*3+1 = 4
	/// (2,2) → 2*3+2 = 8
	/// </summary>
	private int GetIndex(Vector2I position)
	{
		return position.Y * Width + position.X;
	}
	
	/// <summary>
	/// 将一维数组索引转换为二维坐标（调试用）
	/// 
	/// 公式：
	/// x = index % Width  （取余数得到列）
	/// y = index / Width  （整除得到行）
	/// </summary>
	private Vector2I GetPosition(int index)
	{
		return new Vector2I(index % Width, index / Width);
	}
	
	#endregion
}

/// <summary>
/// 物品数据占位符类
/// 
/// 实际项目中应扩展此类，添加：
/// - 物品名称、描述、图标路径
/// - 物品类型、稀有度、价格
/// - 可堆叠数量、耐久度
/// - 自定义形状数据（Vector2I[]）
/// 
/// 示例扩展：
/// public class AdvancedItemData : ItemData
/// {
///     public string Name { get; set; }
///     public Texture2D Icon { get; set; }
///     public Vector2I[] Shape { get; set; } = { new(0,0), new(1,0) }; // 2x1
/// }
/// </summary>
public partial class ItemData : RefCounted
{
	/// <summary>
	/// 物品唯一标识符
	/// </summary>
	public string ItemID { get; set; } = "item_unknown";
	
	/// <summary>
	/// 物品实例 ID（用于区分同类物品的不同实例）
	/// </summary>
	public int InstanceID { get; set; } = -1;
	
	public ItemData()
	{
	}
	
	public ItemData(string itemId)
	{
		ItemID = itemId;
		InstanceID = (int)(GD.Randi() % 100000); // 简单的实例 ID 生成
	}
}
