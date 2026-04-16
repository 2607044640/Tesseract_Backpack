using Godot;
using R3;
using System.Collections.Generic;

/// <summary>
/// 背包交互控制器 - MVC 架构中的 Controller 层
/// 
/// 职责：
/// - 协调物品的拾取、放置、旋转逻辑
/// - 管理拖拽状态（原始位置、网格坐标）
/// - 实现吸附机制（放置成功时对齐网格）
/// - 实现回弹机制（放置失败时恢复原位）
/// - 防止"自我占用"问题（拾取时先从网格移除）
/// 
/// 架构定位：
/// - Controller 层：连接 Model（LogicGrid）和 View（ViewGrid）
/// - 事件驱动：订阅物品的拖拽事件并响应
/// - 状态管理：维护每个物品的拖拽状态
/// 
/// 工作流程：
/// 1. 注册物品 → 订阅拖拽事件
/// 2. 拾取（drag_start）→ 记录原位置 + 从网格移除
/// 3. 旋转（右键）→ 调用形状组件旋转
/// 4. 放置（drag_end）→ 验证位置 + 吸附或回弹
/// 
/// 关键机制：
/// 
/// **防自我占用：**
/// 拾取时立即从 LogicGrid 移除，避免放置时检测到自己占用的格子。
/// 
/// **回弹机制：**
/// 放置失败时（越界或碰撞），强制将物品放回原位置。
/// 
/// **吸附机制：**
/// 放置成功时，将物品 GlobalPosition 对齐到网格坐标。
/// 
/// 使用示例：
/// ```csharp
/// // 在背包 UI 初始化时
/// var controller = GetNode<BackpackInteractionController>("Controller");
/// controller.LogicGrid = logicGrid;
/// controller.ViewGrid = viewGrid;
/// 
/// // 注册物品
/// foreach (var item in GetChildren())
/// {
///     controller.RegisterItem(item);
/// }
/// ```
/// </summary>
[GlobalClass]
public partial class BackpackInteractionController : Node
{
	#region Export Properties
	
	/// <summary>
	/// 逻辑网格组件引用（Model 层）
	/// </summary>
	[Export] public NodePath LogicGridPath { get; set; } = "%BackpackGridComponent";
	
	/// <summary>
	/// UI 网格视图组件引用（View 层）
	/// </summary>
	[Export] public NodePath ViewGridPath { get; set; } = "%BackpackPanel";
	
	#endregion
	
	#region Private Fields
	
	private BackpackGridComponent LogicGrid;
	private BackpackGridUIComponent ViewGrid;
	
	/// <summary>
	/// 物品拖拽状态字典
	/// Key: 物品实体节点
	/// Value: 拖拽状态信息
	/// </summary>
	private Dictionary<Node, ItemDragState> _dragStates = new Dictionary<Node, ItemDragState>();
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		LogicGrid = GetNodeOrNull<BackpackGridComponent>(LogicGridPath);
		if (LogicGrid == null)
		{
			GD.PushError($"[{Name}] LogicGrid not found: {LogicGridPath}");
			return;
		}
		
		ViewGrid = GetNodeOrNull<BackpackGridUIComponent>(ViewGridPath);
		if (ViewGrid == null)
		{
			GD.PushError($"[{Name}] ViewGrid not found: {ViewGridPath}");
			return;
		}
		
