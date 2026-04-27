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
		Vector2 originalGlobalPos = itemControl.GlobalPosition;
		Vector2I originalGridPos = BackpackGridUIComp.GlobalToGridPosition(originalGlobalPos);

		// 从逻辑网格移除，防止"自我占用"
		if (BackpackGridUIComp.IsValidGridPosition(originalGridPos))
		{
			BackpackGridComp.RemoveItem(
				new ItemData(shapeComponent.Data?.ItemID ?? "unknown"),
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
		var followComp = itemEntity.GetNodeOrNull<FollowMouseUIComponent>("%FollowMouseUIComponent")
			?? itemEntity.FindChild("FollowMouseUIComponent", true, false) as FollowMouseUIComponent;

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

		// ── 预览流：双管齐下，Merge 位置流 + 形状流 ──────────────────────────────
		// posStream：鼠标跨越网格边界时触发（DistinctUntilChanged 过滤亚格子抖动）
		var posStream = Observable.EveryUpdate()
			.Select(_ => BackpackGridUIComp.GetGlobalMousePosition())
			.Select(mousePos => {
				// 1. 获取鼠标当前所在的网格单元格
				Vector2I mouseGridPos = BackpackGridUIComp.GlobalToGridPosition(mousePos);
				
				// 2. 获取当前被抓取单元格的逻辑坐标偏移
				Vector2I grabbedCellLogical = Vector2I.Zero;
				if (_currentDrag != null && shapeComponent.CurrentLocalCells != null 
					&& _currentDrag.GrabbedCellIndex >= 0 
					&& _currentDrag.GrabbedCellIndex < shapeComponent.CurrentLocalCells.Length)
				{
					grabbedCellLogical = shapeComponent.CurrentLocalCells[_currentDrag.GrabbedCellIndex];
				}

				// 3. 纯数学计算：左上角坐标 = 鼠标网格坐标 - 抓取点的逻辑偏移
				Vector2I targetTopLeftGridPos = mouseGridPos - grabbedCellLogical;

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
				
				Vector2I grabbedCellLogical = Vector2I.Zero;
				if (_currentDrag != null && shapeComponent.CurrentLocalCells != null 
					&& _currentDrag.GrabbedCellIndex >= 0 
					&& _currentDrag.GrabbedCellIndex < shapeComponent.CurrentLocalCells.Length)
				{
					grabbedCellLogical = shapeComponent.CurrentLocalCells[_currentDrag.GrabbedCellIndex];
				}

				Vector2I targetTopLeftGridPos = mouseGridPos - grabbedCellLogical;

				return (
					inBounds: BackpackGridUIComp.GetGlobalRect().HasPoint(mp),
					gridPos:  targetTopLeftGridPos
				);
			});

		// 合并：任一流发射 → 执行同一段预览逻辑
		Observable.Merge(posStream, shapeStream)
			.TakeUntil(draggable.OnDragEndedAsObservable)
			.Subscribe(state =>
			{
				if (!state.inBounds)
					BackpackGridUIComp.ClearPreview();
				else
					BackpackGridUIComp.ShowPreview(BackpackGridComp.EvaluatePlacementPreview(
						shapeComponent.CurrentLocalCells, state.gridPos));
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

		if (_currentDrag == null)
		{
			GD.PushWarning($"物品 {itemEntity.Name} 没有拖拽状态记录");
			return;
		}

		var dragState = _currentDrag;
		_currentDrag = null;

		// 使用 ViewGrid.GetGlobalMousePosition() 确保在 Camera2D/CanvasLayer 下坐标准确
		Vector2 mousePos = BackpackGridUIComp.GetGlobalMousePosition();
		bool isInBackpack = BackpackGridUIComp.GetGlobalRect().HasPoint(mousePos);

		if (!isInBackpack)
		{
			PerformBounceBack(itemEntity, dragState);
			return;
		}

		Vector2I mouseGridPos = BackpackGridUIComp.GlobalToGridPosition(mousePos);
		
		// 纯数学逻辑计算左上角网格坐标
		Vector2I grabbedCellLogical = Vector2I.Zero;
		if (dragState != null && shapeComponent.CurrentLocalCells != null 
			&& dragState.GrabbedCellIndex >= 0 
			&& dragState.GrabbedCellIndex < shapeComponent.CurrentLocalCells.Length)
		{
			grabbedCellLogical = shapeComponent.CurrentLocalCells[dragState.GrabbedCellIndex];
		}

		Vector2I targetGridPos = mouseGridPos - grabbedCellLogical;
		var itemData = new ItemData(shapeComponent.Data?.ItemID ?? "unknown");
		bool placementSuccess = BackpackGridComp.TryPlaceItem(
			itemData,
			shapeComponent.CurrentLocalCells,
			targetGridPos
		);

		if (placementSuccess)
			PerformSnapToGrid(itemEntity, itemControl, targetGridPos);
		else
			PerformBounceBack(itemEntity, dragState);
	}
    /// 处理物品旋转
    private void HandleItemRotated(Node itemEntity, GridShapeComponent shapeComponent)
    {
        if (_currentDrag == null) return;

        // 1. 获取组件 (防御性编程非常棒，进一步简写)
        var followComp = itemEntity.GetNodeOrNull<FollowMouseUIComponent>("%FollowMouseUIComponent")
                         ?? itemEntity.FindChild("FollowMouseUIComponent", true, false) as FollowMouseUIComponent;
        var interactionArea = itemEntity.GetNodeOrNull<Control>("%InteractionArea");

        if (followComp == null || interactionArea == null)
        {
            GD.PushWarning($"[Rotation] 缺少必要组件！");
            return;
        }

        // 2. 执行底层逻辑旋转 (矩阵变换 + 归一化)
        shapeComponent.Rotate90();

        // 3. 计算当前鼠标抓着的那个格子，在【新矩阵】中的局部中心坐标
        Vector2I newGrabbedCell = shapeComponent.CurrentLocalCells[_currentDrag.GrabbedCellIndex];
        float cellSize = BackpackGridUIComp.CellSize.X; // 假定长宽一致
        Vector2 newGrabbedCellLocalPos = new Vector2(
            (newGrabbedCell.X * cellSize) + (cellSize / 2f),
            (newGrabbedCell.Y * cellSize) + (cellSize / 2f)
        );

        // 4. 同步父节点的抓取偏移 (这一步会让父节点在物理空间瞬间跳变，对齐新网格锚点)
        followComp.GrabOffset = -newGrabbedCellLocalPos;

        // 5. 视觉补偿动画 (【核心魔法】：利用 PivotOffset 彻底消灭 Position 补间)
        // 把旋转轴心死死钉在当前鼠标抓取的位置上
        interactionArea.PivotOffset = newGrabbedCellLocalPos;

        // 父节点跳变后，我们让子节点绕着鼠标逆向旋转 90 度 (-Pi/2)
        // 此时画面绝对完美重合在上一帧，不差一个像素！完全不需要改 Position！
        interactionArea.Rotation = -Mathf.Pi / 2f;

        // 6. 仅需平滑还原角度到 0 即可 (极致丝滑的指尖自转)
        var tween = interactionArea.GetTree().CreateTween();
        tween.TweenProperty(interactionArea, "rotation", 0f, 0.15f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        GD.Print($"[{itemEntity.Name}] 丝滑旋转触发：绕点 {newGrabbedCellLocalPos} 旋转");
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
		var itemData = new ItemData(dragState.ShapeComponent.Data?.ItemID ?? "unknown");
		BackpackGridComp.TryPlaceItem(
			itemData,
			dragState.ShapeComponent.CurrentLocalCells,
			dragState.OriginalGridPos
		);

		GD.Print($"物品 {itemEntity.Name} 回弹到原位置 {dragState.OriginalGlobalPos}，网格坐标 {dragState.OriginalGridPos}");
	}

	#endregion

	#region Helper Methods

	/// 检查当前是否有物品正在被拖拽
	public bool IsItemBeingDragged() => _currentDrag != null;

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
