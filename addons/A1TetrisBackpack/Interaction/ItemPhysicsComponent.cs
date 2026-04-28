using Godot;

// 物品物理代理组件 — 挂载在 RigidBody2D 节点上
// 职责：在 UI Control 和物理世界之间桥接
// 原理：RigidBody2D 下动态生成多个 CollisionShape2D（每格一个），
//       Godot 自动焊接成复合刚体，配合低摩擦 PhysicsMaterial 实现自然滑落
[GlobalClass]
public partial class ItemPhysicsComponent : RigidBody2D
{
	#region Private Fields

	// 父级 Control 节点（物品 UI 根节点）
	private Control _parentControl;

	// 形状组件引用
	private GridShapeComponent _shapeComp;

	// 物理激活时是否正在同步位置（防止反馈循环）
	private bool _isSyncingPosition;

	#endregion

	#region Godot Lifecycle

	public override void _Ready()
	{
		_parentControl = GetParent<Control>();
		if (_parentControl == null)
		{
			GD.PushError($"[{Name}] 父节点不是 Control，无法桥接物理");
			return;
		}

		_shapeComp = _parentControl.GetNodeOrNull<GridShapeComponent>("%GridShapeComponent");
		if (_shapeComp == null)
		{
			GD.PushError($"[{Name}] 找不到 GridShapeComponent");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// 仅在物理激活（非 Freeze）时，将 RigidBody2D 位置同步回 Control
		if (!Freeze && _parentControl != null)
		{
			_isSyncingPosition = true;
			_parentControl.GlobalPosition = GlobalPosition;
			_isSyncingPosition = false;
		}
	}

	#endregion

	#region Public API

	// 启用物理 — 让物品进入自由落体/滑动状态
	public void EnablePhysics()
	{
		if (_parentControl == null || _shapeComp == null) return;

		// 1. 清除旧碰撞体
		ClearCollisionShapes();

		// 2. 根据当前网格形状，为每个格子生成一个 CollisionShape2D
		BuildCompoundCollisionShapes();

		// 3. 将 RigidBody2D 位置同步到 Control 当前位置
		GlobalPosition = _parentControl.GlobalPosition;

		// 4. 配置物理材质 — 低摩擦 + 微弹性 = 自然滑落手感
		var mat = new PhysicsMaterial();
		mat.Friction = 0.15f;  // 低摩擦，让物品容易滑落
		mat.Bounce = 0.05f;    // 极微弹性，落地不会弹跳但有真实触感
		mat.Rough = false;     // false = 取两者最低摩擦，保证滑溜
		PhysicsMaterialOverride = mat;

		// 5. 解锁旋转 — 让物品在不平衡时自然倾斜滑落
		LockRotation = false;

		// 6. 设置阻尼 — 防止无限滑动，模拟空气阻力
		LinearDamp = 0.5f;   // 轻微线速度阻尼
		AngularDamp = 1.0f;  // 适度角速度阻尼，防止疯狂旋转

		// 7. 启用连续碰撞检测 — 防止高速穿透
		ContinuousCd = CcdMode.CastShape;

		// 8. 解冻 — 让物理引擎接管
		Freeze = false;

		GD.Print($"[{Name}] 物理已启用 — 复合碰撞体: {_shapeComp.CurrentLocalCells.Length} 块");
	}

	// 禁用物理 — 物品回到纯 UI 控制
	public void DisablePhysics()
	{
		// 1. 冻结刚体
		Freeze = true;

		// 2. 清零速度，防止残留动量
		LinearVelocity = Vector2.Zero;
		AngularVelocity = 0;

		// 3. 清除碰撞体（下次启用时根据最新形状重建）
		ClearCollisionShapes();

		// 4. 重置旋转（UI 不需要物理旋转角度）
		Rotation = 0;

		GD.Print($"[{Name}] 物理已禁用");
	}

	#endregion

	#region Private Helpers

	// 为每个网格格子生成一个独立的 CollisionShape2D
	// Godot 物理引擎会自动将同一 RigidBody2D 下的所有碰撞体焊接成复合刚体
	private void BuildCompoundCollisionShapes()
	{
		if (_shapeComp?.CurrentLocalCells == null) return;

		var cellSize = _shapeComp.CellSize;

		foreach (var cell in _shapeComp.CurrentLocalCells)
		{
			var collisionShape = new CollisionShape2D();

			// 每个格子用一个刚好贴合的矩形
			var rect = new RectangleShape2D();
			rect.Size = cellSize;
			collisionShape.Shape = rect;

			// 定位到格子中心（相对于 RigidBody2D 原点即 Control 左上角）
			collisionShape.Position = new Vector2(
				cell.X * cellSize.X + cellSize.X * 0.5f,
				cell.Y * cellSize.Y + cellSize.Y * 0.5f
			);

			AddChild(collisionShape);
		}
	}

	// 清除所有子级碰撞体
	private void ClearCollisionShapes()
	{
		foreach (var child in GetChildren())
		{
			if (child is CollisionShape2D shape)
			{
				RemoveChild(shape);
				shape.QueueFree();
			}
		}
	}

	#endregion
}