		GD.Print("BackpackInteractionController: 初始化完成");
	}
	
	#endregion
	
	#region Item Registration
	
	/// <summary>
	/// 注册物品到控制器
	/// 目的：订阅物品的拖拽事件，使其能够与背包交互
	/// 示例：controller.RegisterItem(swordItem) → 剑可以被拖拽到背包
	/// 算法：1. 获取必需组件 → 2. 订阅拖拽事件 → 3. 绑定到物品生命周期
	/// </summary>
	public void RegisterItem(Node itemEntity)
	{
		if (itemEntity == null)
		{
			GD.PushWarning("BackpackInteractionController: 尝试注册空物品");
			return;
		}
		
		// 1. 获取必需组件
		var draggable = itemEntity.GetNodeOrNull<DraggableItemComponent>("%DraggableItemComponent");
		if (draggable == null)
		{
			draggable = itemEntity.FindChild("*", true, false) as DraggableItemComponent;
			if (draggable == null)
			{
				GD.PushError($"BackpackInteractionController: 物品 {itemEntity.Name} 缺少 DraggableItemComponent");
				return;
			}
		}
		
		var shapeComponent = itemEntity.GetNodeOrNull<GridShapeComponent>("%GridShapeComponent");
		if (shapeComponent == null)
		{
			shapeComponent = itemEntity.FindChild("*", true, false) as GridShapeComponent;
			if (shapeComponent == null)
			{
				GD.PushError($"BackpackInteractionController: 物品 {itemEntity.Name} 缺少 GridShapeComponent");
				return;
			}
		}
		
		// 获取物品的 Control 节点（用于设置位置）
		Control itemControl = itemEntity as Control;
		if (itemControl == null)
		{
			GD.PushError($"BackpackInteractionController: 物品 {itemEntity.Name} 不是 Control 节点");
			return;
		}
		
		// 2. 订阅拖拽事件（使用 R3）
		// 【关键】使用 .AddTo(itemEntity) 确保物品销毁时自动取消订阅
		
		draggable.OnDragStartedAsObservable
			.Subscribe(_ => HandleItemPickedUp(itemEntity, itemControl, shapeComponent))
			.AddTo(itemEntity);
		
		draggable.OnDragEndedAsObservable
			.Subscribe(_ => HandleItemDropped(itemEntity, itemControl, shapeComponent))
			.AddTo(itemEntity);
		
		draggable.OnRotateRequestedAsObservable
			.Subscribe(_ => HandleItemRotated(itemEntity, shapeComponent))
			.AddTo(itemEntity);
		
		// 3. 【内存安全】监听物品销毁事件，防止字典持有悬空引用
		// 如果物品在拖拽过程中被销毁（如背包着火），立即从字典中移除
		itemEntity.TreeExited += () =>
		{
			if (_dragStates.ContainsKey(itemEntity))
			{
				_dragStates.Remove(itemEntity);
				GD.Print($"物品 {itemEntity.Name} 意外销毁，已从拖拽状态中清理");
			}
		};
		
		GD.Print($"BackpackInteractionController: 已注册物品 {itemEntity.Name}");
	}
	
	/// <summary>
	/// 批量注册多个物品
	/// </summary>
	public void RegisterItems(IEnumerable<Node> items)
	{
		foreach (var item in items)
		{
			RegisterItem(item);
		}
	}
	
	#endregion
	
	#region Drag Event Handlers
	
	/// <summary>
	/// 处理物品拾取（拖拽开始）
	/// 目的：记录原始状态并从网格移除，防止"自我占用"
	/// 示例：拾起剑 → 记录位置 (320, 192) 和网格坐标 (5, 3) → 从网格移除
	/// 算法：1. 记录当前位置 → 2. 计算网格坐标 → 3. 从逻辑网格移除 → 4. 保存状态
	/// </summary>
	private void HandleItemPickedUp(Node itemEntity, Control itemControl, GridShapeComponent shapeComponent)
	{
		// 1. 记录当前全局位置
		Vector2 originalGlobalPos = itemControl.GlobalPosition;
		
		// 2. 计算当前网格坐标
		Vector2I originalGridPos = ViewGrid.GlobalToGridPosition(originalGlobalPos);
		
		// 3. 从逻辑网格移除（防止"自我占用"）
		// 【关键机制】拾取时立即移除，这样放置时不会检测到自己占用的格子
		if (ViewGrid.IsValidGridPosition(originalGridPos))
		{
			LogicGrid.RemoveItem(
				new ItemData(shapeComponent.Data?.ItemID ?? "unknown"),
				shapeComponent.CurrentLocalCells,
				originalGridPos
			);
			
			GD.Print($"物品 {itemEntity.Name} 已从网格 {originalGridPos} 移除");
		}
		
		// 4. 保存拖拽状态
		var dragState = new ItemDragState
		{
			OriginalGlobalPos = originalGlobalPos,
			OriginalGridPos = originalGridPos,
			ShapeComponent = shapeComponent,
			ItemControl = itemControl
		};
		
		_dragStates[itemEntity] = dragState;
		
		GD.Print($"物品 {itemEntity.Name} 拾取：原位置 {originalGlobalPos}，网格坐标 {originalGridPos}");
	}
	
	/// <summary>
	/// 处理物品放置（拖拽结束）
	/// 目的：验证放置位置，成功则吸附，失败则回弹
	/// 示例：放置剑到 (400, 256) → 转换为网格 (6, 4) → 检测合法 → 吸附到 (384, 256)
	/// 算法：1. 获取目标位置 → 2. 检查范围 → 3. 尝试放置 → 4. 吸附或回弹
	/// </summary>
	private void HandleItemDropped(Node itemEntity, Control itemControl, GridShapeComponent shapeComponent)
	{
		// 检查是否有拖拽状态
		if (!_dragStates.TryGetValue(itemEntity, out var dragState))
		{
			GD.PushWarning($"物品 {itemEntity.Name} 没有拖拽状态记录");
			return;
		}
		
		// 1. 获取当前鼠标位置
		// 使用 ViewGrid.GetGlobalMousePosition() 而非 GetViewport().GetMousePosition()
		// 原因：自动处理 Camera2D 移动和 CanvasLayer 缩放，确保坐标准确
		Vector2 mousePos = ViewGrid.GetGlobalMousePosition();
		
		// 2. 检查鼠标是否在背包 UI 范围内
		bool isInBackpack = ViewGrid.GetGlobalRect().HasPoint(mousePos);
		
		if (!isInBackpack)
		{
			// 【回弹机制】鼠标不在背包内，回弹到原位置
			PerformBounceBack(itemEntity, dragState);
			return;
		}
		
		// 3. 计算目标网格坐标
		Vector2I targetGridPos = ViewGrid.GlobalToGridPosition(mousePos);
		
		// 4. 尝试放置到逻辑网格
		var itemData = new ItemData(shapeComponent.Data?.ItemID ?? "unknown");
		bool placementSuccess = LogicGrid.TryPlaceItem(
			itemData,
			shapeComponent.CurrentLocalCells,
			targetGridPos
		);
		
		if (placementSuccess)
		{
			// 【吸附机制】放置成功，对齐到网格
			PerformSnapToGrid(itemEntity, itemControl, targetGridPos);
		}
		else
		{
			// 【回弹机制】放置失败（碰撞或越界），回弹到原位置
			PerformBounceBack(itemEntity, dragState);
		}
		
		// 清理拖拽状态
		_dragStates.Remove(itemEntity);
	}
	
	/// <summary>
	/// 处理物品旋转
	/// </summary>
	private void HandleItemRotated(Node itemEntity, GridShapeComponent shapeComponent)
	{
		// 调用形状组件的旋转方法
		shapeComponent.Rotate90();
		
		GD.Print($"物品 {itemEntity.Name} 已旋转 90°");
		
		// TODO: 可选 - 检查旋转后是否仍然合法
		// 如果在拖拽中旋转导致越界，可以考虑撤销旋转或调整位置
	}
	
	#endregion
	
	#region Placement Logic
	
	/// <summary>
	/// 执行吸附到网格
	/// 目的：将物品精确对齐到网格位置
	/// 算法：1. 计算网格局部坐标 → 2. 转换为全局坐标 → 3. 更新物品位置
	/// </summary>
	private void PerformSnapToGrid(Node itemEntity, Control itemControl, Vector2I gridPos)
	{
		// 1. 计算网格位置的局部像素坐标
		Vector2 localPos = ViewGrid.GridToLocalPosition(gridPos);
		
		// 2. 转换为全局坐标
		Vector2 globalPos = ViewGrid.GlobalPosition + localPos;
		
		// 3. 更新物品位置
		itemControl.GlobalPosition = globalPos;
		
		GD.Print($"物品 {itemEntity.Name} 吸附到网格 {gridPos}，全局位置 {globalPos}");
	}
	
	/// <summary>
	/// 执行回弹到原位置
	/// 目的：放置失败时恢复物品到拾取前的位置
	/// 算法：1. 恢复全局位置 → 2. 强制放回逻辑网格 → 3. 记录日志
	/// </summary>
	private void PerformBounceBack(Node itemEntity, ItemDragState dragState)
	{
		// 1. 恢复原始全局位置
		dragState.ItemControl.GlobalPosition = dragState.OriginalGlobalPos;
		
		// 2. 强制放回逻辑网格的原位置
		// 【关键】这里不检查返回值，因为原位置一定是合法的（拾取前就在那里）
		var itemData = new ItemData(dragState.ShapeComponent.Data?.ItemID ?? "unknown");
		LogicGrid.TryPlaceItem(
			itemData,
			dragState.ShapeComponent.CurrentLocalCells,
			dragState.OriginalGridPos
		);
		
		GD.Print($"物品 {itemEntity.Name} 回弹到原位置 {dragState.OriginalGlobalPos}，网格坐标 {dragState.OriginalGridPos}");
	}
	
	#endregion
	
	#region Helper Methods
	
	/// <summary>
	/// 检查物品是否正在被拖拽
	/// </summary>
	public bool IsItemBeingDragged(Node itemEntity)
	{
		return _dragStates.ContainsKey(itemEntity);
	}
	
	/// <summary>
	/// 获取物品的拖拽状态
	/// </summary>
	public ItemDragState GetDragState(Node itemEntity)
	{
		return _dragStates.TryGetValue(itemEntity, out var state) ? state : null;
	}
	
	/// <summary>
	/// 清除所有拖拽状态（用于重置）
	/// </summary>
	public void ClearAllDragStates()
	{
		_dragStates.Clear();
		GD.Print("BackpackInteractionController: 已清除所有拖拽状态");
	}
	
	#endregion
	
	#region Inner Classes
	
	/// <summary>
	/// 物品拖拽状态数据
	/// </summary>
	public class ItemDragState
	{
		/// <summary>
		/// 原始全局位置（用于回弹）
		/// </summary>
		public Vector2 OriginalGlobalPos { get; set; }
		
		/// <summary>
		/// 原始网格坐标（用于回弹时放回逻辑网格）
		/// </summary>
		public Vector2I OriginalGridPos { get; set; }
		
		/// <summary>
		/// 形状组件引用（用于获取当前形状）
		/// </summary>
		public GridShapeComponent ShapeComponent { get; set; }
		
		/// <summary>
		/// 物品 Control 节点引用（用于设置位置）
		/// </summary>
		public Control ItemControl { get; set; }
	}
	
	#endregion
}
