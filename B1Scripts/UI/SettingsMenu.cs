using Godot;
using R3;

/// <summary>
/// Settings menu controller - 完全使用 R3 ReactiveProperty 重构
/// SettingsManager 是唯一的状态源，UI 自动双向绑定
/// 无需手动初始化同步，无需 LoadSettingsDeferred 等 workaround
/// </summary>
public partial class SettingsMenu : Control
{
	// UI Components (仅用于显示，不持有状态)
	[Export] public HSlider MasterVolumeSlider { get; set; }
	[Export] public HSlider MusicVolumeSlider { get; set; }
	[Export] public HSlider SFXVolumeSlider { get; set; }
	[Export] public CheckButton MuteToggle { get; set; }
	
	[Export] public CheckButton FullscreenToggle { get; set; }
	[Export] public OptionButton ResolutionDropdown { get; set; }
	[Export] public OptionButton AntiAliasingDropdown { get; set; }
	[Export] public OptionButton CameraShakeDropdown { get; set; }
	
	[Export] public Button BackButton { get; set; }
	[Export] public Button ResetAllButton { get; set; }
	
	// Settings manager (单一状态源)
	private SettingsManager _settingsManager;
	
	// Audio bus indices
	private int _masterBusIdx;
	private int _musicBusIdx;
	private int _sfxBusIdx;
	
	// R3: 单一 disposal 容器
	private readonly CompositeDisposable _disposables = new();
	
	public override void _Ready()
	{
		// 创建设置管理器（状态在其 _Ready 中自动加载）
		_settingsManager = new SettingsManager();
		AddChild(_settingsManager);
		
		// 获取音频总线索引
		_masterBusIdx = AudioServer.GetBusIndex("Master");
		_musicBusIdx = AudioServer.GetBusIndex("Music");
		_sfxBusIdx = AudioServer.GetBusIndex("SFX");
		
		// 延迟订阅，确保 SettingsManager._Ready() 已执行
		CallDeferred(MethodName.BindSettingsToUI);
	}
	
	public override void _ExitTree()
	{
		_disposables.Dispose();
	}
	
