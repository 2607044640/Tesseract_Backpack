using Godot;
using System;
using R3;

/// <summary>
/// 开关组件Helper - 用于设置菜单中的复选框控件
/// 支持配置标签文本、默认状态等
/// R3 增强版：支持 ReactiveProperty 双向绑定
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
	
	// ===== 事件：C# event Action模式（向后兼容）=====
	public event Action<bool> Toggled;
	
	// ===== R3 ReactiveProperty（新增）=====
	public ReactiveProperty<bool> IsToggled { get; private set; }
	
	// ===== 内部引用 =====
	[Export] public NodePath CheckboxPath { get; set; } = "%ToggleCheckbox_CheckBox";
	private CheckBox _checkbox;
	
	private bool _isUpdating = false;
	private readonly CompositeDisposable _disposables = new();
	
	protected override void InitializeSpecificNodes()
	{
		_checkbox = GetNodeOrNull<CheckBox>(CheckboxPath);
		if (_checkbox == null)
		{
			GD.PushError($"[{Name}] Checkbox not found: {CheckboxPath}");
			return;
		}
		
		// 初始化 ReactiveProperty
		IsToggled = new ReactiveProperty<bool>(DefaultState);
		
		// 订阅 ReactiveProperty 变化，同步到 UI
		if (!Engine.IsEditorHint())
		{
			IsToggled
				.Skip(1) // 跳过初始值
				.Subscribe(state =>
				{
					if (!_isUpdating)
					{
						_isUpdating = true;
						SetStateInternal(state);
						_isUpdating = false;
					}
				})
				.AddTo(_disposables);
		}
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
		}
		
		if (IsToggled != null)
		{
			IsToggled.Value = DefaultState;
		}
	}
	
	private void OnCheckboxToggled(bool toggledOn)
	{
		if (_isUpdating) return;
		
		_isUpdating = true;
		
		// 更新 ReactiveProperty
		if (IsToggled != null)
			IsToggled.Value = toggledOn;
		
		// 触发传统事件（向后兼容）
		Toggled?.Invoke(toggledOn);
		
		_isUpdating = false;
	}
	
	private void SetStateInternal(bool state)
	{
		if (_checkbox != null)
			_checkbox.ButtonPressed = state;
	}
	
	public override void ResetToDefault()
	{
		if (_checkbox != null)
			_checkbox.ButtonPressed = DefaultState;
	}
	
	public override Variant GetSettingValue()
	{
		return IsToggled?.Value ?? DefaultState;
	}
	
	public override void SetSettingValue(Variant value)
	{
		SetState((bool)value);
	}
	
	public void SetState(bool state)
	{
		if (_isUpdating) return;
		
		_isUpdating = true;
		
		if (_checkbox != null)
			_checkbox.ButtonPressed = state;
		
		if (IsToggled != null)
			IsToggled.Value = state;
		
		_isUpdating = false;
	}
	
	public bool GetState()
	{
		return IsToggled?.Value ?? DefaultState;
	}
	
	protected override void DisconnectSignals()
	{
		if (_checkbox != null)
			_checkbox.Toggled -= OnCheckboxToggled;
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		_disposables.Dispose();
	}
}
