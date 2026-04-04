using Godot;
using R3;

/// <summary>
/// GameSettings 场景控制器 - 兼容 Component Helper 系统
/// 使用 R3 ReactiveProperty 作为中央状态管理
/// </summary>
public partial class GameSettingsController : Control
{
	// ===== Audio Component Helpers =====
	[Export] public SliderComponentHelper MasterVolume { get; set; }
	[Export] public SliderComponentHelper MusicVolume { get; set; }
	[Export] public SliderComponentHelper SFXVolume { get; set; }
	
	// ===== Video Component Helpers =====
	[Export] public DropdownComponentHelper Resolution { get; set; }
	[Export] public ToggleComponentHelper Fullscreen { get; set; }
	
	// ===== Buttons =====
	[Export] public Button SaveButton { get; set; }
	[Export] public Button CancelButton { get; set; }
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
		// 创建设置管理器
		_settingsManager = new SettingsManager();
		AddChild(_settingsManager);
		
		// 获取音频总线索引
		_masterBusIdx = AudioServer.GetBusIndex("Master");
		_musicBusIdx = AudioServer.GetBusIndex("Music");
		_sfxBusIdx = AudioServer.GetBusIndex("SFX");
		
		// 延迟绑定，确保 SettingsManager._Ready() 已执行
		CallDeferred(MethodName.BindSettings);
	}
	
	public override void _ExitTree()
	{
		_disposables.Dispose();
	}
	
	/// <summary>
	/// 双向绑定：Manager ↔ Component Helpers（使用 ReactiveProperty）
	/// </summary>
	private void BindSettings()
	{
		// ===== Manager → Component Helpers (使用 ReactiveProperty 双向绑定) =====
		
		if (MasterVolume != null)
		{
			// Manager → Component Helper
			_settingsManager.MasterVolume
				.Subscribe(value =>
				{
					MasterVolume.Value.Value = value;
					float dbVolume = Mathf.LinearToDb(value / 100f);
					AudioServer.SetBusVolumeDb(_masterBusIdx, dbVolume);
				})
				.AddTo(_disposables);
			
			// Component Helper → Manager
			MasterVolume.Value
				.Skip(1) // 跳过初始值
				.Subscribe(value => _settingsManager.MasterVolume.Value = value)
				.AddTo(_disposables);
		}
		
		if (MusicVolume != null)
		{
			_settingsManager.MusicVolume
				.Subscribe(value =>
				{
					MusicVolume.Value.Value = value;
					float dbVolume = Mathf.LinearToDb(value / 100f);
					AudioServer.SetBusVolumeDb(_musicBusIdx, dbVolume);
				})
				.AddTo(_disposables);
			
			MusicVolume.Value
				.Skip(1)
				.Subscribe(value => _settingsManager.MusicVolume.Value = value)
				.AddTo(_disposables);
		}
		
		if (SFXVolume != null)
		{
			_settingsManager.SFXVolume
				.Subscribe(value =>
				{
					SFXVolume.Value.Value = value;
					float dbVolume = Mathf.LinearToDb(value / 100f);
					AudioServer.SetBusVolumeDb(_sfxBusIdx, dbVolume);
				})
				.AddTo(_disposables);
			
			SFXVolume.Value
				.Skip(1)
				.Subscribe(value => _settingsManager.SFXVolume.Value = value)
				.AddTo(_disposables);
		}
		
		if (Fullscreen != null)
		{
			_settingsManager.Fullscreen
				.Subscribe(fullscreen =>
				{
					Fullscreen.IsToggled.Value = fullscreen;
					DisplayServer.WindowSetMode(fullscreen 
						? DisplayServer.WindowMode.Fullscreen 
						: DisplayServer.WindowMode.Windowed);
				})
				.AddTo(_disposables);
			
			Fullscreen.IsToggled
				.Skip(1)
				.Subscribe(fullscreen => _settingsManager.Fullscreen.Value = fullscreen)
				.AddTo(_disposables);
		}
		
		if (Resolution != null)
		{
			_settingsManager.ResolutionIndex
				.Subscribe(index =>
				{
					Resolution.SelectedIndex.Value = index;
					
					// 应用分辨率
					string text = Resolution.GetSelectedText();
					if (!string.IsNullOrEmpty(text))
					{
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
			
			Resolution.SelectedIndex
				.Skip(1)
				.Subscribe(index => _settingsManager.ResolutionIndex.Value = index)
				.AddTo(_disposables);
		}
		
		// ===== Buttons =====
		
		if (SaveButton != null)
		{
			SaveButton.OnPressedAsObservable()
				.Subscribe(_ =>
				{
					GD.Print("Settings saved manually");
					Hide();
				})
				.AddTo(_disposables);
		}
		
		if (CancelButton != null)
		{
			CancelButton.OnPressedAsObservable()
				.Subscribe(_ => Hide())
				.AddTo(_disposables);
		}
		
		if (ResetAllButton != null)
		{
			ResetAllButton.OnPressedAsObservable()
				.Subscribe(_ => _settingsManager.ResetAllSettings())
				.AddTo(_disposables);
		}
		
		GD.Print("GameSettings bound to SettingsManager via R3 ReactiveProperty (bidirectional)");
	}
}
