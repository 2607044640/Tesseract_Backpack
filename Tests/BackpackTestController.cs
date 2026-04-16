using Godot;
using Godot.Collections;

/// <summary>
/// 背包测试控制器
/// 初始化背包系统并注册测试物品
/// </summary>
public partial class BackpackTestController : Node
{
	public override void _Ready()
	{
		CallDeferred(MethodName.InitializeBackpack);
	}
	
	private void InitializeBackpack()
	{
		// 获取组件
		var controller = GetNodeOrNull<BackpackInteractionController>("%BackpackInteractionController");
		if (controller == null)
		{
			GD.PushError("BackpackTestController: 找不到 BackpackInteractionController");
			return;
		}
		
		var item = GetNodeOrNull<Control>("%Item");
		if (item == null)
		{
			GD.PushError("BackpackTestController: 找不到 Item");
			return;
		}
		
		// GridShapeComponent 是 Item 的子节点，使用 GetNode 而不是 GetNodeOrNull("%...")
		var shapeComponent = item.GetNodeOrNull<GridShapeComponent>("GridShapeComponent");
		if (shapeComponent == null)
		{
			GD.PushError("BackpackTestController: 找不到 GridShapeComponent");
			return;
		}
		
		// 创建测试物品数据（2x2 方块）
		var itemData = new ItemDataResource
		{
			ItemID = "test_item_001",
			ItemName = "测试物品",
			BaseShape = new Array<Vector2I>
			{
				new Vector2I(0, 0),
				new Vector2I(1, 0),
				new Vector2I(0, 1),
				new Vector2I(1, 1)
			}
		};
		
		shapeComponent.Data = itemData;
		
		// 注册物品到控制器
		controller.RegisterItem(item);
		
		GD.Print("═══════════════════════════════════════════════════════");
		GD.Print("BackpackTestController: 背包系统初始化完成");
		GD.Print($"  - 物品: {itemData.ItemName} ({itemData.ItemID})");
		GD.Print($"  - 形状: {itemData.BaseShape.Count} 个格子");
		GD.Print("  - 操作: 左键拖拽 | 右键旋转");
		GD.Print("═══════════════════════════════════════════════════════");
	}
}
