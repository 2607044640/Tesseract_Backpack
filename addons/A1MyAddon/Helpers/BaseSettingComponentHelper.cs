using Godot;
using System;

/// 设置组件Helper基类 - 所有设置UI组件的共同基类
/// 提供统一的LabelText、Reset和保存/加载功能
[Tool]
[GlobalClass]
public abstract partial class BaseSettingComponentHelper : HBoxContainer
{
	// ===== 共同的配置参数 =====
	[ExportCategory("Base Settings")]
	
	private string _labelText = "设置项:";
	[Export] 
	public string LabelText 
	{ 
		get => _labelText;
		set
		{
			_labelText = value;
			UpdateLabel();
		}
	}
	
	// ===== 共同的事件 =====
	public event Action ResetRequested;
	
	// ===== 保护的内部引用（子类可访问）=====
	protected Label _label;
	protected Button _resetButton;
	
	// ===== 设置键（用于保存/加载）=====
	private string _settingKey = "";
	[Export]
	public string SettingKey
	{
		get => _settingKey;
		set => _settingKey = value;
	}
	
	public override void _Ready()
	{
		// 子类需要先调用base._Ready()，然后获取自己的特定节点
		InitializeCommonNodes();
		InitializeSpecificNodes();
		
		UpdateLabel();
		UpdateControl();
		
		// 连接Reset按钮（仅在运行时）
		if (!Engine.IsEditorHint())
		{
			if (_resetButton != null)
				_resetButton.Pressed += OnResetButtonPressed;
			
			ConnectSignals();
		}
	}
	
	/// 初始化共同的节点（Label和ResetButton）
	private void InitializeCommonNodes()
	{
		// 按类型查找第一个Label节点
		_label = this.GetRequiredComponentInChildren<Label>();
		
		// 按类型查找名称包含"Reset"的Button
		foreach (var child in GetChildren())
		{
			if (child is Button btn && child.Name.ToString().Contains("Reset"))
			{
				_resetButton = btn;
				break;
			}
		}
		
		if (_resetButton == null)
		{
			GD.PushError($"ResetButton not found in {Name}");
		}
	}
	
	/// 子类实现：初始化特定的控件节点
	protected abstract void InitializeSpecificNodes();
	
	/// 子类实现：连接特定控件的信号
	protected abstract void ConnectSignals();
	
	/// 更新Label文本
	protected virtual void UpdateLabel()
	{
		if (_label != null)
			_label.Text = LabelText;
	}
	
	/// 子类实现：更新控件状态
	protected abstract void UpdateControl();
	
	/// Reset按钮按下时调用
	private void OnResetButtonPressed()
	{
		ResetToDefault();
		ResetRequested?.Invoke();
	}
	
	/// 子类实现：重置到默认值
	public abstract void ResetToDefault();
	
	/// 子类实现：获取当前设置值（用于保存）
	/// 返回Variant类型以支持不同的值类型
	public abstract Variant GetSettingValue();
	
	/// 子类实现：设置当前值（用于加载）
	public abstract void SetSettingValue(Variant value);
	
	/// 保存设置到ConfigFile
	public void SaveSetting(ConfigFile config, string section = "Settings")
	{
		if (string.IsNullOrEmpty(SettingKey))
		{
			GD.PushWarning($"SettingKey is empty for {Name}, skipping save");
			return;
		}
		
		config.SetValue(section, SettingKey, GetSettingValue());
	}
	
	/// 从ConfigFile加载设置
	public void LoadSetting(ConfigFile config, string section = "Settings")
	{
		if (string.IsNullOrEmpty(SettingKey))
		{
			GD.PushWarning($"SettingKey is empty for {Name}, skipping load");
			return;
		}
		
		// 安全检查：确保节点已初始化
		if (_label == null || _resetButton == null)
		{
			GD.PushWarning($"Component {Name} not fully initialized, deferring load");
			// 延迟加载，等待_Ready()完成
			CallDeferred(MethodName.LoadSetting, config, section);
			return;
		}
		
		if (config.HasSectionKey(section, SettingKey))
		{
			Variant value = config.GetValue(section, SettingKey);
			SetSettingValue(value);
		}
	}
	
	public override void _ExitTree()
	{
		if (!Engine.IsEditorHint())
		{
			if (_resetButton != null)
				_resetButton.Pressed -= OnResetButtonPressed;
			
			DisconnectSignals();
		}
	}
	
	/// 子类实现：断开特定控件的信号连接
	protected abstract void DisconnectSignals();
}
