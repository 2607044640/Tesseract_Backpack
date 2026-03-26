using Godot;
using System;

/// <summary>
/// Settings menu controller that manages audio and video settings
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
		
		// Subscribe to audio events
		if (MasterVolume != null)
		{
			MasterVolume.ValueChanged += OnMasterVolumeChanged;
			MasterVolume.ResetRequested += OnMasterVolumeReset;
		}
		
		if (MusicVolume != null)
		{
			MusicVolume.ValueChanged += OnMusicVolumeChanged;
			MusicVolume.ResetRequested += OnMusicVolumeReset;
		}
		
		if (SFXVolume != null)
		{
			SFXVolume.ValueChanged += OnSFXVolumeChanged;
			SFXVolume.ResetRequested += OnSFXVolumeReset;
		}
		
		if (Mute != null)
		{
			Mute.Toggled += OnMuteToggled;
			Mute.ResetRequested += OnMuteReset;
		}
		
		// Subscribe to video events
		if (Fullscreen != null)
		{
			Fullscreen.Toggled += OnFullscreenToggled;
			Fullscreen.ResetRequested += OnFullscreenReset;
		}
		
		if (Resolution != null)
		{
			Resolution.ItemSelected += OnResolutionSelected;
			Resolution.ResetRequested += OnResolutionReset;
		}
		
		if (AntiAliasing != null)
		{
			AntiAliasing.ItemSelected += OnAntiAliasingSelected;
			AntiAliasing.ResetRequested += OnAntiAliasingReset;
		}
		
		if (CameraShake != null)
		{
			CameraShake.ItemSelected += OnCameraShakeSelected;
			CameraShake.ResetRequested += OnCameraShakeReset;
		}
		
		// Subscribe to back button
		if (BackButton != null)
		{
			BackButton.Pressed += OnBackPressed;
		}
		
		// Load current settings
		LoadSettings();
	}
	
	public override void _ExitTree()
	{
		// Unsubscribe from audio events
		if (MasterVolume != null)
		{
			MasterVolume.ValueChanged -= OnMasterVolumeChanged;
			MasterVolume.ResetRequested -= OnMasterVolumeReset;
		}
		
		if (MusicVolume != null)
		{
			MusicVolume.ValueChanged -= OnMusicVolumeChanged;
			MusicVolume.ResetRequested -= OnMusicVolumeReset;
		}
		
		if (SFXVolume != null)
		{
			SFXVolume.ValueChanged -= OnSFXVolumeChanged;
			SFXVolume.ResetRequested -= OnSFXVolumeReset;
		}
		
		if (Mute != null)
		{
			Mute.Toggled -= OnMuteToggled;
			Mute.ResetRequested -= OnMuteReset;
		}
		
		// Unsubscribe from video events
		if (Fullscreen != null)
		{
			Fullscreen.Toggled -= OnFullscreenToggled;
			Fullscreen.ResetRequested -= OnFullscreenReset;
		}
		
		if (Resolution != null)
		{
			Resolution.ItemSelected -= OnResolutionSelected;
			Resolution.ResetRequested -= OnResolutionReset;
		}
		
		if (AntiAliasing != null)
		{
			AntiAliasing.ItemSelected -= OnAntiAliasingSelected;
			AntiAliasing.ResetRequested -= OnAntiAliasingReset;
		}
		
		if (CameraShake != null)
		{
			CameraShake.ItemSelected -= OnCameraShakeSelected;
			CameraShake.ResetRequested -= OnCameraShakeReset;
		}
		
		// Unsubscribe from back button
		if (BackButton != null)
		{
			BackButton.Pressed -= OnBackPressed;
		}
	}
	
	// === Audio Handlers ===
	
	private void OnMasterVolumeChanged(float value)
	{
		float dbVolume = Mathf.LinearToDb(value / 100f);
		AudioServer.SetBusVolumeDb(_masterBusIdx, dbVolume);
		_settingsManager.SaveSettings();
		GD.Print($"Master volume: {value}% ({dbVolume:F2} dB)");
	}
	
	private void OnMasterVolumeReset()
	{
		GD.Print("Master volume reset requested");
	}
	
	private void OnMusicVolumeChanged(float value)
	{
		float dbVolume = Mathf.LinearToDb(value / 100f);
		AudioServer.SetBusVolumeDb(_musicBusIdx, dbVolume);
		_settingsManager.SaveSettings();
		GD.Print($"Music volume: {value}% ({dbVolume:F2} dB)");
	}
	
	private void OnMusicVolumeReset()
	{
		GD.Print("Music volume reset requested");
	}
	
	private void OnSFXVolumeChanged(float value)
	{
		float dbVolume = Mathf.LinearToDb(value / 100f);
		AudioServer.SetBusVolumeDb(_sfxBusIdx, dbVolume);
		_settingsManager.SaveSettings();
		GD.Print($"SFX volume: {value}% ({dbVolume:F2} dB)");
	}
	
	private void OnSFXVolumeReset()
	{
		GD.Print("SFX volume reset requested");
	}
	
	private void OnMuteToggled(bool muted)
	{
		AudioServer.SetBusMute(_masterBusIdx, muted);
		_settingsManager.SaveSettings();
		GD.Print($"Mute: {muted}");
	}
	
	private void OnMuteReset()
	{
		GD.Print("Mute reset requested");
	}
	
	// === Video Handlers ===
	
	private void OnFullscreenToggled(bool fullscreen)
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
	}
	
	private void OnFullscreenReset()
	{
		GD.Print("Fullscreen reset requested");
	}
	
	private void OnResolutionSelected(int index, string text)
	{
		// Parse resolution string (e.g., "1920 x 1080")
		string[] parts = text.Split('x');
		if (parts.Length == 2)
		{
			int width = int.Parse(parts[0].Trim());
			int height = int.Parse(parts[1].Trim());
			DisplayServer.WindowSetSize(new Vector2I(width, height));
			_settingsManager.SaveSettings();
			GD.Print($"Resolution: {width}x{height}");
		}
	}
	
	private void OnResolutionReset()
	{
		GD.Print("Resolution reset requested");
	}
	
	private void OnAntiAliasingSelected(int index, string text)
	{
		// Apply anti-aliasing settings
		// Note: This is a simplified example. Actual implementation depends on your rendering setup
		GD.Print($"Anti-Aliasing: {text} (index: {index})");
		
		// Example: Set MSAA level
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
	}
	
	private void OnAntiAliasingReset()
	{
		GD.Print("Anti-Aliasing reset requested");
	}
	
	private void OnCameraShakeSelected(int index, string text)
	{
		// Store camera shake intensity
		// This would typically be saved to a settings file or autoload
		_settingsManager.SaveSettings();
		GD.Print($"Camera Shake: {text} (index: {index})");
	}
	
	private void OnCameraShakeReset()
	{
		GD.Print("Camera Shake reset requested");
	}
	
	// === Navigation ===
	
	private void OnBackPressed()
	{
		GD.Print("Back button pressed");
		// Hide settings menu or return to main menu
		Hide();
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
