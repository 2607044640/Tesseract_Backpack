using Godot;
using R3;

/// 网格单元格UI组件 - 统一的格子视觉表现
/// 目的：提供"透明体+发光边框"的专业视觉效果，支持状态切换和输入事件聚合
/// 示例：Normal状态=半透明白边框，Valid状态=绿色发光边框，Hover状态=高亮边框
/// 算法：1. 使用StyleBoxFlat渲染边框 -> 2. 状态切换更新颜色 -> 3. R3 Subject发布输入事件
[Tool]
[GlobalClass]
public partial class GridCellUI : Panel
{
	#region State Enum

	/// 单元格状态枚举
	public enum CellState
	{
		/// <summary>正常状态（默认半透明白色边框）</summary>
		Normal,
		/// <summary>有效放置状态（绿色发光边框）</summary>
		Valid,
		/// <summary>无效放置状态（红色发光边框）</summary>
		Invalid,
		/// <summary>鼠标悬停状态（高亮边框）</summary>
		Hover
	}

	#endregion

	#region Export Properties

	/// 边框宽度（像素）
	[Export] public float BorderWidth { get; set; } = 2f;

	/// 正常状态颜色（半透明白色）
	[Export] public Color NormalColor_Border { get; set; } = new Color(1f, 1f, 1f, 0.3f);

	/// 正常状态背景颜色（几乎透明）
	[Export] public Color NormalColor_Background { get; set; } = new Color(1f, 1f, 1f, 0.05f);

	/// 有效状态边框颜色（绿色发光）
	[Export] public Color ValidColor_Border { get; set; } = new Color(0.2f, 1f, 0.2f, 0.8f);

	/// 有效状态背景颜色（淡绿色）
	[Export] public Color ValidColor_Background { get; set; } = new Color(0.2f, 0.8f, 0.2f, 0.15f);

	/// 无效状态边框颜色（红色发光）
	[Export] public Color InvalidColor_Border { get; set; } = new Color(1f, 0.2f, 0.2f, 0.8f);

	/// 无效状态背景颜色（淡红色）
	[Export] public Color InvalidColor_Background { get; set; } = new Color(0.8f, 0.2f, 0.2f, 0.15f);

	/// 悬停状态边框颜色（亮白色）
	[Export] public Color HoverColor_Border { get; set; } = new Color(1f, 1f, 1f, 0.6f);

	/// 悬停状态背景颜色（淡白色）
	[Export] public Color HoverColor_Background { get; set; } = new Color(1f, 1f, 1f, 0.1f);

	#endregion

	#region Private Fields

	/// StyleBoxFlat实例（用于动态渲染边框和背景）
	private StyleBoxFlat _styleBox;

	/// 当前状态
	private CellState _currentState = CellState.Normal;

	/// R3 Subject - 单元格输入事件流
	private readonly Subject<InputEvent> _onCellInputSubject = new();

	/// R3 Subject - 单元格悬停事件流
	private readonly Subject<Unit> _onCellHoverSubject = new();

	#endregion

	#region Public Interface

	/// 暴露单元格输入事件流（供控制器聚合）
	public Observable<InputEvent> OnCellInputAsObservable => _onCellInputSubject;

	/// 暴露单元格悬停事件流（供控制器聚合）
	public Observable<Unit> OnCellHoverAsObservable => _onCellHoverSubject;

	#endregion

	#region Godot Lifecycle
	
	public override void _EnterTree()
	{
		// 【架构修正】在 _EnterTree 中初始化 StyleBox
		// 确保在 AddChild() 后立即调用 SetState() 时 _styleBox 已就绪
		_styleBox ??= new StyleBoxFlat();
	}

	public override void _Ready()
	{
		// 配置StyleBox基础属性
		_styleBox.DrawCenter = true; // 绘制中心背景

		// 设置边框宽度（四边相同）
		_styleBox.BorderWidthLeft = (int)BorderWidth;
		_styleBox.BorderWidthRight = (int)BorderWidth;
		_styleBox.BorderWidthTop = (int)BorderWidth;
		_styleBox.BorderWidthBottom = (int)BorderWidth;

		// 应用StyleBox到Panel
		AddThemeStyleboxOverride("panel", _styleBox);

		// 设置鼠标过滤模式（允许接收输入事件）
		MouseFilter = MouseFilterEnum.Pass;

		// 初始化为Normal状态
		SetState(CellState.Normal);
	}

	public override void _GuiInput(InputEvent @event)
	{
		// 将所有GUI输入事件推送到R3流
		_onCellInputSubject.OnNext(@event);
	}

	public override void _Notification(int what)
	{
		// 监听鼠标进入/离开事件
		if (what == NotificationMouseEnter)
		{
			_onCellHoverSubject.OnNext(Unit.Default);
		}
		else if (what == NotificationMouseExit)
		{
			// 鼠标离开时恢复之前的状态（如果当前是Hover状态）
			if (_currentState == CellState.Hover)
			{
				SetState(CellState.Normal);
			}
		}
	}

	public override void _ExitTree()
	{
		// 清理R3 Subjects
		_onCellInputSubject?.Dispose();
		_onCellHoverSubject?.Dispose();
	}

	#endregion

	#region State Management

	/// 设置单元格状态
	/// 目的：根据状态切换边框和背景颜色
	/// 示例：SetState(CellState.Valid) -> 绿色发光边框 + 淡绿色背景
	/// 算法：1. 根据状态选择颜色 -> 2. 更新StyleBox的BgColor和BorderColor
	public void SetState(CellState state)
	{
		_currentState = state;

		// 根据状态选择颜色
		Color borderColor;
		Color backgroundColor;

		switch (state)
		{
			case CellState.Normal:
				borderColor = NormalColor_Border;
				backgroundColor = NormalColor_Background;
				break;

			case CellState.Valid:
				borderColor = ValidColor_Border;
				backgroundColor = ValidColor_Background;
				break;

			case CellState.Invalid:
				borderColor = InvalidColor_Border;
				backgroundColor = InvalidColor_Background;
				break;

			case CellState.Hover:
				borderColor = HoverColor_Border;
				backgroundColor = HoverColor_Background;
				break;

			default:
				borderColor = NormalColor_Border;
				backgroundColor = NormalColor_Background;
				break;
		}

		// 更新StyleBox颜色
		_styleBox.BgColor = backgroundColor;
		_styleBox.BorderColor = borderColor;
	}

	/// 获取当前状态
	public CellState GetCurrentState()
	{
		return _currentState;
	}

	#endregion
}