	/// <summary>
	/// 双向绑定：Manager → UI 和 UI → Manager
	/// </summary>
	private void BindSettingsToUI()
	{
		// ===== Manager → UI (自动同步初始值 + 监听后续变化) =====
		
		// Master Volume: Manager → UI + Audio
		_settingsManager.MasterVolume
			.Subscribe(value =>
			{
				if (MasterVolumeSlider != null)
					MasterVolumeSlider.Value = value;
				
				float dbVolume = Mathf.LinearToDb(value / 100f);
				AudioServer.SetBusVolumeDb(_masterBusIdx, dbVolume);
			})
			.AddTo(_disposables);
		
		// Music Volume: Manager → UI + Audio
		_settingsManager.MusicVolume
			.Subscribe(value =>
			{
				if (MusicVolumeSlider != null)
					MusicVolumeSlider.Value = value;
				
				float dbVolume = Mathf.LinearToDb(value / 100f);
				AudioServer.SetBusVolumeDb(_musicBusIdx, dbVolume);
			})
			.AddTo(_disposables);
		
		// SFX Volume: Manager → UI + Audio
		_settingsManager.SFXVolume
			.Subscribe(value =>
			{
				if (SFXVolumeSlider != null)
					SFXVolumeSlider.Value = value;
				
				float dbVolume = Mathf.LinearToDb(value / 100f);
				AudioServer.SetBusVolumeDb(_sfxBusIdx, dbVolume);
			})
			.AddTo(_disposables);
		
		// Mute: Manager → UI + Audio
		_settingsManager.Mute
			.Subscribe(muted =>
			{
				if (MuteToggle != null)
					MuteToggle.ButtonPressed = muted;
				
				AudioServer.SetBusMute(_masterBusIdx, muted);
			})
			.AddTo(_disposables);
		
		// Fullscreen: Manager → UI + Display
		_settingsManager.Fullscreen
			.Subscribe(fullscreen =>
			{
				if (FullscreenToggle != null)
					FullscreenToggle.ButtonPressed = fullscreen;
				
				DisplayServer.WindowSetMode(fullscreen 
					? DisplayServer.WindowMode.Fullscreen 
					: DisplayServer.WindowMode.Windowed);
			})
			.AddTo(_disposables);
		
		// Resolution: Manager → UI + Display
		_settingsManager.ResolutionIndex
			.Subscribe(index =>
			{
				if (ResolutionDropdown != null)
				{
					ResolutionDropdown.Selected = index;
					
					// 应用分辨率
					string text = ResolutionDropdown.GetItemText(index);
					string[] parts = text.Split('x');
					if (parts.Length == 2)
					{
						int width = int.Parse(parts[0].Trim());
						int height = int.Parse(parts[1].Trim());
						DisplayServer.WindowSetSize(new Vector2I(width, height));
					}
				}
			})
			.AddTo(_disposables);
		
		// Anti-Aliasing: Manager → UI + Viewport
		_settingsManager.AntiAliasingIndex
			.Subscribe(index =>
			{
				if (AntiAliasingDropdown != null)
					AntiAliasingDropdown.Selected = index;
				
				ApplyAntiAliasing(index);
			})
			.AddTo(_disposables);
		
		// Camera Shake: Manager → UI
		_settingsManager.CameraShakeIndex
			.Subscribe(index =>
			{
				if (CameraShakeDropdown != null)
					CameraShakeDropdown.Selected = index;
			})
			.AddTo(_disposables);
		
		// ===== UI → Manager (用户交互更新状态) =====
		
		if (MasterVolumeSlider != null)
		{
			MasterVolumeSlider.OnValueChangedAsObservable()
				.Subscribe(value => _settingsManager.MasterVolume.Value = (float)value)
				.AddTo(_disposables);
		}
		
		if (MusicVolumeSlider != null)
		{
			MusicVolumeSlider.OnValueChangedAsObservable()
				.Subscribe(value => _settingsManager.MusicVolume.Value = (float)value)
				.AddTo(_disposables);
		}
		
		if (SFXVolumeSlider != null)
		{
			SFXVolumeSlider.OnValueChangedAsObservable()
				.Subscribe(value => _settingsManager.SFXVolume.Value = (float)value)
				.AddTo(_disposables);
		}
		
		if (MuteToggle != null)
		{
			MuteToggle.OnToggledAsObservable()
				.Subscribe(muted => _settingsManager.Mute.Value = muted)
				.AddTo(_disposables);
		}
		
		if (FullscreenToggle != null)
		{
			FullscreenToggle.OnToggledAsObservable()
				.Subscribe(fullscreen => _settingsManager.Fullscreen.Value = fullscreen)
				.AddTo(_disposables);
		}
		
		if (ResolutionDropdown != null)
		{
			ResolutionDropdown.OnItemSelectedAsObservable()
				.Subscribe(index => _settingsManager.ResolutionIndex.Value = (int)index)
				.AddTo(_disposables);
		}
		
		if (AntiAliasingDropdown != null)
		{
			AntiAliasingDropdown.OnItemSelectedAsObservable()
				.Subscribe(index => _settingsManager.AntiAliasingIndex.Value = (int)index)
				.AddTo(_disposables);
		}
		
		if (CameraShakeDropdown != null)
		{
			CameraShakeDropdown.OnItemSelectedAsObservable()
				.Subscribe(index => _settingsManager.CameraShakeIndex.Value = (int)index)
				.AddTo(_disposables);
		}
		
		// ===== Buttons =====
		
		if (BackButton != null)
		{
			BackButton.OnPressedAsObservable()
				.Subscribe(_ => Hide())
				.AddTo(_disposables);
		}
		
		if (ResetAllButton != null)
		{
			ResetAllButton.OnPressedAsObservable()
				.Subscribe(_ => _settingsManager.ResetAllSettings())
				.AddTo(_disposables);
		}
		
		GD.Print("Settings UI bound to SettingsManager via R3 ReactiveProperty");
	}
	
	/// <summary>
	/// 应用抗锯齿设置
	/// </summary>
	private void ApplyAntiAliasing(int index)
	{
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
	}
}
