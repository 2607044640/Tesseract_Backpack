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
	private Button _optionButton;
	private PopupMenu _popup;
	
	private int _currentIndex;
	
	protected override void InitializeSpecificNodes()
	{
		_optionButton = GetNodeOrNull<Button>(OptionButtonPath);
		if (_optionButton == null)
		{
			GD.PushError($"[{Name}] OptionButton not found: {OptionButtonPath}");
			return;
		}
		
		// 创建PopupMenu
		if (_optionButton != null && !_optionButton.HasNode("PopupMenu"))
		{
			_popup = new PopupMenu();
			_popup.Name = "PopupMenu";
			_optionButton.AddChild(_popup);
		}
		else if (_optionButton != null)
		{
			_popup = _optionButton.GetNode<PopupMenu>("PopupMenu");
		}
	}
	
	protected override void ConnectSignals()
	{
		if (_optionButton != null)
			_optionButton.Pressed += OnOptionButtonPressed;
		
		if (_popup != null)
			_popup.IndexPressed += OnPopupIndexPressed;
	}
	
	protected override void UpdateControl()
	{
		UpdateOptions();
		UpdateSelection();
	}
	
	private void UpdateOptions()
	{
		if (_popup != null)
		{
			_popup.Clear();
			for (int i = 0; i < Options.Length; i++)
			{
				_popup.AddItem(Options[i], i);
			}
		}
		UpdateSelection();
	}
	
	private void UpdateSelection()
	{
		if (_optionButton != null && Options.Length > 0)
		{
			int index = Mathf.Clamp(DefaultIndex, 0, Options.Length - 1);
			_currentIndex = index;
			_optionButton.Text = Options[index];
		}
	}
	
	private void OnOptionButtonPressed()
	{
		if (_popup != null && _optionButton != null)
		{
			Vector2 buttonPos = _optionButton.GlobalPosition;
			Vector2 buttonSize = _optionButton.Size;
			_popup.Position = (Vector2I)(buttonPos + new Vector2(0, buttonSize.Y));
			_popup.Popup();
		}
	}
	
	private void OnPopupIndexPressed(long index)
	{
		_currentIndex = (int)index;
		if (_optionButton != null && _currentIndex < Options.Length)
		{
			_optionButton.Text = Options[_currentIndex];
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
			if (_optionButton != null)
				_optionButton.Text = Options[index];
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
		if (_optionButton != null)
			_optionButton.Pressed -= OnOptionButtonPressed;
		
		if (_popup != null)
			_popup.IndexPressed -= OnPopupIndexPressed;
	}
}
