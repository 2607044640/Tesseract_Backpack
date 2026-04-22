using Godot;
using R3;
using System.Collections.Generic;

/// 羁绊组件 - 运行时羁绊检测与激活
/// 
/// 职责：
/// - 检测相邻物品是否满足羁绊条件
/// - 维护当前激活的星星列表
/// - 通过 R3 通知 UI 层更新星星显示
/// - 处理物品旋转导致的星星位置变化
/// 
/// 工作流程：
/// 1. 物品放置到背包 → 调用 CheckSynergies()
/// 2. 遍历所有星星位置（StarOffsets）
/// 3. 根据物品当前旋转角度旋转星星偏移量
/// 4. 计算星星在背包中的绝对坐标
/// 5. 查询该位置的物品
/// 6. 检查物品是否提供所需标签
/// 7. 更新 ActiveStars 集合
/// 8. 触发 OnSynergyChangedAsObservable 通知 UI
/// 
/// 旋转处理：
/// 
/// 问题：物品可能被旋转，星星位置也需要跟随旋转
/// 
/// 示例：
/// 初始状态（0°）：
/// ■■
/// 星星在 (2, 0)
/// 
/// 旋转 90°：
/// ■
/// ■
/// 星星应该在 (0, 2)
/// 
/// 旋转公式（顺时针 90°）：
/// (x, y) → (-y, x)
/// 
/// 应用：
/// (2, 0) → (0, 2) ✓
/// 
/// 使用示例：
/// ```csharp
/// // 物品放置后
/// synergyComponent.CheckSynergies(backpackGridComp, itemGridPos);
/// 
/// // 订阅变化
/// synergyComponent.OnSynergyChangedAsObservable
///     .Subscribe(activeStars => UpdateStarVisuals(activeStars))
///     .AddTo(disposables);
/// ```
[GlobalClass]
public partial class SynergyComponent : Node
{
	#region Export Properties
	
	/// 羁绊数据资源引用
	[Export] public SynergyDataResource SynergyData { get; set; }
	
	/// 形状组件引用（用于获取旋转状态）
	[Export] public GridShapeComponent GridShapeComponent { get; set; }
	
	#endregion
	
	#region Public Properties
	
	/// 当前激活的星星集合（局部坐标，已旋转）
	public HashSet<Vector2I> ActiveStars { get; private set; } = new HashSet<Vector2I>();
	
	#endregion
	
	#region R3 Reactive Streams
	
	/// 羁绊变化事件流
	/// 
	/// 发送数据：当前所有激活的星星坐标（局部坐标，已旋转）
	/// 
	/// 用途：
	/// - 通知 UI 层更新星星显示（灰色 → 亮色）
	/// - 通知战斗系统应用羁绊效果
	/// - 通知音效系统播放激活音效
	public Subject<HashSet<Vector2I>> OnSynergyChangedAsObservable { get; private set; }
	
	#endregion
	
	#region Private Fields
	
	/// 当前旋转次数（0 = 0°, 1 = 90°, 2 = 180°, 3 = 270°）
	private int _rotationCount = 0;
	
	/// R3 订阅容器
	private CompositeDisposable _disposables = new CompositeDisposable();
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		// 初始化 R3 Subject
		OnSynergyChangedAsObservable = new Subject<HashSet<Vector2I>>();
		
		// 验证必需引用
		if (SynergyData == null)
		{
			GD.PushWarning("SynergyComponent: SynergyData 未设置");
		}
		
		if (GridShapeComponent == null)
		{
			GD.PushWarning("SynergyComponent: GridShapeComponent 未设置");
		}
		
		// 订阅形状变化事件（用于追踪旋转）
		if (GridShapeComponent != null)
		{
			GridShapeComponent.OnShapeChangedAsObservable
				.Subscribe(_ => OnShapeRotated())
				.AddTo(_disposables);
		}
		
