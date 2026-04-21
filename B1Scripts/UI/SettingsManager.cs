using Godot;
using R3;
using System;
using System.Collections.Generic;
using GameSettings;

/// <summary>
/// 设置管理器 - R3 ReactiveProperty 响应式状态管理
/// 所有设置状态集中管理，UI 自动同步
/// 
/// ✅ 优化点：
/// 1. 使用通用绑定器减少重复代码
/// 2. 统一的自动保存（Debounce 500ms）避免频繁 I/O
/// 3. 简化的重置逻辑
/// </summary>
public partial class SettingsManager : Node
{
	private const string SettingsFilePath = "user://settings.cfg";
	private const string SettingsSection = "Settings";
	
	private ConfigFile ConfigFile_Settings;
	private readonly CompositeDisposable CompositeDisposable_Disposables = new();
	
	// ✅ 使用绑定器管理所有设置
	private readonly List<SettingBinderBase<float>> _floatBinders = new();
	private readonly List<SettingBinderBase<bool>> _boolBinders = new();
	private readonly List<SettingBinderBase<int>> _intBinders = new();
	
	// ===== 默认值配置（可在编辑器中修改）=====
	[ExportGroup("Audio Defaults")]
	[Export] public float DefaultMasterVolume { get; set; } = 100f;
	[Export] public float DefaultMusicVolume { get; set; } = 80f;
	[Export] public float DefaultSFXVolume { get; set; } = 80f;
	[Export] public bool DefaultMute { get; set; } = false;
	
	[ExportGroup("Video Defaults")]
	[Export] public bool DefaultFullscreen { get; set; } = false;
	[Export] public int DefaultResolutionIndex { get; set; } = 0;
	[Export] public int DefaultAntiAliasingIndex { get; set; } = 0;
	[Export] public int DefaultCameraShakeIndex { get; set; } = 1;
	
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
		ConfigFile_Settings = new ConfigFile();
		LoadConfig();
		
		// ✅ 使用绑定器创建所有设置（使用 [Export] 默认值）
		MasterVolume = CreateFloatSetting("master_volume", DefaultMasterVolume);
		MusicVolume = CreateFloatSetting("music_volume", DefaultMusicVolume);
		SFXVolume = CreateFloatSetting("sfx_volume", DefaultSFXVolume);
		Mute = CreateBoolSetting("mute", DefaultMute);
		
		Fullscreen = CreateBoolSetting("fullscreen", DefaultFullscreen);
		ResolutionIndex = CreateIntSetting("resolution", DefaultResolutionIndex);
		AntiAliasingIndex = CreateIntSetting("anti_aliasing", DefaultAntiAliasingIndex);
		CameraShakeIndex = CreateIntSetting("camera_shake", DefaultCameraShakeIndex);
		
		// ✅ 统一的自动保存（Debounce 500ms）
		SubscribeAutoSave();
		
		GD.Print("SettingsManager initialized with optimized binders");
	}
	
	/// <summary>
	/// ✅ 创建 Float 设置（使用绑定器）
	/// </summary>
	private ReactiveProperty<float> CreateFloatSetting(string key, float defaultValue)
	{
		var binder = new FloatSettingBinder(key, defaultValue, ConfigFile_Settings, SettingsSection);
		_floatBinders.Add(binder);
		return binder.GetProperty();
	}
	
	/// <summary>
	/// ✅ 创建 Bool 设置（使用绑定器）
	/// </summary>
	private ReactiveProperty<bool> CreateBoolSetting(string key, bool defaultValue)
	{
		var binder = new BoolSettingBinder(key, defaultValue, ConfigFile_Settings, SettingsSection);
		_boolBinders.Add(binder);
		return binder.GetProperty();
	}
	
	/// <summary>
	/// ✅ 创建 Int 设置（使用绑定器）
	/// </summary>
	private ReactiveProperty<int> CreateIntSetting(string key, int defaultValue)
	{
		var binder = new IntSettingBinder(key, defaultValue, ConfigFile_Settings, SettingsSection);
		_intBinders.Add(binder);
		return binder.GetProperty();
	}
	
	/// <summary>
	/// ✅ 优化后的自动保存：合并所有变化 + Debounce 500ms
	/// 
	/// 优化点：
	/// 1. 削峰填谷：用户拖动滑块时，不再每帧写入磁盘，而是等待 500ms 后统一保存
	/// 2. 减少 I/O：将成百上千次的写入压缩为一次
	/// 3. 避免卡顿：防止频繁磁盘操作导致的 UI 卡顿
	/// 
	/// 示例：用户拖动音量滑块 0-100，触发 100 次变化
	/// - 优化前：写入磁盘 100 次（卡顿）
	/// - 优化后：等待 500ms 后写入 1 次（流畅）
	/// </summary>
	private void SubscribeAutoSave()
	{
		Observable.Merge(
			MasterVolume.Skip(1).AsUnitObservable(),
			MusicVolume.Skip(1).AsUnitObservable(),
			SFXVolume.Skip(1).AsUnitObservable(),
			Mute.Skip(1).AsUnitObservable(),
			Fullscreen.Skip(1).AsUnitObservable(),
			ResolutionIndex.Skip(1).AsUnitObservable(),
			AntiAliasingIndex.Skip(1).AsUnitObservable(),
			CameraShakeIndex.Skip(1).AsUnitObservable()
		)
		.Debounce(TimeSpan.FromMilliseconds(500)) // ✅ 等待 500ms 后保存
		.Subscribe(_ => SaveAllSettings())
		.AddTo(CompositeDisposable_Disposables);
		
		GD.Print("Auto-save enabled with 500ms debounce");
	}
	
	/// <summary>
	/// ✅ 一次性保存所有设置（使用绑定器）
	/// </summary>
	private void SaveAllSettings()
	{
		// 使用绑定器保存所有设置
		foreach (var binder in _floatBinders)
		{
			binder.SaveValue();
		}
		foreach (var binder in _boolBinders)
		{
			binder.SaveValue();
		}
		foreach (var binder in _intBinders)
		{
			binder.SaveValue();
		}
		
		// 一次性写入文件
		Error err = ConfigFile_Settings.Save(SettingsFilePath);
		if (err == Error.Ok)
		{
			GD.Print($"✓ Settings saved to {SettingsFilePath}");
		}
		else
		{
			GD.PrintErr($"✗ Failed to save settings: {err}");
		}
	}
	
	private void LoadConfig()
	{
		Error err = ConfigFile_Settings.Load(SettingsFilePath);
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
	/// ✅ 重置所有设置到默认值（使用 [Export] 配置的默认值）
	/// </summary>
	public void ResetAllSettings()
	{
		MasterVolume.Value = DefaultMasterVolume;
		MusicVolume.Value = DefaultMusicVolume;
		SFXVolume.Value = DefaultSFXVolume;
		Mute.Value = DefaultMute;
		
		Fullscreen.Value = DefaultFullscreen;
		ResolutionIndex.Value = DefaultResolutionIndex;
		AntiAliasingIndex.Value = DefaultAntiAliasingIndex;
		CameraShakeIndex.Value = DefaultCameraShakeIndex;
		
		GD.Print("✓ All settings reset to defaults");
	}
	
	public override void _ExitTree()
	{
		CompositeDisposable_Disposables.Dispose();
	}
}
