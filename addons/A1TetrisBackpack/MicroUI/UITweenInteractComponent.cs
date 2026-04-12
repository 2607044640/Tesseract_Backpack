using Godot;

/// <summary>
/// UI Tween 交互组件 - 响应式微交互动效
/// 
/// 职责：
/// - 提供悬停、按下的缩放动画反馈
/// - 使用 Tween 实现平滑过渡（支持打断）
/// - 基于"逻辑与视觉分离"原则设计
/// 
/// 核心设计原则：逻辑与视觉分离
/// 
/// **为什么要分离 InteractionArea 和 VisualTarget？**
/// 
/// 问题场景：
/// 如果直接对物品根节点（InteractionArea）应用缩放动画：
/// 1. Scale 改变会影响 GlobalPosition 的计算（缩放导致坐标偏移）
/// 2. Size 也会随之改变，破坏网格吸附的坐标系统
/// 3. BackpackInteractionController 依赖精确的 GlobalPosition 进行坐标转换
/// 4. 结果：物品拖拽时位置计算错误，无法正确吸附到网格
/// 
/// 解决方案：
/// - InteractionArea（外层）：保持 Scale = (1, 1)，负责接收鼠标事件和坐标计算
/// - VisualTarget（内层）：执行缩放动画，提供视觉反馈，不影响外层坐标系
/// 
/// 节点结构示例：
/// ```
/// ItemEntity (Control) ← InteractionArea [Scale 永远是 1,1]
/// ├── UITweenInteractComponent
/// └── VisualContainer (Control) ← VisualTarget [Scale 可以变化]
///     └── TextureRect (物品图标)
/// ```
/// 
/// 优势：
/// - 外层坐标系稳定：GlobalPosition 和 Size 不受动画影响
/// - 内层视觉反馈：用户看到缩放效果
/// - 解耦设计：动画系统与交互系统独立
/// 
/// 使用示例：
/// ```csharp
/// // 在编辑器中：
/// // 1. 将 UITweenInteractComponent 添加为物品的子节点
/// // 2. 设置 InteractionArea 为物品根节点
/// // 3. 设置 VisualTarget 为内部的视觉容器
/// // 4. 调整动画参数（HoverScale, PressScale, TweenDuration）
/// 
/// // 无需任何代码！组件会自动工作。
/// ```
/// </summary>
[GlobalClass]
public partial class UITweenInteractComponent : Node
{
	#region Export Properties
	
	/// <summary>
	/// 交互区域（接收鼠标事件的逻辑节点）
	/// 
	/// 特性：
	/// - 保持 Scale = (1, 1)，不执行缩放动画
	/// - 负责接收鼠标事件（mouse_entered, mouse_exited, gui_input）
	/// - 用于坐标计算和碰撞检测
	/// 
	/// 默认值：
	/// 如果未设置，自动使用父节点
	/// </summary>
	[Export] public Control InteractionArea { get; set; }
	
	/// <summary>
	/// 视觉目标（执行缩放动画的视觉节点）
	/// 
	/// 特性：
	/// - 执行缩放动画，提供视觉反馈
	/// - 不影响外层的坐标系统
	/// - 通常是物品图标的容器节点
	/// 
	/// 必需：
	/// 必须手动设置，否则组件无法工作
	/// </summary>
	[Export] public Control VisualTarget { get; set; }
	
	/// <summary>
	/// 悬停时的目标缩放
	/// </summary>
	[Export] public Vector2 HoverScale { get; set; } = new Vector2(1.05f, 1.05f);
	
	/// <summary>
	/// 按下时的目标缩放
	/// </summary>
	[Export] public Vector2 PressScale { get; set; } = new Vector2(0.95f, 0.95f);
	
	/// <summary>
	/// 动画过渡时间（秒）
	/// </summary>
	[Export] public float TweenDuration { get; set; } = 0.15f;
	
	#endregion
	
	#region Private Fields
	
	/// <summary>
	/// 当前正在执行的 Tween 动画
	/// 用于支持平滑打断
	/// </summary>
	private Tween _currentTween;
	
	/// <summary>
	/// 是否正在按下
	/// </summary>
	private bool _isPressed = false;
	
	#endregion
	
	#region Godot Lifecycle
	
