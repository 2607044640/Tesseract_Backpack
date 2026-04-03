using Godot;
using System;
using R3;

/// <summary>
/// Settings menu controller that manages audio and video settings
/// Refactored with R3 (Reactive Extensions) for automatic event disposal
/// </summary>
public partial class SettingsMenu : Control
{
	// Audio components
	[Export] public SliderComponentHelper MasterVolume { get; set; }
	[Export] public SliderComponentHelper MusicVolume { get; set; }
	[Export] public SliderComponentHelper SFXVolume { get; set; }
	[Export] public ToggleComponentHelper Mute { get; set; }
	
	// Video components
	[Export] public ToggleComponentHelper Fullscreen { get; set; }
	[Export] public DropdownComponentHelper Resolution { get; set; }
	[Export] public DropdownComponentHelper AntiAliasing { get; set; }
	[Export] public DropdownComponentHelper CameraShake { get; set; }
	
	// Back button
	[Export] public Button BackButton { get; set; }
	
	// Settings manager
	private SettingsManager _settingsManager;
	
	// Audio bus indices
	private int _masterBusIdx;
	private int _musicBusIdx;
	private int _sfxBusIdx;
	
	// R3: Single disposal container for ALL subscriptions (replaces 50+ lines of manual unsubscribe)
	private readonly CompositeDisposable _disposables = new();
	
	public override void _Ready()
	{
		// 创建设置管理器
		_settingsManager = new SettingsManager();
		AddChild(_settingsManager);
		
		// 配置SettingKey（用于保存/加载）
		if (MasterVolume != null) MasterVolume.SettingKey = "master_volume";
		if (MusicVolume != null) MusicVolume.SettingKey = "music_volume";
		if (SFXVolume != null) SFXVolume.SettingKey = "sfx_volume";
		if (Mute != null) Mute.SettingKey = "mute";
		if (Fullscreen != null) Fullscreen.SettingKey = "fullscreen";
		if (Resolution != null) Resolution.SettingKey = "resolution";
		if (AntiAliasing != null) AntiAliasing.SettingKey = "anti_aliasing";
		if (CameraShake != null) CameraShake.SettingKey = "camera_shake";
		
		// 注册所有组件到设置管理器
		_settingsManager.RegisterComponents(
			MasterVolume, MusicVolume, SFXVolume, Mute,
			Fullscreen, Resolution, AntiAliasing, CameraShake
		);
		
		// 延迟加载设置，确保所有子节点的_Ready()都已执行
		CallDeferred(MethodName.LoadSettingsDeferred);
		
		// Get audio bus indices
		_masterBusIdx = AudioServer.GetBusIndex("Master");
		_musicBusIdx = AudioServer.GetBusIndex("Music");
		_sfxBusIdx = AudioServer.GetBusIndex("SFX");
		
		// R3: Subscribe to all events with automatic disposal
		SubscribeToEvents();
		
		// Load current settings
		LoadSettings();
	}
	
	public override void _ExitTree()
	{
		// R3: ONE LINE replaces 50+ lines of manual unsubscribe - zero memory leak risk!
		_disposables.Dispose();
	}
	
