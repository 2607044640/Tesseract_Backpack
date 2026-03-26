using Godot;
using System;

/// <summary>
/// 下拉菜单组件Helper - 用于设置菜单中的下拉列表控件
/// 与OptionComponent类似，但使用OptionButton而不是普通Button
/// </summary>
[Tool]
[GlobalClass]
public partial class DropdownComponentHelper : BaseSettingComponentHelper
{
	// ===== 暴露的配置参数 =====
	[ExportCategory("Dropdown Settings")]
	
	private string[] _items = new string[] { "选项1", "选项2", "选项3" };
	[Export] 
	public string[] Items 
	{ 
		get => _items;
		set
		{
			_items = value;
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
	public event Action<int, string> ItemSelected;
	
	// ===== 内部引用 =====
	private OptionButton _dropdown;
	
	private int _currentIndex;
	
	protected override void InitializeSpecificNodes()
	{
		_dropdown = GetNodeOrNull<OptionButton>("Dropdown_OptionButton");
	}
	
	protected override void ConnectSignals()
	{
		if (_dropdown != null)
			_dropdown.ItemSelected += OnItemSelected;
	}
	
	protected override void UpdateControl()
	{
		UpdateItems();
		UpdateSelection();
	}
	
	private void UpdateItems()
	{
		if (_dropdown != null)
		{
			_dropdown.Clear();
			for (int i = 0; i < Items.Length; i++)
			{
				_dropdown.AddItem(Items[i], i);
			}
		}
		UpdateSelection();
	}
	
	private void UpdateSelection()
	{
		if (_dropdown != null && Items.Length > 0)
		{
			int index = Mathf.Clamp(DefaultIndex, 0, Items.Length - 1);
			_currentIndex = index;
			_dropdown.Selected = index;
		}
	}
	
	private void OnItemSelected(long index)
	{
		_currentIndex = (int)index;
		if (_currentIndex < Items.Length)
		{
			ItemSelected?.Invoke(_currentIndex, Items[_currentIndex]);
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
		if (_dropdown != null && index >= 0 && index < Items.Length)
		{
			_currentIndex = index;
			_dropdown.Selected = index;
		}
	}
	
	public int GetCurrentIndex()
	{
		return _currentIndex;
	}
	
	public string GetCurrentItem()
	{
		return _currentIndex < Items.Length ? Items[_currentIndex] : "";
	}
	
	protected override void DisconnectSignals()
	{
		if (_dropdown != null)
			_dropdown.ItemSelected -= OnItemSelected;
	}
}