	public override void _Ready()
	{
		// 默认 InteractionArea 为父节点
		if (InteractionArea == null)
		{
			InteractionArea = GetParent<Control>();
			if (InteractionArea == null)
			{
				GD.PushError("UITweenInteractComponent: 无法找到 InteractionArea（父节点不是 Control）");
				return;
			}
		}
		
		// 验证 VisualTarget
		if (VisualTarget == null)
		{
			GD.PushError("UITweenInteractComponent: VisualTarget 未设置！组件将无法工作。");
			return;
		}
		
		// 设置 PivotOffset 为中心点（确保从中心缩放）
		UpdatePivotOffset();
		
		// 订阅鼠标事件
		InteractionArea.MouseEntered += OnMouseEntered;
		InteractionArea.MouseExited += OnMouseExited;
		InteractionArea.GuiInput += OnGuiInput;
		
		GD.Print($"UITweenInteractComponent: 已初始化（InteractionArea={InteractionArea.Name}, VisualTarget={VisualTarget.Name}）");
	}
	
	public override void _ExitTree()
	{
		// 取消订阅事件
		if (InteractionArea != null)
		{
			InteractionArea.MouseEntered -= OnMouseEntered;
			InteractionArea.MouseExited -= OnMouseExited;
			InteractionArea.GuiInput -= OnGuiInput;
		}
		
		// 清理 Tween
		_currentTween?.Kill();
	}
	
	#endregion
	
	#region Event Handlers
	
	private void OnMouseEntered()
	{
		if (!_isPressed)
		{
			AnimateToScale(HoverScale);
		}
	}
	
	private void OnMouseExited()
	{
		_isPressed = false;
		AnimateToScale(Vector2.One);
	}
	
	private void OnGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (mouseEvent.Pressed)
			{
				// 左键按下
				_isPressed = true;
				AnimateToScale(PressScale);
			}
			else
			{
				// 左键松开
				_isPressed = false;
				
				// 如果鼠标仍在区域内，回到悬停状态
				if (InteractionArea.GetGlobalRect().HasPoint(InteractionArea.GetGlobalMousePosition()))
				{
					AnimateToScale(HoverScale);
				}
				else
				{
					AnimateToScale(Vector2.One);
				}
			}
		}
	}
	
	#endregion
	
	#region Animation Logic
	
	/// <summary>
	/// 执行缩放动画
	/// 目的：平滑过渡到目标缩放值
	/// 示例：AnimateToScale((1.05, 1.05)) → VisualTarget 在 0.15 秒内缩放到 105%
	/// 算法：1. 杀死当前动画 → 2. 创建新 Tween → 3. 配置缓动 → 4. 执行缩放
	/// </summary>
	private void AnimateToScale(Vector2 targetScale)
	{
		if (VisualTarget == null)
			return;
		
		// 1. 杀死当前动画（支持平滑打断）
		// 【关键】如果不 Kill，新动画会与旧动画冲突
		_currentTween?.Kill();
		
		// 2. 创建新的 Tween
		_currentTween = GetTree().CreateTween();
		
		// 3. 配置缓动曲线
		// EaseType.Out：快速开始，缓慢结束（自然的减速效果）
		// TransitionType.Sine：正弦曲线（平滑的过渡）
		_currentTween.SetEase(Tween.EaseType.Out);
		_currentTween.SetTrans(Tween.TransitionType.Sine);
		
		// 4. 执行缩放动画
		_currentTween.TweenProperty(VisualTarget, "scale", targetScale, TweenDuration);
	}
	
	/// <summary>
	/// 更新 PivotOffset 为中心点
	/// 确保缩放从中心进行，而不是左上角
	/// </summary>
	private void UpdatePivotOffset()
	{
		if (VisualTarget == null)
			return;
		
		// 设置为尺寸的一半（中心点）
		VisualTarget.PivotOffset = VisualTarget.Size / 2;
	}
	
	#endregion
	
	#region Public Methods
	
	/// <summary>
	/// 手动触发悬停动画
	/// </summary>
	public void TriggerHover()
	{
		AnimateToScale(HoverScale);
	}
	
	/// <summary>
	/// 手动触发按下动画
	/// </summary>
	public void TriggerPress()
	{
		AnimateToScale(PressScale);
	}
	
	/// <summary>
	/// 手动重置到正常状态
	/// </summary>
	public void ResetToNormal()
	{
		_isPressed = false;
		AnimateToScale(Vector2.One);
	}
	
	/// <summary>
	/// 立即设置缩放（无动画）
	/// </summary>
	public void SetScaleImmediate(Vector2 scale)
	{
		if (VisualTarget == null)
			return;
		
		_currentTween?.Kill();
		VisualTarget.Scale = scale;
	}
	
	/// <summary>
	/// 刷新 PivotOffset（当 VisualTarget 尺寸改变时调用）
	/// </summary>
	public void RefreshPivotOffset()
	{
		UpdatePivotOffset();
	}
	
	#endregion
}