	/// <summary>
	/// Subscribe to all component events using R3 for automatic disposal
	/// </summary>
	private void SubscribeToEvents()
	{
		// === Audio Events ===
		
		if (MasterVolume != null)
		{
			Observable.FromEvent<float>(
				h => MasterVolume.ValueChanged += h,
				h => MasterVolume.ValueChanged -= h
			)
			.Subscribe(value =>
			{
				float dbVolume = Mathf.LinearToDb(value / 100f);
				AudioServer.SetBusVolumeDb(_masterBusIdx, dbVolume);
				_settingsManager.SaveSettings();
				GD.Print($"Master volume: {value}% ({dbVolume:F2} dB)");
			})
			.AddTo(_disposables);
			
			Observable.FromEvent(
				h => MasterVolume.ResetRequested += h,
				h => MasterVolume.ResetRequested -= h
			)
			.Subscribe(_ => GD.Print("Master volume reset requested"))
			.AddTo(_disposables);
		}
		
		if (MusicVolume != null)
		{
			Observable.FromEvent<float>(
				h => MusicVolume.ValueChanged += h,
				h => MusicVolume.ValueChanged -= h
			)
			.Subscribe(value =>
			{
				float dbVolume = Mathf.LinearToDb(value / 100f);
				AudioServer.SetBusVolumeDb(_musicBusIdx, dbVolume);
				_settingsManager.SaveSettings();
				GD.Print($"Music volume: {value}% ({dbVolume:F2} dB)");
			})
			.AddTo(_disposables);
			
			Observable.FromEvent(
				h => MusicVolume.ResetRequested += h,
				h => MusicVolume.ResetRequested -= h
			)
			.Subscribe(_ => GD.Print("Music volume reset requested"))
			.AddTo(_disposables);
		}
		
		if (SFXVolume != null)
		{
			Observable.FromEvent<float>(
				h => SFXVolume.ValueChanged += h,
				h => SFXVolume.ValueChanged -= h
			)
			.Subscribe(value =>
			{
				float dbVolume = Mathf.LinearToDb(value / 100f);
				AudioServer.SetBusVolumeDb(_sfxBusIdx, dbVolume);
				_settingsManager.SaveSettings();
				GD.Print($"SFX volume: {value}% ({dbVolume:F2} dB)");
			})
			.AddTo(_disposables);
			
			Observable.FromEvent(
				h => SFXVolume.ResetRequested += h,
				h => SFXVolume.ResetRequested -= h
			)
			.Subscribe(_ => GD.Print("SFX volume reset requested"))
			.AddTo(_disposables);
		}
		
		if (Mute != null)
		{
			Observable.FromEvent<bool>(
				h => Mute.Toggled += h,
				h => Mute.Toggled -= h
			)
			.Subscribe(muted =>
			{
				AudioServer.SetBusMute(_masterBusIdx, muted);
				_settingsManager.SaveSettings();
				GD.Print($"Mute: {muted}");
			})
			.AddTo(_disposables);
			
			Observable.FromEvent(
				h => Mute.ResetRequested += h,
				h => Mute.ResetRequested -= h
			)
			.Subscribe(_ => GD.Print("Mute reset requested"))
			.AddTo(_disposables);
		}
		
		// === Video Events ===
		
		if (Fullscreen != null)
		{
			Observable.FromEvent<bool>(
				h => Fullscreen.Toggled += h,
				h => Fullscreen.Toggled -= h
			)
			.Subscribe(fullscreen =>
			{
				if (fullscreen)
				{
					DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
				}
				else
				{
					DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
				}
				_settingsManager.SaveSettings();
				GD.Print($"Fullscreen: {fullscreen}");
			})
			.AddTo(_disposables);
			
			Observable.FromEvent(
				h => Fullscreen.ResetRequested += h,
				h => Fullscreen.ResetRequested -= h
			)
			.Subscribe(_ => GD.Print("Fullscreen reset requested"))
			.AddTo(_disposables);
		}
		
		if (Resolution != null)
		{
			Observable.FromEvent<Action<int, string>, (int, string)>(
				conversion: h => (index, text) => h((index, text)),
				addHandler: h => Resolution.ItemSelected += h,
				removeHandler: h => Resolution.ItemSelected -= h
			)
			.Subscribe(x =>
			{
				var (index, text) = x;
				string[] parts = text.Split('x');
				if (parts.Length == 2)
				{
					int width = int.Parse(parts[0].Trim());
					int height = int.Parse(parts[1].Trim());
					DisplayServer.WindowSetSize(new Vector2I(width, height));
					_settingsManager.SaveSettings();
					GD.Print($"Resolution: {width}x{height}");
				}
			})
			.AddTo(_disposables);
			
			Observable.FromEvent(
				h => Resolution.ResetRequested += h,
				h => Resolution.ResetRequested -= h
			)
			.Subscribe(_ => GD.Print("Resolution reset requested"))
			.AddTo(_disposables);
		}
		
		if (AntiAliasing != null)
		{
			Observable.FromEvent<Action<int, string>, (int, string)>(
				conversion: h => (index, text) => h((index, text)),
				addHandler: h => AntiAliasing.ItemSelected += h,
				removeHandler: h => AntiAliasing.ItemSelected -= h
			)
			.Subscribe(x =>
			{
				var (index, text) = x;
				GD.Print($"Anti-Aliasing: {text} (index: {index})");
				
				switch (index)
				{
					case 0: // Disabled
						GetViewport().Msaa3D = Viewport.Msaa.Disabled;
						GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Disabled;
						break;
					case 1: // FXAA
						GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Fxaa;
						break;
					case 2: // MSAA 2x
						GetViewport().Msaa3D = Viewport.Msaa.Msaa2X;
						break;
					case 3: // MSAA 4x
						GetViewport().Msaa3D = Viewport.Msaa.Msaa4X;
						break;
					case 4: // MSAA 8x
						GetViewport().Msaa3D = Viewport.Msaa.Msaa8X;
						break;
				}
				
				_settingsManager.SaveSettings();
			})
			.AddTo(_disposables);
			
			Observable.FromEvent(
				h => AntiAliasing.ResetRequested += h,
				h => AntiAliasing.ResetRequested -= h
			)
			.Subscribe(_ => GD.Print("Anti-Aliasing reset requested"))
			.AddTo(_disposables);
		}
		
		if (CameraShake != null)
		{
			Observable.FromEvent<Action<int, string>, (int, string)>(
				conversion: h => (index, text) => h((index, text)),
				addHandler: h => CameraShake.ItemSelected += h,
				removeHandler: h => CameraShake.ItemSelected -= h
			)
			.Subscribe(x =>
			{
				var (index, text) = x;
				_settingsManager.SaveSettings();
				GD.Print($"Camera Shake: {text} (index: {index})");
			})
			.AddTo(_disposables);
			
			Observable.FromEvent(
				h => CameraShake.ResetRequested += h,
				h => CameraShake.ResetRequested -= h
			)
			.Subscribe(_ => GD.Print("Camera Shake reset requested"))
			.AddTo(_disposables);
		}
		
		// === Back Button ===
		
		if (BackButton != null)
		{
			Observable.FromEvent(
				h => BackButton.Pressed += h,
				h => BackButton.Pressed -= h
			)
			.Subscribe(_ =>
			{
				GD.Print("Back button pressed");
				Hide();
			})
			.AddTo(_disposables);
		}
	}
	
	// === Settings Management ===
	
	private void LoadSettingsDeferred()
	{
		// 此时所有子节点的_Ready()都已执行完毕
		_settingsManager.LoadSettings();
		GD.Print("Settings loaded (deferred)");
	}
	
	private void LoadSettings()
	{
		// Settings are loaded by SettingsManager in LoadSettingsDeferred()
		GD.Print("Settings loaded via SettingsManager");
	}
	
	private void SaveSettings()
	{
		// Settings are saved automatically by SettingsManager
		GD.Print("Settings saved via SettingsManager");
	}
}
