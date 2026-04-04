using Godot;
using R3;

/// <summary>
/// 设置管理器 - 使用 R3 ReactiveProperty 实现响应式状态管理
/// 所有设置状态集中管理，UI 自动同步，无需手动事件订阅
/// </summary>
public partial class SettingsManager : Node
{
	private const string SettingsFilePath = "user://settings.cfg";
	private const string SettingsSection = "Settings";
	
	private ConfigFile _config;
	private readonly CompositeDisposable _disposables = new();
	
	// ===== Audio Settings (ReactiveProperty) =====
	public ReactiveProperty<float> MasterVolume { get; private set; }
	public ReactiveProperty<float> MusicVolume { get; private set; }
	public ReactiveProperty<float> SFXVolume { get; private set; }
	public ReactiveProperty<bool> Mute { get; private set; }
	
	// ===== Video Settings (ReactiveProperty) =====
	public ReactiveProperty<bool> Fullscreen { get; private set; }
	public ReactiveProperty<int> ResolutionIndex { get; private set; }
	public ReactiveProperty<int> AntiAliasingIndex { get; private set; }
	public ReactiveProperty<int> CameraShakeIndex { get; private set; }
	
	public override void _Ready()
	{
		_config = new ConfigFile();
		
		// 加载配置文件
		LoadConfig();
		
		// 初始化 ReactiveProperty（从配置文件加载或使用默认值）
		MasterVolume = new ReactiveProperty<float>(LoadFloat("master_volume", 100f));
		MusicVolume = new ReactiveProperty<float>(LoadFloat("music_volume", 80f));
		SFXVolume = new ReactiveProperty<float>(LoadFloat("sfx_volume", 80f));
		Mute = new ReactiveProperty<bool>(LoadBool("mute", false));
		
		Fullscreen = new ReactiveProperty<bool>(LoadBool("fullscreen", false));
		ResolutionIndex = new ReactiveProperty<int>(LoadInt("resolution", 0));
		AntiAliasingIndex = new ReactiveProperty<int>(LoadInt("anti_aliasing", 0));
		CameraShakeIndex = new ReactiveProperty<int>(LoadInt("camera_shake", 1));
		
		// 订阅所有 ReactiveProperty 变化，自动保存
		SubscribeAutoSave();
		
		GD.Print("SettingsManager initialized with ReactiveProperty");
	}
	
	/// <summary>
	/// 订阅所有设置变化，自动保存到配置文件
	/// </summary>
	private void SubscribeAutoSave()
	{
		// Audio settings auto-save
		MasterVolume
			.Skip(1) // 跳过初始值，避免重复保存
			.Subscribe(v => SaveFloat("master_volume", v))
			.AddTo(_disposables);
		
		MusicVolume
			.Skip(1)
			.Subscribe(v => SaveFloat("music_volume", v))
			.AddTo(_disposables);
		
		SFXVolume
			.Skip(1)
			.Subscribe(v => SaveFloat("sfx_volume", v))
			.AddTo(_disposables);
		
		Mute
			.Skip(1)
			.Subscribe(v => SaveBool("mute", v))
			.AddTo(_disposables);
		
		// Video settings auto-save
		Fullscreen
			.Skip(1)
			.Subscribe(v => SaveBool("fullscreen", v))
			.AddTo(_disposables);
		
		ResolutionIndex
			.Skip(1)
			.Subscribe(v => SaveInt("resolution", v))
			.AddTo(_disposables);
		
		AntiAliasingIndex
			.Skip(1)
			.Subscribe(v => SaveInt("anti_aliasing", v))
			.AddTo(_disposables);
		
		CameraShakeIndex
			.Skip(1)
			.Subscribe(v => SaveInt("camera_shake", v))
			.AddTo(_disposables);
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
	/// 重置所有设置到默认值
	/// </summary>
	public void ResetAllSettings()
	{
		MasterVolume.Value = 100f;
		MusicVolume.Value = 80f;
		SFXVolume.Value = 80f;
		Mute.Value = false;
		
		Fullscreen.Value = false;
		ResolutionIndex.Value = 0;
		AntiAliasingIndex.Value = 0;
		CameraShakeIndex.Value = 1;
		
		GD.Print("All settings reset to defaults");
	}
	
	// ===== Helper Methods for Loading =====
	
	private float LoadFloat(string key, float defaultValue)
	{
		if (_config.HasSectionKey(SettingsSection, key))
		{
			return (float)_config.GetValue(SettingsSection, key);
		}
		return defaultValue;
	}
	
	private int LoadInt(string key, int defaultValue)
	{
		if (_config.HasSectionKey(SettingsSection, key))
		{
			return (int)_config.GetValue(SettingsSection, key);
		}
		return defaultValue;
	}
	
	private bool LoadBool(string key, bool defaultValue)
	{
		if (_config.HasSectionKey(SettingsSection, key))
		{
			return (bool)_config.GetValue(SettingsSection, key);
		}
		return defaultValue;
	}
	
	// ===== Helper Methods for Saving =====
	
	private void SaveFloat(string key, float value)
	{
		_config.SetValue(SettingsSection, key, value);
		SaveConfigFile();
	}
	
	private void SaveInt(string key, int value)
	{
		_config.SetValue(SettingsSection, key, value);
		SaveConfigFile();
	}
	
	private void SaveBool(string key, bool value)
	{
		_config.SetValue(SettingsSection, key, value);
		SaveConfigFile();
	}
	
	private void SaveConfigFile()
	{
		Error err = _config.Save(SettingsFilePath);
		if (err == Error.Ok)
		{
			GD.Print($"Settings saved to {SettingsFilePath}");
		}
		else
		{
			GD.PrintErr($"Failed to save settings: {err}");
		}
	}
	
	public override void _ExitTree()
	{
		_disposables.Dispose();
	}
}
