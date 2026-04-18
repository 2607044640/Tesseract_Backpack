using System;

/// <summary>
/// 物品数据提供者接口
/// 目的：解耦 GridShapeComponent 与具体父节点类型的依赖
/// 示例：TSItemWrapper 实现此接口，GridShapeComponent 只依赖接口
/// 算法：1. 父节点实现接口并定义事件 -> 2. 子节点订阅接口事件 -> 3. 父节点触发事件传递数据
/// </summary>
public interface IItemDataProvider
{
	/// <summary>
	/// 数据初始化事件 - 在父节点 _Ready() 中触发
	/// </summary>
	event Action<ItemDataResource> DataInitialized;
}
