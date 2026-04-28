using Godot;
using R3;
using System.Collections.Generic;

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
/// - Controller 层：连接 Model（BackpackGridComp）和 View（ViewGrid）
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
/// 拾取时立即从 BackpackGridComp 移除，避免放置时检测到自己占用的格子。
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
/// controller.BackpackGridComp = backpackGridComp;
/// controller.ViewGrid = viewGrid;
///
/// // 注册物品
/// foreach (var item in GetChildren())
/// {
///     controller.RegisterItem(item);
/// }
/// ```
[GlobalClass]
public partial class BackpackInteractionController : Node
{
	#region Export Properties

	/// 逻辑网格组件引用（Model 层）
	[Export] public NodePath BackpackGridComponentPath { get; set; } = "%BackpackGridComponent";

	/// UI 网格视图组件引用（View 层）
	[Export] public NodePath ViewGridPath { get; set; } = "%BackpackGridUIComponent";

	/// 物品容器路径，_Ready() 时自动注册其所有子节点
	[Export] public NodePath ItemsContainerPath { get; set; } = "";

	/// 生成战利品的区域路径
	[Export] public NodePath LootSpawnAreaPath { get; set; } = "";

	#endregion

	#region Private Fields

	private BackpackGridComponent BackpackGridComp;
	private BackpackGridUIComponent BackpackGridUIComp;

	// 当前拖拽状态（单一物品，玩家只有一个鼠标）
	private ItemDragState _currentDrag;

	/// CompositeDisposable - 管理所有订阅
	private CompositeDisposable _disposables = new CompositeDisposable();

	#endregion

	#region Godot Lifecycle

	public override void _Ready()
	{
		BackpackGridComp = GetNodeOrNull<BackpackGridComponent>(BackpackGridComponentPath);
		if (BackpackGridComp == null)
		{
			GD.PushError($"[{Name}] BackpackGridComp not found: {BackpackGridComponentPath}");
			return;
		}

		BackpackGridUIComp = GetNodeOrNull<BackpackGridUIComponent>(ViewGridPath);
		if (BackpackGridUIComp == null)
		{
			GD.PushError($"[{Name}] ViewGrid not found: {ViewGridPath}");
			return;
		}

		// 自动注册 ItemsContainer 内所有物品
		if (!string.IsNullOrEmpty(ItemsContainerPath))
		{
			var container = GetNodeOrNull(ItemsContainerPath);
			if (container != null)
			{
				foreach (Node child in container.GetChildren())
					RegisterItem(child);
			}
			else
			{
				GD.PushWarning($"[{Name}] ItemsContainer not found: {ItemsContainerPath}");
			}
		}

		GD.Print("BackpackInteractionController: 初始化完成");
	}


	public override void _ExitTree()
	{
		_disposables?.Dispose();
	}

	#endregion

	#region Item Registration

	/// 注册物品到控制器
	/// 目的：订阅物品的拖拽事件，使其能够与背包交互
	/// 示例：controller.RegisterItem(swordItem) → 剑可以被拖拽到背包
	/// 算法：1. 获取必需组件 → 2. 订阅拖拽事件 → 3. 绑定到物品生命周期
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
			.Subscribe(_ => HandleItemPickedUp(itemEntity, itemControl, shapeComponent, draggable))
			.AddTo(itemEntity);

		draggable.OnDragEndedAsObservable
			.Subscribe(_ => HandleItemDropped(itemEntity, itemControl, shapeComponent))
			.AddTo(itemEntity);

		draggable.OnRotateRequestedAsObservable
			.Subscribe(_ => HandleItemRotated(itemEntity, shapeComponent))
			.AddTo(itemEntity);

