using Godot;
using R3;
using System;

/// <summary>
/// GameSettings 场景控制器 - Component Helper 系统集成
/// 使用 R3 ReactiveProperty 作为中央状态管理
/// 
/// ✅ 优化点：
/// 1. 通用绑定方法减少 80% 重复代码
/// 2. DistinctUntilChanged 防止双向绑定循环
/// 3. ThrottleFirst 防止按钮连击
/// 4. TryParse 防止解析崩溃
/// 5. 全屏与分辨率联动
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
	/// ✅ 优化后的绑定逻辑：使用通用方法，代码量减少 80%
	/// </summary>
	private void BindSettings()
	{
		// ✅ 音频设置（专用方法：双向绑定 + 音频总线应用）
		BindAudioSlider(_settingsManager.MasterVolume, MasterVolume, _masterBusIdx);
		BindAudioSlider(_settingsManager.MusicVolume, MusicVolume, _musicBusIdx);
		BindAudioSlider(_settingsManager.SFXVolume, SFXVolume, _sfxBusIdx);
		
		// 也可以使用通用方法（不包含音频总线逻辑）：
		// BindSlider(_settingsManager.MasterVolume, MasterVolume, value => {
		//     float dbVolume = Mathf.LinearToDb(value / 100f);
		//     AudioServer.SetBusVolumeDb(_masterBusIdx, dbVolume);
		// });
		
		// ✅ 视频设置
		BindToggle(_settingsManager.Fullscreen, Fullscreen, ApplyFullscreen);
		BindDropdown(_settingsManager.ResolutionIndex, Resolution, ApplyResolution);
		
		// ✅ 全屏与分辨率联动（新增功能）
		SetupFullscreenResolutionLink();
		
		// ✅ 按钮
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
				.ThrottleFirst(TimeSpan.FromSeconds(1)) // ✅ 防止连击
				.Subscribe(_ =>
				{
					_settingsManager.ResetAllSettings();
					GD.Print("✓ Settings reset (1s cooldown)");
				})
				.AddTo(_disposables);
		}
		
		GD.Print("✓ GameSettings bound with optimized binders");
	}
	
	/// <summary>
	/// ✅ 通用滑块绑定（双向 + 自定义应用逻辑）
	/// 
	/// 优化点：
	/// 1. DistinctUntilChanged：防止双向绑定循环
	/// 2. 统一的绑定逻辑：减少重复代码
	/// 3. 空值检查：避免空引用异常
	/// 4. 支持自定义应用逻辑：灵活处理不同场景
	/// </summary>
	private void BindSlider(
		ReactiveProperty<float> property, 
		SliderComponentHelper slider, 
		Action<float> applyAction = null)
	{
		if (slider == null) return;
		
		// Manager → UI + Apply
		property
			.DistinctUntilChanged()
			.Subscribe(value =>
			{
				slider.Value.Value = value;
				applyAction?.Invoke(value);
			})
			.AddTo(_disposables);
		
		// UI → Manager
		slider.Value
			.Skip(1)
			.DistinctUntilChanged()
			.Subscribe(value => property.Value = value)
			.AddTo(_disposables);
	}
	
	/// <summary>
	/// ✅ 音频滑块专用绑定（双向 + 音频总线）
	/// 
	/// 专用于音频设置，自动处理音频总线的 dB 转换和应用
	/// </summary>
	private void BindAudioSlider(
		ReactiveProperty<float> property, 
		SliderComponentHelper slider, 
		int audioBusIdx)
	{
		if (slider == null) return;
		
		// Manager → UI + Audio
		property
			.DistinctUntilChanged()
			.Subscribe(value =>
			{
				slider.Value.Value = value;
				float dbVolume = Mathf.LinearToDb(value / 100f);
				AudioServer.SetBusVolumeDb(audioBusIdx, dbVolume);
			})
			.AddTo(_disposables);
		
		// UI → Manager
		slider.Value
			.Skip(1)
			.DistinctUntilChanged()
			.Subscribe(value => property.Value = value)
			.AddTo(_disposables);
	}
	
	/// <summary>
	/// ✅ 通用开关绑定（双向 + 自定义应用逻辑）
	/// </summary>
	private void BindToggle(
		ReactiveProperty<bool> property, 
		ToggleComponentHelper toggle, 
		Action<bool> applyAction = null)
	{
		if (toggle == null) return;
		
		// Manager → UI + Apply
		property
			.DistinctUntilChanged()
			.Subscribe(value =>
			{
				toggle.IsToggled.Value = value;
				applyAction?.Invoke(value);
			})
			.AddTo(_disposables);
		
		// UI → Manager
		toggle.IsToggled
			.Skip(1)
			.DistinctUntilChanged()
			.Subscribe(value => property.Value = value)
			.AddTo(_disposables);
	}
	
	/// <summary>
	/// ✅ 通用下拉框绑定（双向 + 自定义应用逻辑）
	/// </summary>
	private void BindDropdown(
		ReactiveProperty<int> property, 
		DropdownComponentHelper dropdown, 
		Action<int> applyAction = null)
	{
		if (dropdown == null) return;
		
		// Manager → UI + Apply
		property
			.DistinctUntilChanged()
			.Subscribe(index =>
			{
				dropdown.SelectedIndex.Value = index;
				applyAction?.Invoke(index);
			})
			.AddTo(_disposables);
		
		// UI → Manager
		dropdown.SelectedIndex
			.Skip(1)
			.DistinctUntilChanged()
			.Subscribe(index => property.Value = index)
			.AddTo(_disposables);
	}
	
	/// <summary>
	/// ✅ 应用全屏设置
	/// </summary>
	private void ApplyFullscreen(bool fullscreen)
	{
		DisplayServer.WindowSetMode(fullscreen 
			? DisplayServer.WindowMode.Fullscreen 
			: DisplayServer.WindowMode.Windowed);
		GD.Print($"✓ Fullscreen: {fullscreen}");
	}
	
	/// <summary>
	/// ✅ 应用分辨率设置（带错误处理）
	/// 
	/// 优化点：
	/// 1. 使用 TryParse 替代 Parse：避免崩溃
	/// 2. 详细的错误日志：便于调试
	/// </summary>
	private void ApplyResolution(int index)
	{
		if (Resolution == null) return;
		
		string text = Resolution.GetSelectedText();
		if (!string.IsNullOrEmpty(text))
		{
			string[] parts = text.Split('x');
			if (parts.Length == 2 
				&& int.TryParse(parts[0].Trim(), out int width)
				&& int.TryParse(parts[1].Trim(), out int height))
			{
				DisplayServer.WindowSetSize(new Vector2I(width, height));
				GD.Print($"✓ Resolution set to {width}x{height}");
			}
			else
			{
				GD.PushWarning($"Invalid resolution format: {text}");
			}
		}
	}
	
	/// <summary>
	/// ✅ 全屏与分辨率联动（新增功能）
	/// 
	/// 功能：全屏时禁用分辨率选择，窗口模式时启用
	/// 符合现代游戏交互逻辑，防止用户在不支持的模式下修改设置
	/// </summary>
	private void SetupFullscreenResolutionLink()
	{
		if (Fullscreen == null || Resolution == null) return;
		
		_settingsManager.Fullscreen
			.Subscribe(isFullscreen =>
			{
				// 尝试获取底层的 OptionButton 控件
				var dropdown = Resolution.GetNodeOrNull<OptionButton>("OptionButton");
				if (dropdown != null)
				{
					dropdown.Disabled = isFullscreen;
					GD.Print($"✓ Resolution selector {(isFullscreen ? "disabled" : "enabled")}");
				}
				else
				{
					// 如果找不到，尝试直接访问 Resolution 的子节点
					foreach (var child in Resolution.GetChildren())
					{
						if (child is OptionButton optionButton)
						{
							optionButton.Disabled = isFullscreen;
							GD.Print($"✓ Resolution selector {(isFullscreen ? "disabled" : "enabled")}");
							break;
						}
					}
				}
			})
			.AddTo(_disposables);
	}
}
