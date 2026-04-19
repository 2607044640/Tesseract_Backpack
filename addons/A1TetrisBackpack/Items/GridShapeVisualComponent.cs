using Godot;
using R3;
using System.Collections.Generic;

/// <summary>
/// 网格形状视觉组件 - 离散方块生成器 + 事件聚合器
/// 目的：生成独立的 1x1 ColorRect 方块，并将所有方块的 GuiInput 事件聚合到单一 R3 流
/// 示例：L 形 [(0,0), (0,1), (1,1)] -> 生成 3 个独立 ColorRect，每个都能独立接收鼠标事件
/// 算法：1. 生成方块 -> 2. 绑定每个方块的 GuiInput 到 R3 Subject -> 3. 暴露统一的 Observable 流
/// 
/// 架构模式：Event Aggregator - 将多个离散事件源汇聚成单一流
/// 关键突破：完全绕过 AABB 问题，每个方块独立响应，无需 _HasPoint 重写
/// </summary>
[GlobalClass]
public partial class GridShapeVisualComponent : Node
{
	#region Export Properties
	
	/// <summary>
	/// 逻辑数据源组件路径（提供局部坐标数组和形状变化事件）
	/// </summary>
	[Export] public NodePath GridShapeComponentPath { get; set; } = "%GridShapeComponent";
	
	/// <summary>
	/// 交互区域容器路径（用于放置生成的方块）
	/// </summary>
	[Export] public NodePath InteractionAreaPath { get; set; } = "%InteractionArea";
	
	/// <summary>
	/// GridShapeComponent 引用
	/// </summary>
	public GridShapeComponent GridShapeComponent { get; private set; }
	
	/// <summary>
	/// InteractionArea 引用
	/// </summary>
	public Control InteractionArea { get; private set; }
	
	/// <summary>
	/// 单个格子的像素尺寸（默认 64x64）
	/// </summary>
	[Export] public float CellSize { get; set; } = 64f;
	
	/// <summary>
	/// 正常状态颜色（半透明白色）
	/// </summary>
	[Export] public Color NormalColor { get; set; } = new Color(1f, 1f, 1f, 0.2f);
	
	/// <summary>
	/// 有效放置状态颜色（绿色光）
	/// </summary>
	[Export] public Color ValidColor { get; set; } = new Color(0.2f, 0.8f, 0.2f, 0.6f);
	
	/// <summary>
	/// 无效放置状态颜色（红色光）
	/// </summary>
	[Export] public Color InvalidColor { get; set; } = new Color(0.8f, 0.2f, 0.2f, 0.6f);
	
	#endregion
	
	#region Private Fields
	
	/// <summary>
	/// R3 Subject - 聚合所有方块的 GuiInput 事件
	/// </summary>
	private readonly Subject<InputEvent> _onBlockInputSubject = new();
	
	/// <summary>
	/// 跟踪所有生成的视觉方块（用于颜色反馈和清理）
	/// </summary>
	private readonly List<ColorRect> _visualBlocks = new();
	
	#endregion
	
	#region Public Interface
	
	/// <summary>
	/// 暴露聚合后的输入事件流（供 DraggableItemComponent 订阅）
	/// </summary>
	public Observable<InputEvent> OnBlockInputAsObservable => _onBlockInputSubject;
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		GD.Print($"[{Name}] GridShapeVisualComponent._Ready() 开始");
		
		// 解析 NodePath 引用
		GridShapeComponent = GetNodeOrNull<GridShapeComponent>(GridShapeComponentPath);
		InteractionArea = GetNodeOrNull<Control>(InteractionAreaPath);
		
		// Assert required references are valid
		if (GridShapeComponent == null)
		{
			GD.PushError($"[{Name}] GridShapeVisualComponent: GridShapeComponent not found at path: {GridShapeComponentPath}");
			return;
		}
		
		if (InteractionArea == null)
		{
			GD.PushError($"[{Name}] GridShapeVisualComponent: InteractionArea not found at path: {InteractionAreaPath}");
			return;
		}
		
		GD.Print($"[{Name}] GridShapeComponent 引用有效: {GridShapeComponent.Name}");
		GD.Print($"[{Name}] InteractionArea 引用有效: {InteractionArea.Name}");
		
		// 【关键架构设计】：父容器忽略鼠标，让子块独立接收事件
		// CRITICAL: Parent container must ignore mouse to allow child blocks to receive events
		InteractionArea.MouseFilter = Control.MouseFilterEnum.Ignore;
		GD.Print($"[{Name}] InteractionArea.MouseFilter 已设置为 Ignore");
		
		// 订阅逻辑组件的形状变化事件（旋转时自动重建视觉方块）
		GridShapeComponent.OnShapeChangedAsObservable
			.Subscribe(_ => 
			{
				GD.Print($"[{Name}] 收到形状变化事件，重建视觉方块");
				RebuildVisualBlocks();
			})
			.AddTo(this);
		
		GD.Print($"[{Name}] 已订阅 GridShapeComponent.OnShapeChangedAsObservable");
		
