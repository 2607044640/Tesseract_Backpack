using Godot;
using System;
using R3;

/// 下拉菜单组件Helper - 用于设置菜单中的下拉列表控件
/// 与OptionComponent类似，但使用OptionButton而不是普通Button
/// R3 增强版：支持 ReactiveProperty 双向绑定
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
	
	// ===== 事件：C# event Action模式（向后兼容）=====
	public event Action<int, string> ItemSelected;
	
	// ===== R3 ReactiveProperty（新增）=====
	public ReactiveProperty<int> SelectedIndex { get; private set; }
	
	// ===== 内部引用 =====
	[Export] public NodePath DropdownPath { get; set; } = "%Dropdown_OptionButton";
	private OptionButton OptionButton_Dropdown;
	
	private bool _isUpdating = false;
	private readonly CompositeDisposable _disposables = new();
	
	protected override void InitializeSpecificNodes()
	{
		OptionButton_Dropdown = GetNodeOrNull<OptionButton>(DropdownPath);
		if (OptionButton_Dropdown == null)
		{
			GD.PushError($"[{Name}] Dropdown not found: {DropdownPath}");
			return;
		}
		
		// 初始化 ReactiveProperty
		SelectedIndex = new ReactiveProperty<int>(DefaultIndex);
		
		// 订阅 ReactiveProperty 变化，同步到 UI
		if (!Engine.IsEditorHint())
		{
			SelectedIndex
				.Skip(1) // 跳过初始值
				.Subscribe(index =>
				{
					if (!_isUpdating)
					{
						_isUpdating = true;
						SetSelectionInternal(index);
						_isUpdating = false;
					}
				})
				.AddTo(_disposables);
		}
	}
	
	protected override void ConnectSignals()
	{
		if (OptionButton_Dropdown != null)
			OptionButton_Dropdown.ItemSelected += OnItemSelected;
	}
	
	protected override void UpdateControl()
	{
		UpdateItems();
		UpdateSelection();
	}
	
	private void UpdateItems()
	{
		if (OptionButton_Dropdown != null)
		{
			OptionButton_Dropdown.Clear();
			for (int i = 0; i < Items.Length; i++)
			{
				OptionButton_Dropdown.AddItem(Items[i], i);
			}
		}
		UpdateSelection();
	}
	
	private void UpdateSelection()
	{
		if (OptionButton_Dropdown != null && Items.Length > 0)
		{
			int index = Mathf.Clamp(DefaultIndex, 0, Items.Length - 1);
			OptionButton_Dropdown.Selected = index;
			
			if (SelectedIndex != null)
				SelectedIndex.Value = index;
		}
	}
	
	private void OnItemSelected(long index)
	{
		if (_isUpdating) return;
		
		_isUpdating = true;
		int intIndex = (int)index;
		
		// 更新 ReactiveProperty
		if (SelectedIndex != null)
			SelectedIndex.Value = intIndex;
		
		// 触发传统事件（向后兼容）
		if (intIndex < Items.Length)
		{
			ItemSelected?.Invoke(intIndex, Items[intIndex]);
		}
		
		_isUpdating = false;
	}
	
	private void SetSelectionInternal(int index)
	{
		if (OptionButton_Dropdown != null && index >= 0 && index < Items.Length)
		{
			OptionButton_Dropdown.Selected = index;
		}
	}
	
	public override void ResetToDefault()
	{
		UpdateSelection();
	}
	
	public override Variant GetSettingValue()
	{
		return SelectedIndex?.Value ?? DefaultIndex;
	}
	
	public override void SetSettingValue(Variant value)
	{
		SetSelectedIndex((int)value);
	}
	
	public void SetSelectedIndex(int index)
	{
		if (_isUpdating) return;
		
		_isUpdating = true;
		
		if (OptionButton_Dropdown != null && index >= 0 && index < Items.Length)
		{
			OptionButton_Dropdown.Selected = index;
			
			if (SelectedIndex != null)
				SelectedIndex.Value = index;
		}
		
		_isUpdating = false;
	}
	
	public int GetCurrentIndex()
	{
		return SelectedIndex?.Value ?? DefaultIndex;
	}
	
	public string GetCurrentItem()
	{
		int index = GetCurrentIndex();
		return index < Items.Length ? Items[index] : "";
	}
	
	public string GetSelectedText()
	{
		return GetCurrentItem();
	}
	
	protected override void DisconnectSignals()
	{
		if (OptionButton_Dropdown != null)
			OptionButton_Dropdown.ItemSelected -= OnItemSelected;
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		_disposables.Dispose();
	}
}
