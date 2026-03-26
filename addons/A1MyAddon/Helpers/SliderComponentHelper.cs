using Godot;
using System;

/// <summary>
/// 滑块组件Helper - 用于设置菜单中的滑块控件
/// 遵循MarginContainerHelper的模式：[Tool] + 属性setter立即更新UI
/// </summary>
[Tool]
[GlobalClass]
public partial class SliderComponentHelper : BaseSettingComponentHelper
{
	// ===== 暴露的配置参数 =====
	[ExportCategory("Slider Settings")]
	
	private float _minValue = 0f;
	[Export] 
	public float MinValue 
	{ 
		get => _minValue;
		set
		{
			_minValue = value;
			UpdateControl();
		}
	}
	
	private float _maxValue = 100f;
	[Export] 
	public float MaxValue 
	{ 
		get => _maxValue;
		set
		{
			_maxValue = value;
			UpdateControl();
		}
	}
	
	private float _step = 1f;
	[Export] 
	public float Step 
	{ 
		get => _step;
		set
		{
			_step = value;
			UpdateControl();
		}
	}
	
	private float _defaultValue = 50f;
	[Export] 
	public float DefaultValue 
	{ 
		get => _defaultValue;
		set
		{
			_defaultValue = value;
			UpdateControl();
		}
	}
	
	private int _tickCount = 11;
	[Export] 
	public int TickCount 
	{ 
		get => _tickCount;
		set
		{
			_tickCount = value;
			UpdateControl();
		}
	}
	
	private bool _ticksOnBorders = true;
	[Export] 
	public bool TicksOnBorders 
	{ 
		get => _ticksOnBorders;
		set
		{
			_ticksOnBorders = value;
			UpdateControl();
		}
	}
	
	// ===== 事件：C# event Action模式 =====
	public event Action<float> ValueChanged;
	
	// ===== 内部引用 =====
	private HSlider _slider;
	private SpinBox _spinBox;
	
	private float _currentValue;
	private bool _isUpdating = false; // 防止循环更新
	
	protected override void InitializeSpecificNodes()
	{
		_slider = GetNodeOrNull<HSlider>("SliderBar_HSlider");
		_spinBox = GetNodeOrNull<SpinBox>("ValueSpinBox_SpinBox");
	}
	
	protected override void ConnectSignals()
	{
		if (_slider != null)
			_slider.ValueChanged += OnSliderValueChanged;
		
		if (_spinBox != null)
			_spinBox.ValueChanged += OnSpinBoxValueChanged;
	}
	
	protected override void UpdateControl()
	{
		if (_slider != null)
		{
			_slider.MinValue = MinValue;
			_slider.MaxValue = MaxValue;
			_slider.Step = Step;
			_slider.Value = DefaultValue;
			_slider.TickCount = TickCount;
			_slider.TicksOnBorders = TicksOnBorders;
			_currentValue = DefaultValue;
		}
		
		if (_spinBox != null)
		{
			_spinBox.MinValue = MinValue;
			_spinBox.MaxValue = MaxValue;
			_spinBox.Step = Step;
			_spinBox.Value = DefaultValue;
		}
	}
	
	private void OnSliderValueChanged(double value)
	{
		if (_isUpdating) return;
		
		_isUpdating = true;
		_currentValue = (float)value;
		
		// 同步到SpinBox
		if (_spinBox != null)
			_spinBox.Value = value;
		
		ValueChanged?.Invoke(_currentValue);
		_isUpdating = false;
	}
	
	private void OnSpinBoxValueChanged(double value)
	{
		if (_isUpdating) return;
		
		_isUpdating = true;
		_currentValue = (float)value;
		
		// 同步到Slider
		if (_slider != null)
			_slider.Value = value;
		
		ValueChanged?.Invoke(_currentValue);
		_isUpdating = false;
	}
	
	public override void ResetToDefault()
	{
		if (_slider != null)
			_slider.Value = DefaultValue;
	}
	
	public override Variant GetSettingValue()
	{
		return _currentValue;
	}
	
	public override void SetSettingValue(Variant value)
	{
		SetValue((float)value);
	}
	
	public void SetValue(float value)
	{
		if (_slider != null)
			_slider.Value = value;
		
		if (_spinBox != null)
			_spinBox.Value = value;
	}
	
	public float GetValue()
	{
		return _currentValue;
	}
	
	protected override void DisconnectSignals()
	{
		if (_slider != null)
			_slider.ValueChanged -= OnSliderValueChanged;
		
		if (_spinBox != null)
			_spinBox.ValueChanged -= OnSpinBoxValueChanged;
	}
}