		// 【架构修正】不在 _Ready() 中立即构建方块
		// 原因：数据通过 IItemDataProvider 接口异步注入，此时 CurrentLocalCells 可能为 null
		// 解决方案：仅通过 OnShapeChangedAsObservable 事件触发构建（数据注入后会自动触发）
		// 如果数据已经存在（非接口模式），手动触发一次构建
		if (GridShapeComponent.CurrentLocalCells != null)
		{
			GD.Print($"[{Name}] 数据已存在，立即构建视觉方块");
			RebuildVisualBlocks();
		}
		else
		{
			GD.Print($"[{Name}] 数据尚未注入，等待 OnShapeChangedAsObservable 事件");
		}
		
		GD.Print($"[{Name}] GridShapeVisualComponent._Ready() 完成");
	}
	
	public override void _ExitTree()
	{
		// 清理 R3 Subject
		_onBlockInputSubject?.Dispose();
		
		// 取消订阅所有方块的 GuiInput 事件（防止内存泄漏）
		foreach (var block in _visualBlocks)
		{
			if (block != null && !block.IsQueuedForDeletion())
			{
				// Note: GuiInput 事件会在 QueueFree 时自动清理
			}
		}
	}
	
	#endregion
	
	#region Visual Block Generation
	
	/// <summary>
	/// 重建视觉方块 - 生成独立的 1x1 ColorRect 并绑定 GuiInput 事件
	/// 目的：为每个局部坐标生成独立的可交互方块，并将事件聚合到 R3 流
	/// 示例：L 形 [(0,0), (0,1), (1,1)] -> 3 个 ColorRect，每个都能触发 OnBlockInputAsObservable
	/// 算法：1. 清理旧方块 -> 2. 遍历局部坐标 -> 3. 创建 ColorRect 并绑定 GuiInput -> 4. 添加到容器
	/// </summary>
	private void RebuildVisualBlocks()
	{
		GD.Print($"[{Name}] RebuildVisualBlocks() 开始");
		
		// 1. 清理旧块（从 InteractionArea 移除并释放）
		GD.Print($"[{Name}] 清理旧方块，当前数量: {_visualBlocks.Count}");
		foreach (var block in _visualBlocks)
		{
			block.QueueFree();
		}
		_visualBlocks.Clear();
		
		// 2. 验证数据源有效性
		if (GridShapeComponent.CurrentLocalCells == null)
		{
			GD.PushWarning($"[{Name}] GridShapeVisualComponent: CurrentLocalCells is null, skipping visual block generation.");
			return;
		}
		
		GD.Print($"[{Name}] CurrentLocalCells 有效，格子数量: {GridShapeComponent.CurrentLocalCells.Length}");
		
		// 3. 遍历 GridShapeComponent.CurrentLocalCells 生成视觉方块
		int blockIndex = 0;
		foreach (Vector2I cellPos in GridShapeComponent.CurrentLocalCells)
		{
			GD.Print($"[{Name}] 生成方块 {blockIndex}: cellPos=({cellPos.X}, {cellPos.Y})");
			
			var visualBlock = new ColorRect
			{
				// Position formula: cellPos * CellSize
				Size = new Vector2(CellSize, CellSize),
				Position = new Vector2(cellPos.X * CellSize, cellPos.Y * CellSize),
				Color = NormalColor,
				// 【关键】：每个方块独立接收鼠标事件
				// CRITICAL: Each block independently receives mouse events
				MouseFilter = Control.MouseFilterEnum.Pass,
				Name = $"VisualBlock_{blockIndex}"
			};
			
			GD.Print($"[{Name}] 方块 {blockIndex} 属性: Size={visualBlock.Size}, Position={visualBlock.Position}, Color={visualBlock.Color}, MouseFilter={visualBlock.MouseFilter}");
			
			// 【事件聚合】：将此方块的 GuiInput 事件推送到 R3 Subject
			// EVENT AGGREGATION: Push this block's GuiInput to R3 Subject
			visualBlock.GuiInput += (inputEvent) =>
			{
				GD.Print($"[{Name}] 方块 GuiInput 事件触发: {inputEvent}");
				_onBlockInputSubject.OnNext(inputEvent);
			};
			
			// 4. 添加到 InteractionArea 并跟踪
			InteractionArea.AddChild(visualBlock);
			_visualBlocks.Add(visualBlock);
			
			GD.Print($"[{Name}] 方块 {blockIndex} 已添加到 InteractionArea");
			blockIndex++;
		}
		
		GD.Print($"[{Name}] RebuildVisualBlocks() 完成，生成 {_visualBlocks.Count} 个视觉方块");
		GD.Print($"[{Name}] InteractionArea 子节点数量: {InteractionArea.GetChildCount()}");
	}
	
	#endregion
	
	#region Validation Feedback (Public Interface for Drag/Drop Controller)
	
	/// <summary>
	/// 设置验证反馈 - 用于拖拽悬停时的红绿光反馈
	/// 目的：提供视觉反馈，告知玩家当前放置位置是否有效
	/// 示例：拖拽物品到背包上方，有效位置显示绿色，无效位置显示红色
	/// 算法：遍历所有视觉方块，设置颜色为 ValidColor 或 InvalidColor
	/// </summary>
	public void SetValidationFeedback(bool isValid)
	{
		Color targetColor = isValid ? ValidColor : InvalidColor;
		foreach (var block in _visualBlocks)
		{
			block.Color = targetColor;
		}
	}
	
	/// <summary>
	/// 重置反馈 - 恢复正常状态颜色
	/// </summary>
	public void ResetFeedback()
	{
		foreach (var block in _visualBlocks)
		{
			block.Color = NormalColor;
		}
	}
	
	#endregion
}
