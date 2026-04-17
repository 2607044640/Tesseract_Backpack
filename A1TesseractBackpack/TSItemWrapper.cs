using Godot;

/// <summary>
/// TSItem 场景包装器 - 暴露 Data 属性给外部场景
/// 
/// 用途：
/// - 在父场景中可以直接设置 ItemDataResource
/// - 自动传递给内部的 GridShapeComponent
/// - GridShapeComponent 会自动处理 UI 尺寸调整
/// 
/// 使用方式：
/// 1. 在 BackpackTest 场景中实例化 TSItem
/// 2. 在 Inspector 中设置 Data 属性
/// 3. 运行时自动初始化（GridShapeComponent 处理所有逻辑）
/// </summary>
[GlobalClass]
public partial class TSItemWrapper : Control
{
	/// <summary>
	/// 物品数据资源（可在 Inspector 中编辑）
	/// </summary>
	[Export] public ItemDataResource Data { get; set; }
	
	private GridShapeComponent _shapeComponent;
	
	public override void _Ready()
	{
		// 获取 GridShapeComponent
		_shapeComponent = GetNode<GridShapeComponent>("GridShapeComponent");
		
		if (_shapeComponent == null)
		{
			GD.PushError($"[{Name}] TSItemWrapper: 找不到 GridShapeComponent");
			return;
		}
		
		// 如果外部设置了 Data，传递给 GridShapeComponent
		if (Data != null)
		{
			_shapeComponent.Data = Data;
			GD.Print($"[{Name}] TSItemWrapper: 已设置 Data = {Data.ItemID}");
		}
		else
		{
			GD.PushWarning($"[{Name}] TSItemWrapper: Data 未设置，将使用默认 1x1 形状");
		}
	}
	
	/// <summary>
	/// 获取 GridShapeComponent 引用（供外部使用）
	/// </summary>
	public GridShapeComponent GetShapeComponent()
	{
		return _shapeComponent;
	}
}
