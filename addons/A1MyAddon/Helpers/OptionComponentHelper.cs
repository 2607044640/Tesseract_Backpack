using Godot;
using System;

/// <summary>
/// 选项组件Helper - 用于设置菜单中的下拉选项控件
/// 支持配置选项列表、默认值等
/// </summary>
[Tool]
[GlobalClass]
public partial class OptionComponentHelper : BaseSettingComponentHelper
{
	// ===== 暴露的配置参数 =====
	[ExportCategory("Option Settings")]
	
	private string[] _options = new string[] { "选项1", "选项2", "选项3" };
	[Export] 
	public string[] Options 
	{ 
		get => _options;
		set
		{
			_options = value;
			UpdateControl();
		}
	}
	
	private int _defaultIndex = 0;
	[Export] 
	public int DefaultIndex 
	{ 
		get => _defaultIndex;
		set
		{
			_defaultIndex = value;
			UpdateControl();
		}
	}
	
	// ===== 事件：C# event Action模式 =====
	public event Action<int, string> OptionSelected;
	
	// ===== 内部引用 =====
	[Export] public NodePath OptionButtonPath { get; set; } = "%OptionDropdown_Button";
	private Button Button_Option;
	private PopupMenu PopupMenu_MenuOption;
	
	private int _currentIndex;
	
	protected override void InitializeSpecificNodes()
	{
		Button_Option = GetNodeOrNull<Button>(OptionButtonPath);
		if (Button_Option == null)
		{
			GD.PushError($"[{Name}] OptionButton not found: {OptionButtonPath}");
			return;
		}
		
		// 创建PopupMenu
		if (Button_Option != null && !Button_Option.HasNode("PopupMenu"))
		{
			PopupMenu_MenuOption = new PopupMenu();
			PopupMenu_MenuOption.Name = "PopupMenu";
			Button_Option.AddChild(PopupMenu_MenuOption);
		}
		else if (Button_Option != null)
		{
			PopupMenu_MenuOption = Button_Option.GetNode<PopupMenu>("PopupMenu");
		}
	}
	
	protected override void ConnectSignals()
	{
		if (Button_Option != null)
			Button_Option.Pressed += OnOptionButtonPressed;
		
		if (PopupMenu_MenuOption != null)
			PopupMenu_MenuOption.IndexPressed += OnPopupIndexPressed;
	}
	
	protected override void UpdateControl()
	{
		UpdateOptions();
		UpdateSelection();
	}
	
	private void UpdateOptions()
	{
		if (PopupMenu_MenuOption != null)
		{
			PopupMenu_MenuOption.Clear();
			for (int i = 0; i < Options.Length; i++)
			{
				PopupMenu_MenuOption.AddItem(Options[i], i);
			}
		}
		UpdateSelection();
	}
	
	private void UpdateSelection()
	{
		if (Button_Option != null && Options.Length > 0)
		{
			int index = Mathf.Clamp(DefaultIndex, 0, Options.Length - 1);
			_currentIndex = index;
			Button_Option.Text = Options[index];
		}
	}
	
	private void OnOptionButtonPressed()
	{
		if (PopupMenu_MenuOption != null && Button_Option != null)
		{
			Vector2 buttonPos = Button_Option.GlobalPosition;
			Vector2 buttonSize = Button_Option.Size;
			PopupMenu_MenuOption.Position = (Vector2I)(buttonPos + new Vector2(0, buttonSize.Y));
			PopupMenu_MenuOption.Popup();
		}
	}
	
	private void OnPopupIndexPressed(long index)
	{
		_currentIndex = (int)index;
		if (Button_Option != null && _currentIndex < Options.Length)
		{
			Button_Option.Text = Options[_currentIndex];
			OptionSelected?.Invoke(_currentIndex, Options[_currentIndex]);
		}
	}
	
	public override void ResetToDefault()
	{
		_currentIndex = DefaultIndex;
		UpdateSelection();
	}
	
	public override Variant GetSettingValue()
	{
		return _currentIndex;
	}
	
	public override void SetSettingValue(Variant value)
	{
		SetSelection((int)value);
	}
	
	public void SetSelection(int index)
	{
		if (index >= 0 && index < Options.Length)
		{
			_currentIndex = index;
			if (Button_Option != null)
				Button_Option.Text = Options[index];
		}
	}
	
	public int GetCurrentIndex()
	{
		return _currentIndex;
	}
	
	public string GetCurrentOption()
	{
		return _currentIndex < Options.Length ? Options[_currentIndex] : "";
	}
	
	protected override void DisconnectSignals()
	{
		if (Button_Option != null)
			Button_Option.Pressed -= OnOptionButtonPressed;
		
		if (PopupMenu_MenuOption != null)
			PopupMenu_MenuOption.IndexPressed -= OnPopupIndexPressed;
	}
}
