using Godot;
using System;

/// <summary>
/// 开关组件Helper - 用于设置菜单中的复选框控件
/// 支持配置标签文本、默认状态等
/// </summary>
[Tool]
[GlobalClass]
public partial class ToggleComponentHelper : BaseSettingComponentHelper
{
	// ===== 暴露的配置参数 =====
	[ExportCategory("Toggle Settings")]
	
	private bool _defaultState = false;
	[Export] 
	public bool DefaultState 
	{ 
		get => _defaultState;
		set
		{
			_defaultState = value;
			UpdateControl();
		}
	}
	
	// ===== 事件：C# event Action模式 =====
	public event Action<bool> Toggled;
	
	// ===== 内部引用 =====
	private CheckBox _checkbox;
	
	private bool _currentState;
	
	protected override void InitializeSpecificNodes()
	{
		_checkbox = GetNodeOrNull<CheckBox>("ToggleCheckbox_CheckBox");
	}
	
	protected override void ConnectSignals()
	{
		if (_checkbox != null)
			_checkbox.Toggled += OnCheckboxToggled;
	}
	
	protected override void UpdateControl()
	{
		if (_checkbox != null)
		{
			_checkbox.ButtonPressed = DefaultState;
			_currentState = DefaultState;
		}
	}
	
	private void OnCheckboxToggled(bool toggledOn)
	{
		_currentState = toggledOn;
		Toggled?.Invoke(_currentState);
	}
	
	public override void ResetToDefault()
	{
		if (_checkbox != null)
			_checkbox.ButtonPressed = DefaultState;
	}
	
	public override Variant GetSettingValue()
	{
		return _currentState;
	}
	
	public override void SetSettingValue(Variant value)
	{
		SetState((bool)value);
	}
	
	public void SetState(bool state)
	{
		if (_checkbox != null)
			_checkbox.ButtonPressed = state;
	}
	
	public bool GetState()
	{
		return _currentState;
	}
	
	protected override void DisconnectSignals()
	{
		if (_checkbox != null)
			_checkbox.Toggled -= OnCheckboxToggled;
	}
}
