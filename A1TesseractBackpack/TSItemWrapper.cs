using Godot;
using System;

/// <summary>
/// TSItem 场景包装器 - 使用接口解耦的事件模式传递数据
/// 
/// 用途：
/// - 在父场景中可以直接设置 ItemDataResource
/// - 通过 IItemDataProvider 接口在 _Ready 阶段传递给 GridShapeComponent
/// - GridShapeComponent 会自动处理 UI 尺寸调整
/// 
/// 使用方式：
/// 1. 在 BackpackTest 场景中实例化 TSItem
/// 2. 在 Inspector 中设置 Data 属性
/// 3. 运行时自动初始化（GridShapeComponent 订阅事件并处理所有逻辑）
/// 
/// 架构模式：Observer Pattern + Interface Segregation
/// - TSItemWrapper 实现 IItemDataProvider 接口
/// - GridShapeComponent 依赖接口而非具体类型
/// - 解决 Godot _Ready() 生命周期顺序问题（子节点先于父节点执行）
/// - 解耦：任何实现 IItemDataProvider 的父节点都可以与 GridShapeComponent 配合
/// </summary>
[GlobalClass]
public partial class TSItemWrapper : Control, IItemDataProvider
{
	/// <summary>
	/// 物品数据资源（可在 Inspector 中编辑）
	/// </summary>
	[Export] public ItemDataResource Data { get; set; }
	
	/// <summary>
	/// 数据初始化事件 - 在 _Ready() 中触发，传递 ItemDataResource 给订阅者
	/// </summary>
	public event Action<ItemDataResource> DataInitialized;
	
	public override void _Ready()
	{
		// 触发事件，传递 Data 给所有订阅者（GridShapeComponent）
		if (Data != null)
		{
			DataInitialized?.Invoke(Data);
			GD.Print($"[{Name}] TSItemWrapper._Ready: 已触发 DataInitialized 事件，Data = {Data.ItemID}");
		}
		else
		{
			GD.PushWarning($"[{Name}] TSItemWrapper._Ready: Data 未设置，将使用默认 1x1 形状");
			// 即使 Data 为 null，也触发事件让 GridShapeComponent 使用默认形状
			DataInitialized?.Invoke(null);
		}
	}
	
}