		GD.Print($"BackpackInteractionController: 已注册物品 {itemEntity.Name}");
	}

	/// 批量注册多个物品
	public void RegisterItems(IEnumerable<Node> items)
	{
		foreach (var item in items)
		{
			RegisterItem(item);
		}
	}

	#endregion

	#region Drag Event Handlers

	/// 处理物品拾取（拖拽开始）
	/// 目的：记录原始状态并从网格移除，防止"自我占用"；启动 R3 预览流
	/// 算法：1. 记录位置 → 2. 从网格移除 → 3. 保存状态 → 4. 启动预览流（TakeUntil 自动停止）
	private void HandleItemPickedUp(Node itemEntity, Control itemControl, GridShapeComponent shapeComponent, DraggableItemComponent draggable)
	{
		var itemPhysics = itemEntity.GetNodeOrNull<ItemPhysicsComponent>("%PhysicsProxy");
		if (itemPhysics != null)
		{
			itemPhysics.DisablePhysics();
		}

		Vector2 originalGlobalPos = itemControl.GlobalPosition;
		Vector2I originalGridPos = BackpackGridUIComp.GlobalToGridPosition(originalGlobalPos);

		// 从逻辑网格移除，防止"自我占用"
		// CRITICAL CLAMP FIX: Do NOT blindly trust GlobalToGridPosition as it clamps!
		// Only call RemoveItem IF the item was actually inside the grid.
		bool wasInBackpack = BackpackGridUIComp.GetGlobalRect().HasPoint(originalGlobalPos);
		if (wasInBackpack && BackpackGridUIComp.IsValidGridPosition(originalGridPos))
		{
			BackpackGridComp.RemoveItem(
				GetItemData(shapeComponent),
				shapeComponent.CurrentLocalCells,
				originalGridPos
			);
		}

		_currentDrag = new ItemDragState
		{
			OriginalGlobalPos = originalGlobalPos,
			OriginalGridPos = originalGridPos,
			ShapeComponent = shapeComponent,
			ItemControl = itemControl
		};

		// 【架构重构】在拾取瞬间预计算被抓取的单元格索引
		var followComp = GetFollowComp(itemEntity);

		if (followComp != null && shapeComponent.CurrentLocalCells != null)
		{
			// 1. 确保跟随组件已初始化抓取偏移量
			followComp.CalculateGrabOffsetFromMouse();

			// 2. 将物理抓取偏移转换为逻辑网格坐标
			Vector2 localGrabPos = -followComp.GrabOffset;
			Vector2I grabbedCellPos = new Vector2I(
				Mathf.FloorToInt(localGrabPos.X / BackpackGridUIComp.CellSize.X),
				Mathf.FloorToInt(localGrabPos.Y / BackpackGridUIComp.CellSize.Y)
			);

			// 3. 在当前形状中查找匹配该坐标的单元格索引
			int grabbedIndex = System.Array.IndexOf(shapeComponent.CurrentLocalCells, grabbedCellPos);

			// 容错处理：如果鼠标正好抓在了格子间的缝隙或空位，默认为第一个格子
			if (grabbedIndex == -1) grabbedIndex = 0;

			// 4. 持久化到拖拽状态中，供后续旋转逻辑使用
			_currentDrag.GrabbedCellIndex = grabbedIndex;
		}

		// 【视觉修正】锁定 Hover 状态，防止拖拽时闪烁白光
		var itemGroupCtrl = GetItemGroupCtrl(itemEntity);
		if (itemGroupCtrl != null)
		{
			itemGroupCtrl.IsDragging = true;
			itemGroupCtrl.ResetCellsVisualState();
		}

		// ── 预览流：双管齐下，Merge 位置流 + 形状流 ──────────────────────────────
		// posStream：鼠标跨越网格边界时触发（DistinctUntilChanged 过滤亚格子抖动）
		var posStream = Observable.EveryUpdate()
			.Select(_ => BackpackGridUIComp.GetGlobalMousePosition())
			.Select(mousePos => {
				Vector2I mouseGridPos = BackpackGridUIComp.GlobalToGridPosition(mousePos);
				// 纯数学计算：目标位置 = 鼠标所在格子 - 被抓格子的逻辑偏移
				Vector2I targetTopLeftGridPos = mouseGridPos - GetGrabbedCellLogicalOffset(_currentDrag, shapeComponent);

				return (
					inBounds: BackpackGridUIComp.GetGlobalRect().HasPoint(mousePos),
					gridPos:  targetTopLeftGridPos
				);
			})
			.DistinctUntilChanged(); // 只比较坐标元组，不碰形状数组，避免引用比较陷阱

		// shapeStream：物品旋转时强制刷新，绕过 DistinctUntilChanged
		// 需要伪装成与 posStream 相同的元组类型，才能让 Merge 合并
		var shapeStream = shapeComponent.OnShapeChangedAsObservable
			.Select(_ =>
			{
				var mp = BackpackGridUIComp.GetGlobalMousePosition();
				Vector2I mouseGridPos = BackpackGridUIComp.GlobalToGridPosition(mp);
				Vector2I targetTopLeftGridPos = mouseGridPos - GetGrabbedCellLogicalOffset(_currentDrag, shapeComponent);

				return (
					inBounds: BackpackGridUIComp.IsPointInside(mp),
					gridPos:  targetTopLeftGridPos
				);
			});

		// 合并：任一流发射 → 执行同一段预览逻辑


		Observable.Merge(posStream, shapeStream)
			.TakeUntil(draggable.OnDragEndedAsObservable)
			.Subscribe(state =>
			{
				if (!state.inBounds)
				{
					BackpackGridUIComp.ClearPreview();
					itemGroupCtrl?.ResetCellsVisualState();
				}
				else
				{
					var previewData = BackpackGridComp.EvaluatePlacementPreview(
						shapeComponent.CurrentLocalCells, state.gridPos);
					
					BackpackGridUIComp.ShowPreview(previewData);
					
					// 同步更新物品自己的格子颜色
					var cellStates = new GridCellUI.CellState[previewData.Count];
					for (int i = 0; i < previewData.Count; i++) {
						cellStates[i] = previewData[i].State;
					}
					itemGroupCtrl?.UpdateCellsVisualState(cellStates);
				}
			})
			.AddTo(itemEntity);

		GD.Print($"物品 {itemEntity.Name} 拾取：原位置 {originalGlobalPos}，网格坐标 {originalGridPos}");
	}

	/// 处理物品放置（拖拽结束）
	/// 目的：验证放置位置，成功则吸附，失败则回弹
	/// 算法：1. 清除预览 → 2. 检查范围 → 3. 尝试放置 → 4. 吸附或回弹
	private void HandleItemDropped(Node itemEntity, Control itemControl, GridShapeComponent shapeComponent)
	{
		BackpackGridUIComp.ClearPreview();

		// 【视觉修正】解锁 Hover 状态
		var itemGroupCtrl = GetItemGroupCtrl(itemEntity);
		if (itemGroupCtrl != null)
		{
			itemGroupCtrl.IsDragging = false;
			itemGroupCtrl.ResetCellsVisualState();
		}


		if (_currentDrag == null)
		{
			GD.PushWarning($"物品 {itemEntity.Name} 没有拖拽状态记录");
			return;
		}

		var dragState = _currentDrag;
		_currentDrag = null;

		// 使用 ViewGrid.GetGlobalMousePosition() 确保在 Camera2D/CanvasLayer 下坐标准确
		Vector2 mousePos = BackpackGridUIComp.GetGlobalMousePosition();
		bool isInBackpack = BackpackGridUIComp.IsPointInside(mousePos);

		if (!isInBackpack)
		{
			// Dropped OUTSIDE: Do NOT bounce back.
			itemControl.GlobalPosition = mousePos;
			var itemPhysics = itemEntity.GetNodeOrNull<ItemPhysicsComponent>("%PhysicsProxy");
			if (itemPhysics != null)
			{
				itemPhysics.EnablePhysics();
			}
			return;
		}

		Vector2I mouseGridPos = BackpackGridUIComp.GlobalToGridPosition(mousePos);
		
		// 纯数学逻辑计算左上角网格坐标
		Vector2I targetGridPos = mouseGridPos - GetGrabbedCellLogicalOffset(dragState, shapeComponent);
		var itemData = GetItemData(shapeComponent);
		bool placementSuccess = BackpackGridComp.TryPlaceItem(
			itemData,
			shapeComponent.CurrentLocalCells,
			targetGridPos
		);

		if (placementSuccess)
		{
			PerformSnapToGrid(itemEntity, itemControl, targetGridPos);
		}
		else
		{
			bool originallyInBackpack = BackpackGridUIComp.GetGlobalRect().HasPoint(dragState.OriginalGlobalPos);
			if (originallyInBackpack)
			{
				PerformBounceBack(itemEntity, dragState);
			}
			else
			{
				var itemPhysics = itemEntity.GetNodeOrNull<ItemPhysicsComponent>("%PhysicsProxy");
				if (itemPhysics != null)
				{
					itemPhysics.EnablePhysics();
				}
			}
		}
	}

	/// 处理物品旋转
	/// 职责：仅作为指挥官发送逻辑指令（Rotate90）和动画指令（AnimateRotation90）
	private void HandleItemRotated(Node itemEntity, GridShapeComponent shapeComponent)
	{
		if (_currentDrag == null) return;

		// 1. 获取必需组件
		var followComp = GetFollowComp(itemEntity);
		var tweenComp = GetTweenComp(itemEntity);
		var interactionArea = itemEntity.GetNodeOrNull<Control>("%InteractionArea");

		if (followComp == null || tweenComp == null || interactionArea == null)
		{
			GD.PushWarning($"[Rotation] {itemEntity.Name} 缺少必要动画、跟随或交互组件！");
			return;
		}

		// 2. 指令 A：执行底层逻辑旋转
		shapeComponent.Rotate90();

		// 3. 准备数据：计算新的抓取点中心坐标 (空间换算交给 ViewGrid)
		Vector2I newGrabbedCell = shapeComponent.CurrentLocalCells[_currentDrag.GrabbedCellIndex];
		Vector2 newPivot = BackpackGridUIComp.GetCellCenterLocalPos(newGrabbedCell);

		// 4. 指令 B：同步物理抓取偏移 (消除坐标跳变)
		followComp.GrabOffset = -newPivot;

		// 5. 指令 C：执行视觉补间动画 (底层补间魔法由 TweenComp 处理)
		tweenComp.PlayRotationAnimation(interactionArea, newPivot);

		GD.Print($"[{itemEntity.Name}] 指令：逻辑旋转 + 视觉补间 (Pivot: {newPivot})");
	}
	#endregion

	#region Placement Logic

	/// 执行吸附到网格
	/// 目的：将物品精确对齐到网格位置
	/// 算法：1. 计算网格局部坐标 → 2. 转换为全局坐标 → 3. 更新物品位置
	private void PerformSnapToGrid(Node itemEntity, Control itemControl, Vector2I gridPos)
	{
		// 1. 计算网格位置的局部像素坐标
		Vector2 localPos = BackpackGridUIComp.GridToLocalPosition(gridPos);

		// 2. 转换为全局坐标
		Vector2 globalPos = BackpackGridUIComp.GlobalPosition + localPos;

		// 3. 更新物品位置
		itemControl.GlobalPosition = globalPos;

		GD.Print($"物品 {itemEntity.Name} 吸附到网格 {gridPos}，全局位置 {globalPos}");
	}

	/// 执行回弹到原位置
	/// 目的：放置失败时恢复物品到拾取前的位置
	/// 算法：1. 恢复全局位置 → 2. 强制放回逻辑网格 → 3. 记录日志
	private void PerformBounceBack(Node itemEntity, ItemDragState dragState)
	{
		// 1. 恢复原始全局位置
		dragState.ItemControl.GlobalPosition = dragState.OriginalGlobalPos;

		// 2. 强制放回逻辑网格的原位置
		// 【关键】这里不检查返回值，因为原位置一定是合法的（拾取前就在那里）
		var itemData = GetItemData(dragState.ShapeComponent);
		BackpackGridComp.TryPlaceItem(
			itemData,
			dragState.ShapeComponent.CurrentLocalCells,
			dragState.OriginalGridPos
		);

		GD.Print($"物品 {itemEntity.Name} 回弹到原位置 {dragState.OriginalGlobalPos}，网格坐标 {dragState.OriginalGridPos}");
	}

	#endregion

	#region Helper Methods

	public async void SpawnLootItem(ItemData itemData)
	{
		var lootArea = GetNodeOrNull<Control>(LootSpawnAreaPath);
		if (lootArea == null) return;

		var scene = GD.Load<PackedScene>("res://A1TesseractBackpack/TSItem.tscn");
		if (scene == null) return;

		var itemInstance = scene.Instantiate<Control>();
		lootArea.AddChild(itemInstance);
		
		// Spawn somewhere near top-center of the screen
		itemInstance.GlobalPosition = new Vector2(GetViewport().GetVisibleRect().Size.X / 2f, 100);

		RegisterItem(itemInstance);

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		var itemPhysics = itemInstance.GetNodeOrNull<ItemPhysicsComponent>("%PhysicsProxy");
		if (itemPhysics != null)
		{
			itemPhysics.EnablePhysics();
		}
	}

	/// 检查当前是否有物品正在被拖拽
	public bool IsItemBeingDragged() => _currentDrag != null;

	// 获取跟随鼠标组件 (带有容错兜底)
	private FollowMouseUIComponent GetFollowComp(Node itemEntity) =>
		itemEntity.GetNodeOrNull<FollowMouseUIComponent>("%FollowMouseUIComponent")
		?? itemEntity.FindChild("FollowMouseUIComponent", true, false) as FollowMouseUIComponent;

	// 获取补间动画组件 (带有容错兜底)
	private UITweenInteractComponent GetTweenComp(Node itemEntity) =>
		itemEntity.GetNodeOrNull<UITweenInteractComponent>("%UITweenInteractComponent")
		?? itemEntity.FindChild("UITweenInteractComponent", true, false) as UITweenInteractComponent;

	// 安全获取或构造 ItemData
	private ItemData GetItemData(GridShapeComponent shape) =>
		new ItemData(shape.Data?.ItemID ?? "unknown");

	// 获取单元格组控制器
	private ItemCellGroupController GetItemGroupCtrl(Node itemEntity) =>
		itemEntity.GetNodeOrNull<ItemCellGroupController>("%ItemCellGroupController")
		?? itemEntity.FindChild("ItemCellGroupController", true, false) as ItemCellGroupController;

	// 计算当前被抓取单元格相对于物品逻辑原点 (0,0) 的逻辑偏移
	private Vector2I GetGrabbedCellLogicalOffset(ItemDragState dragState, GridShapeComponent shapeComponent)
	{
		if (dragState == null || shapeComponent.CurrentLocalCells == null)
			return Vector2I.Zero;

		int index = dragState.GrabbedCellIndex;
		if (index >= 0 && index < shapeComponent.CurrentLocalCells.Length)
		{
			return shapeComponent.CurrentLocalCells[index];
		}

		return Vector2I.Zero;
	}

	#endregion

	#region Inner Classes

	/// 物品拖拽状态数据
	public class ItemDragState
	{
		/// 原始全局位置（用于回弹）
		public Vector2 OriginalGlobalPos { get; set; }

		/// 原始网格坐标（用于回弹时放回逻辑网格）
		public Vector2I OriginalGridPos { get; set; }

		/// 形状组件引用（用于获取当前形状）
		public GridShapeComponent ShapeComponent { get; set; }

		/// 物品 Control 节点引用（用于设置位置）
		public Control ItemControl { get; set; }

		/// 抓取时的单元格索引
		public int GrabbedCellIndex { get; set; }
	}

	#endregion
}
