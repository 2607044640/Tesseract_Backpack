using Godot;
using System;

/// <summary>
/// 设置组件Helper基类 - 所有设置UI组件的共同基类
/// 提供统一的LabelText、Reset和保存/加载功能
/// </summary>
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
	
	/// <summary>
	/// 初始化共同的节点（Label和ResetButton）
	/// </summary>
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
	
	/// <summary>
	/// 子类实现：初始化特定的控件节点
	/// </summary>
	protected abstract void InitializeSpecificNodes();
	
	/// <summary>
	/// 子类实现：连接特定控件的信号
	/// </summary>
	protected abstract void ConnectSignals();
	
	/// <summary>
	/// 更新Label文本
	/// </summary>
	protected virtual void UpdateLabel()
	{
		if (_label != null)
			_label.Text = LabelText;
	}
	
	/// <summary>
	/// 子类实现：更新控件状态
	/// </summary>
	protected abstract void UpdateControl();
	
	/// <summary>
	/// Reset按钮按下时调用
	/// </summary>
	private void OnResetButtonPressed()
	{
		ResetToDefault();
		ResetRequested?.Invoke();
	}
	
	/// <summary>
	/// 子类实现：重置到默认值
	/// </summary>
	public abstract void ResetToDefault();
	
	/// <summary>
	/// 子类实现：获取当前设置值（用于保存）
	/// 返回Variant类型以支持不同的值类型
	/// </summary>
	public abstract Variant GetSettingValue();
	
	/// <summary>
	/// 子类实现：设置当前值（用于加载）
	/// </summary>
	public abstract void SetSettingValue(Variant value);
	
	/// <summary>
	/// 保存设置到ConfigFile
	/// </summary>
	public void SaveSetting(ConfigFile config, string section = "Settings")
	{
		if (string.IsNullOrEmpty(SettingKey))
		{
			GD.PushWarning($"SettingKey is empty for {Name}, skipping save");
			return;
		}
		
		config.SetValue(section, SettingKey, GetSettingValue());
	}
	
	/// <summary>
	/// 从ConfigFile加载设置
	/// </summary>
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
	
	/// <summary>
	/// 子类实现：断开特定控件的信号连接
	/// </summary>
	protected abstract void DisconnectSignals();
}