		GD.Print($"SynergyComponent: 初始化完成");
	}
	
	public override void _ExitTree()
	{
		// R3 规则：释放所有订阅和 Subject
		_disposables.Dispose();
		OnSynergyChangedAsObservable?.Dispose();
	}
	
	#endregion
	
	#region Synergy Detection
	
	/// 检测羁绊激活状态
	/// 目的：遍历所有星星位置，检测相邻物品是否满足条件
	/// 示例：香蕉在 (5, 3)，星星在 (1, 0) → 检测 (6, 3) 是否有 "Food" 物品
	/// 算法：1. 清空激活列表 → 2. 遍历星星 → 3. 旋转偏移 → 4. 查询物品 → 5. 检查标签 → 6. 更新状态
	public void CheckSynergies(BackpackGridComponent backpackGridComp, Vector2I currentGridPos)
	{
		// 1. 清空当前激活状态
		ActiveStars.Clear();
		
		// 验证必需数据
		if (SynergyData == null || SynergyData.StarOffsets == null || SynergyData.StarOffsets.Count == 0)
		{
			OnSynergyChangedAsObservable.OnNext(ActiveStars);
			return;
		}
		
		if (backpackGridComp == null)
		{
			GD.PushWarning("SynergyComponent: backpackGridComp 为空");
			OnSynergyChangedAsObservable.OnNext(ActiveStars);
			return;
		}
		
		// 2. 遍历所有星星位置
		foreach (var starOffset in SynergyData.StarOffsets)
		{
			// 3. 根据当前旋转角度旋转星星偏移量
			// 【关键】配置的 StarOffset 是相对于初始状态（0°）的
			// 需要根据物品当前旋转次数应用旋转变换
			Vector2I rotatedOffset = ApplyRotationToOffset(starOffset, _rotationCount);
			
			// 4. 计算星星在背包中的绝对网格坐标
			Vector2I starWorldPos = currentGridPos + rotatedOffset;
			
			// 5. 查询该位置的物品
			ItemData itemAtStar = backpackGridComp.GetItemAt(starWorldPos);
			
			if (itemAtStar == null)
			{
				// 该位置没有物品，星星不激活
				continue;
			}
			
			// 6. 获取该物品的 SynergyComponent
			// 注意：需要通过某种方式找到对应的物品实体节点
			// 这里简化处理：假设 ItemData 包含了 SynergyDataResource 引用
			// 实际项目中可能需要维护一个 ItemData → Node 的映射表
			
			// 简化方案：直接检查 ItemData 是否有 SynergyData
			// 这需要扩展 ItemData 类，或者使用其他方式关联
			
			// 临时方案：假设可以通过某种方式获取 SynergyDataResource
			// 这里先跳过实际查询，留待后续完善
			
			// TODO: 实现物品查询逻辑
			// 可能的方案：
			// 1. 在 BackpackInteractionController 中维护 Dictionary<Vector2I, Node>
			// 2. 在 ItemData 中添加 Node 引用
			// 3. 使用全局管理器查询
			
			// 暂时使用占位逻辑
			bool hasRequiredTag = CheckItemHasTag(itemAtStar, SynergyData.RequiredTag);
			
			if (hasRequiredTag)
			{
				// 7. 标签匹配，激活星星
				ActiveStars.Add(rotatedOffset);
			}
		}
		
		// 8. 触发变化事件
		OnSynergyChangedAsObservable.OnNext(ActiveStars);
		
		GD.Print($"SynergyComponent: 检测完成，激活 {ActiveStars.Count}/{SynergyData.StarOffsets.Count} 颗星星");
	}
	
	/// 检查物品是否有指定标签（占位实现）
	/// 
	/// TODO: 实现实际的标签查询逻辑
	/// 需要从 ItemData 关联到实际的物品节点，再获取其 SynergyComponent
	private bool CheckItemHasTag(ItemData item, string requiredTag)
	{
		// 占位实现：总是返回 false
		// 实际项目中需要：
		// 1. 从 ItemData 获取对应的物品节点
		// 2. 获取该节点的 SynergyComponent
		// 3. 检查 SynergyData.ProvidedTags 是否包含 requiredTag
		
		GD.PushWarning("SynergyComponent.CheckItemHasTag: 占位实现，需要完善");
		return false;
	}
	
	#endregion
	
	#region Rotation Logic
	
	/// 形状旋转时的回调
	private void OnShapeRotated()
	{
		// 每次旋转，旋转计数 +1（模 4）
		_rotationCount = (_rotationCount + 1) % 4;
		
		GD.Print($"SynergyComponent: 物品已旋转，当前旋转次数 = {_rotationCount} ({_rotationCount * 90}°)");
	}
	
	/// 应用旋转变换到偏移量
	/// 目的：将配置的星星偏移量根据物品当前旋转角度进行变换
	/// 示例：偏移 (2, 0)，旋转 1 次（90°）→ (0, 2)
	/// 算法：循环应用顺时针 90° 旋转矩阵 (x, y) → (-y, x)
	private Vector2I ApplyRotationToOffset(Vector2I offset, int rotationCount)
	{
		Vector2I result = offset;
		
		// 应用旋转矩阵 rotationCount 次
		// 顺时针 90° 旋转公式：(x, y) → (-y, x)
		for (int i = 0; i < rotationCount; i++)
		{
			int newX = -result.Y;
			int newY = result.X;
			result = new Vector2I(newX, newY);
		}
		
		return result;
	}
	
	/// 重置旋转计数（用于物品重新放置时）
	public void ResetRotation()
	{
		_rotationCount = 0;
	}
	
	#endregion
	
	#region Helper Methods
	
	/// 获取激活的星星数量
	public int GetActiveStarCount()
	{
		return ActiveStars.Count;
	}
	
	/// 检查指定星星是否激活
	public bool IsStarActive(Vector2I starOffset)
	{
		return ActiveStars.Contains(starOffset);
	}
	
	/// 获取羁绊效果描述
	public string GetSynergyEffectDescription()
	{
		if (SynergyData == null)
			return "";
		
		int activeCount = ActiveStars.Count;
		int totalCount = SynergyData.GetStarCount();
		
		return $"{SynergyData.SynergyEffect} ({activeCount}/{totalCount} 星星激活)";
	}
	
	#endregion
}
