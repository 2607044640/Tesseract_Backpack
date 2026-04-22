using Godot;
using System;
using R3;

/// 滑块组件Helper - 用于设置菜单中的滑块控件
/// 遵循MarginContainerHelper的模式：[Tool] + 属性setter立即更新UI
/// R3 增强版：支持 ReactiveProperty 双向绑定
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
	
	// ===== 事件：C# event Action模式（向后兼容）=====
	public event Action<float> ValueChanged;
	
	// ===== R3 ReactiveProperty（新增）=====
	public ReactiveProperty<float> Value { get; private set; }
	
	// ===== 内部引用 =====
	[Export] public NodePath SliderPath { get; set; } = "%SliderBar_HSlider";
	[Export] public NodePath SpinBoxPath { get; set; } = "%ValueSpinBox_SpinBox";
	private HSlider _slider;
	private SpinBox _spinBox;
	
	private bool _isUpdating = false; // 防止循环更新
	private readonly CompositeDisposable _disposables = new();
	
	protected override void InitializeSpecificNodes()
	{
		_slider = GetNodeOrNull<HSlider>(SliderPath);
		if (_slider == null)
		{
			GD.PushError($"[{Name}] Slider not found: {SliderPath}");
			return;
		}
		
		_spinBox = GetNodeOrNull<SpinBox>(SpinBoxPath);
		if (_spinBox == null)
		{
			GD.PushError($"[{Name}] SpinBox not found: {SpinBoxPath}");
			return;
		}
		
		// 初始化 ReactiveProperty
		Value = new ReactiveProperty<float>(DefaultValue);
		
		// 订阅 ReactiveProperty 变化，同步到 UI
		if (!Engine.IsEditorHint())
		{
			Value
				.Skip(1) // 跳过初始值
				.Subscribe(v =>
				{
					if (!_isUpdating)
					{
						_isUpdating = true;
						SetValueInternal(v);
						_isUpdating = false;
					}
				})
				.AddTo(_disposables);
		}
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
		}
		
		if (_spinBox != null)
		{
			_spinBox.MinValue = MinValue;
			_spinBox.MaxValue = MaxValue;
			_spinBox.Step = Step;
			_spinBox.Value = DefaultValue;
		}
		
		if (Value != null)
		{
			Value.Value = DefaultValue;
		}
	}
	
	private void OnSliderValueChanged(double value)
	{
		if (_isUpdating) return;
		
		_isUpdating = true;
		float floatValue = (float)value;
		
		// 同步到 SpinBox
		if (_spinBox != null)
			_spinBox.Value = value;
		
		// 更新 ReactiveProperty（会自动触发订阅者）
		if (Value != null)
			Value.Value = floatValue;
		
		// 触发传统事件（向后兼容）
		ValueChanged?.Invoke(floatValue);
		
		_isUpdating = false;
	}
	
	private void OnSpinBoxValueChanged(double value)
	{
		if (_isUpdating) return;
		
		_isUpdating = true;
		float floatValue = (float)value;
		
		// 同步到 Slider
		if (_slider != null)
			_slider.Value = value;
		
		// 更新 ReactiveProperty
		if (Value != null)
			Value.Value = floatValue;
		
		// 触发传统事件
		ValueChanged?.Invoke(floatValue);
		
		_isUpdating = false;
	}
	
	private void SetValueInternal(float value)
	{
		if (_slider != null)
			_slider.Value = value;
		
		if (_spinBox != null)
			_spinBox.Value = value;
	}
	
	public override void ResetToDefault()
	{
		if (_slider != null)
			_slider.Value = DefaultValue;
	}
	
	public override Variant GetSettingValue()
	{
		return Value?.Value ?? DefaultValue;
	}
	
	public override void SetSettingValue(Variant value)
	{
		SetValue((float)value);
	}
	
	public void SetValue(float value)
	{
		if (_isUpdating) return;
		
		_isUpdating = true;
		
		if (_slider != null)
			_slider.Value = value;
		
		if (_spinBox != null)
			_spinBox.Value = value;
		
		if (Value != null)
			Value.Value = value;
		
		_isUpdating = false;
	}
	
	public float GetValue()
	{
		return Value?.Value ?? DefaultValue;
	}
	
	protected override void DisconnectSignals()
	{
		if (_slider != null)
			_slider.ValueChanged -= OnSliderValueChanged;
		
		if (_spinBox != null)
			_spinBox.ValueChanged -= OnSpinBoxValueChanged;
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		_disposables.Dispose();
	}
}
