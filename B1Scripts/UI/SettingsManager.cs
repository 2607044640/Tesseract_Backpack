using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 设置管理器 - 统一管理所有设置组件的保存和加载
/// 使用ConfigFile进行持久化存储
/// </summary>
public partial class SettingsManager : Node
{
	private const string SettingsFilePath = "user://settings.cfg";
	private const string SettingsSection = "Settings";
	
	private ConfigFile _config;
	private List<BaseSettingComponentHelper> _settingComponents;
	
	public override void _Ready()
	{
		_config = new ConfigFile();
		_settingComponents = new List<BaseSettingComponentHelper>();
		
		// 加载现有配置
		LoadConfig();
	}
	
	/// <summary>
	/// 注册一个设置组件
	/// </summary>
	public void RegisterComponent(BaseSettingComponentHelper component)
	{
		if (component != null && !_settingComponents.Contains(component))
		{
			_settingComponents.Add(component);
			
			// 订阅ResetRequested事件，当用户点击Reset时自动保存
			component.ResetRequested += () => SaveSettings();
		}
	}
	
	/// <summary>
	/// 批量注册多个设置组件
	/// </summary>
	public void RegisterComponents(params BaseSettingComponentHelper[] components)
	{
		foreach (var component in components)
		{
			RegisterComponent(component);
		}
	}
	
	/// <summary>
	/// 从文件加载配置
	/// </summary>
	private void LoadConfig()
	{
		Error err = _config.Load(SettingsFilePath);
		if (err != Error.Ok)
		{
			GD.Print($"Settings file not found or error loading: {err}. Using defaults.");
		}
		else
		{
			GD.Print("Settings loaded successfully");
		}
	}
	
	/// <summary>
	/// 加载所有已注册组件的设置
	/// </summary>
	public void LoadSettings()
	{
		LoadConfig();
		
		foreach (var component in _settingComponents)
		{
			component.LoadSetting(_config, SettingsSection);
		}
		
		GD.Print($"Loaded settings for {_settingComponents.Count} components");
	}
	
	/// <summary>
	/// 保存所有已注册组件的设置
	/// </summary>
	public void SaveSettings()
	{
		foreach (var component in _settingComponents)
		{
			component.SaveSetting(_config, SettingsSection);
		}
		
		Error err = _config.Save(SettingsFilePath);
		if (err == Error.Ok)
		{
			GD.Print($"Settings saved successfully to {SettingsFilePath}");
		}
		else
		{
			GD.PrintErr($"Failed to save settings: {err}");
		}
	}
	
	/// <summary>
	/// 重置所有设置到默认值
	/// </summary>
	public void ResetAllSettings()
	{
		foreach (var component in _settingComponents)
		{
			component.ResetToDefault();
		}
		
		SaveSettings();
		GD.Print("All settings reset to defaults");
	}
	
	/// <summary>
	/// 获取特定设置的值
	/// </summary>
	public Variant GetSetting(string key, Variant defaultValue = default)
	{
		if (_config.HasSectionKey(SettingsSection, key))
		{
			return _config.GetValue(SettingsSection, key);
		}
		return defaultValue;
	}
	
	/// <summary>
	/// 设置特定设置的值
	/// </summary>
	public void SetSetting(string key, Variant value)
	{
		_config.SetValue(SettingsSection, key, value);
	}
	
	public override void _ExitTree()
	{
		// 退出时自动保存
		SaveSettings();
		
		// 取消订阅所有组件
		foreach (var component in _settingComponents)
		{
			if (component != null)
			{
				component.ResetRequested -= SaveSettings;
			}
		}
	}
}
