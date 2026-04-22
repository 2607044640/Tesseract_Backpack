using Godot;
using R3;

public partial class SettingsMenu : Control
{
	// UI Components (仅用于显示，不持有状态)
	[Export] public HSlider HSlider_MasterVolume { get; set; }
	[Export] public HSlider HSlider_MusicVolume { get; set; }
	[Export] public HSlider HSlider_SFXVolume { get; set; }
	[Export] public CheckButton CheckButton_Mute { get; set; }
	
	[Export] public CheckButton CheckButton_Fullscreen { get; set; }
	[Export] public OptionButton OptionButton_Resolution { get; set; }
	[Export] public OptionButton OptionButton_AntiAliasing { get; set; }
	[Export] public OptionButton OptionButton_CameraShake { get; set; }
	
	[Export] public Button Button_Back { get; set; }
	[Export] public Button Button_ResetAll { get; set; }
	
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
	
	// 双向绑定设置
	// 目的：实现 Manager ↔ UI 的自动同步，Manager 变化自动更新 UI，UI 交互自动更新 Manager
	// 示例：Manager.MasterVolume 改变 -> Slider 自动更新 + 音频总线音量改变；Slider 拖动 -> Manager.MasterVolume 自动更新
	// 算法：1. 订阅 Manager ReactiveProperty -> 更新 UI + 应用设置 -> 2. 订阅 UI 事件 -> 更新 Manager ReactiveProperty
	private void BindSettingsToUI()
	{
		// ===== Manager → UI (自动同步初始值 + 监听后续变化) =====
		
		// Master Volume: Manager → UI + Audio
		_settingsManager.MasterVolume
			.Subscribe(value =>
			{
				if (HSlider_MasterVolume != null)
					HSlider_MasterVolume.Value = value;
				
				float dbVolume = Mathf.LinearToDb(value / 100f);
				AudioServer.SetBusVolumeDb(_masterBusIdx, dbVolume);
			})
			.AddTo(_disposables);
		
		// Music Volume: Manager → UI + Audio
		_settingsManager.MusicVolume
			.Subscribe(value =>
			{
				if (HSlider_MusicVolume != null)
					HSlider_MusicVolume.Value = value;
				
				float dbVolume = Mathf.LinearToDb(value / 100f);
				AudioServer.SetBusVolumeDb(_musicBusIdx, dbVolume);
			})
			.AddTo(_disposables);
		
		// SFX Volume: Manager → UI + Audio
		_settingsManager.SFXVolume
			.Subscribe(value =>
			{
				if (HSlider_SFXVolume != null)
					HSlider_SFXVolume.Value = value;
				
				float dbVolume = Mathf.LinearToDb(value / 100f);
				AudioServer.SetBusVolumeDb(_sfxBusIdx, dbVolume);
			})
			.AddTo(_disposables);
		
		// Mute: Manager → UI + Audio
		_settingsManager.Mute
			.Subscribe(muted =>
			{
				if (CheckButton_Mute != null)
					CheckButton_Mute.ButtonPressed = muted;
				
				AudioServer.SetBusMute(_masterBusIdx, muted);
			})
			.AddTo(_disposables);
		
		// Fullscreen: Manager → UI + Display
		_settingsManager.Fullscreen
			.Subscribe(fullscreen =>
			{
				if (CheckButton_Fullscreen != null)
					CheckButton_Fullscreen.ButtonPressed = fullscreen;
				
				DisplayServer.WindowSetMode(fullscreen 
					? DisplayServer.WindowMode.Fullscreen 
					: DisplayServer.WindowMode.Windowed);
			})
			.AddTo(_disposables);
		
		// Resolution: Manager → UI + Display
		_settingsManager.ResolutionIndex
			.Subscribe(index =>
			{
				if (OptionButton_Resolution != null)
				{
					OptionButton_Resolution.Selected = index;
					
					// 应用分辨率
					string text = OptionButton_Resolution.GetItemText(index);
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
				if (OptionButton_AntiAliasing != null)
					OptionButton_AntiAliasing.Selected = index;
				
				ApplyAntiAliasing(index);
			})
			.AddTo(_disposables);
		
		// Camera Shake: Manager → UI
		_settingsManager.CameraShakeIndex
			.Subscribe(index =>
			{
				if (OptionButton_CameraShake != null)
					OptionButton_CameraShake.Selected = index;
			})
			.AddTo(_disposables);
		
		// ===== UI → Manager (用户交互更新状态) =====
		
		if (HSlider_MasterVolume != null)
		{
			HSlider_MasterVolume.OnValueChangedAsObservable()
				.Subscribe(value => _settingsManager.MasterVolume.Value = (float)value)
				.AddTo(_disposables);
		}
		
		if (HSlider_MusicVolume != null)
		{
			HSlider_MusicVolume.OnValueChangedAsObservable()
				.Subscribe(value => _settingsManager.MusicVolume.Value = (float)value)
				.AddTo(_disposables);
		}
		
		if (HSlider_SFXVolume != null)
		{
			HSlider_SFXVolume.OnValueChangedAsObservable()
				.Subscribe(value => _settingsManager.SFXVolume.Value = (float)value)
				.AddTo(_disposables);
		}
		
		if (CheckButton_Mute != null)
		{
			CheckButton_Mute.OnToggledAsObservable()
				.Subscribe(muted => _settingsManager.Mute.Value = muted)
				.AddTo(_disposables);
		}
		
		if (CheckButton_Fullscreen != null)
		{
			CheckButton_Fullscreen.OnToggledAsObservable()
				.Subscribe(fullscreen => _settingsManager.Fullscreen.Value = fullscreen)
				.AddTo(_disposables);
		}
		
		if (OptionButton_Resolution != null)
		{
			OptionButton_Resolution.OnItemSelectedAsObservable()
				.Subscribe(index => _settingsManager.ResolutionIndex.Value = (int)index)
				.AddTo(_disposables);
		}
		
		if (OptionButton_AntiAliasing != null)
		{
			OptionButton_AntiAliasing.OnItemSelectedAsObservable()
				.Subscribe(index => _settingsManager.AntiAliasingIndex.Value = (int)index)
				.AddTo(_disposables);
		}
		
		if (OptionButton_CameraShake != null)
		{
			OptionButton_CameraShake.OnItemSelectedAsObservable()
				.Subscribe(index => _settingsManager.CameraShakeIndex.Value = (int)index)
				.AddTo(_disposables);
		}
		
		// ===== Buttons =====
		
		if (Button_Back != null)
		{
			Button_Back.OnPressedAsObservable()
				.Subscribe(_ => Hide())
				.AddTo(_disposables);
		}
		
		if (Button_ResetAll != null)
		{
			Button_ResetAll.OnPressedAsObservable()
				.Subscribe(_ => _settingsManager.ResetAllSettings())
				.AddTo(_disposables);
		}
		
		GD.Print("Settings UI bound to SettingsManager via R3 ReactiveProperty");
	}
	
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
