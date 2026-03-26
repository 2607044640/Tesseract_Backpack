# UI Components Helper 使用指南

## 概述

所有UI组件Helper遵循`MarginContainerHelper`的设计模式：
- 使用`[Tool]`属性，在编辑器中实时预览
- 使用属性setter，修改参数立即更新UI
- 使用`Signal Up`模式，组件发出事件供父节点订阅
- 零硬编码，所有内容通过`[Export]`暴露

## 1. SliderComponentHelper

**用途：** 滑块控件，用于调整数值（如音量、亮度）

**暴露的属性：**
```csharp
LabelText      // 标签文本，默认"滑块名称:"
MinValue       // 最小值，默认0
MaxValue       // 最大值，默认100
Step           // 步进值，默认1
DefaultValue   // 默认值，默认50
TickCount      // 刻度数量，默认11
TicksOnBorders // 边界显示刻度，默认true
```

**事件：**
```csharp
public event Action<float> ValueChanged;      // 滑块值改变时触发
public event Action ResetRequested;            // 点击重置按钮时触发
```

**在SettingsWithPrefabs.tscn中使用：**
```
[node name="MusicVolumeSlider" instance=ExtResource("slider")]
LabelText = "音乐音量:"
MinValue = 0.0
MaxValue = 100.0
DefaultValue = 80.0
```

**C#中订阅事件：**
```csharp
[Export] public SliderComponentHelper MusicVolumeSlider { get; set; }

public override void _Ready()
{
    MusicVolumeSlider.ValueChanged += (value) => {
        float linearVolume = value / 100f;
        float dbVolume = Mathf.LinearToDb(linearVolume);
        AudioServer.SetBusVolumeDb(musicBusIdx, dbVolume);
    };
}

public override void _ExitTree()
{
    // 取消订阅，防止内存泄漏
    MusicVolumeSlider.ValueChanged -= OnMusicVolumeChanged;
}
```

---

## 2. OptionComponentHelper

**用途：** 选项按钮，点击弹出菜单选择（如分辨率、画质）

**暴露的属性：**
```csharp
LabelText      // 标签文本，默认"选项名称:"
Options        // 选项数组，默认["选项1", "选项2", "选项3"]
DefaultIndex   // 默认选中索引，默认0
```

**事件：**
```csharp
public event Action<int, string> OptionSelected;  // 选择选项时触发
public event Action ResetRequested;               // 点击重置按钮时触发
```

**在SettingsWithPrefabs.tscn中使用：**
```
[node name="ResolutionOption" instance=ExtResource("option")]
LabelText = "分辨率:"
Options = PackedStringArray("1920x1080", "2560x1440", "3840x2160")
DefaultIndex = 0
```

**C#中订阅事件：**
```csharp
[Export] public OptionComponentHelper ResolutionOption { get; set; }

public override void _Ready()
{
    ResolutionOption.OptionSelected += (index, text) => {
        GD.Print($"选择了分辨率: {text}");
        ApplyResolution(text);
    };
}

public override void _ExitTree()
{
    ResolutionOption.OptionSelected -= OnResolutionSelected;
}
```

---

## 3. ToggleComponentHelper

**用途：** 开关控件，用于布尔选项（如全屏、垂直同步）

**暴露的属性：**
```csharp
LabelText      // 标签文本，默认"开关名称:"
DefaultState   // 默认状态，默认false
```

**事件：**
```csharp
public event Action<bool> Toggled;    // 开关状态改变时触发
public event Action ResetRequested;   // 点击重置按钮时触发
```

**在SettingsWithPrefabs.tscn中使用：**
```
[node name="FullscreenToggle" instance=ExtResource("toggle")]
LabelText = "全屏模式:"
DefaultState = false
```

**C#中订阅事件：**
```csharp
[Export] public ToggleComponentHelper FullscreenToggle { get; set; }

public override void _Ready()
{
    FullscreenToggle.Toggled += (isOn) => {
        if (isOn)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
    };
}

public override void _ExitTree()
{
    FullscreenToggle.Toggled -= OnFullscreenToggled;
}
```

---

## 4. DropdownComponentHelper

**用途：** 下拉列表控件，使用OptionButton（如语言选择）

**暴露的属性：**
```csharp
LabelText      // 标签文本，默认"下拉菜单:"
Items          // 选项数组，默认["选项1", "选项2", "选项3"]
DefaultIndex   // 默认选中索引，默认0
```

**事件：**
```csharp
ItemSelected(int index, string itemText)  // 选择项目时触发
ResetRequested()                          // 点击重置按钮时触发
```

---

## 完整示例：SettingsWithPrefabs.tscn

```csharp
using Godot;

public partial class SettingsMenu : Control
{
    [Export] public SliderComponentHelper MusicVolumeSlider { get; set; }
    [Export] public SliderComponentHelper SoundVolumeSlider { get; set; }
    [Export] public OptionComponentHelper ResolutionOption { get; set; }
    [Export] public ToggleComponentHelper FullscreenToggle { get; set; }
    [Export] public ToggleComponentHelper VSyncToggle { get; set; }
    
    private int _musicBusIdx;
    private int _sfxBusIdx;
    
    public override void _Ready()
    {
        _musicBusIdx = AudioServer.GetBusIndex("Music");
        _sfxBusIdx = AudioServer.GetBusIndex("SFX");
        
        // 订阅音量滑块
        MusicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
        SoundVolumeSlider.ValueChanged += OnSoundVolumeChanged;
        
        // 订阅分辨率选项
        ResolutionOption.OptionSelected += OnResolutionSelected;
        
        // 订阅开关
        FullscreenToggle.Toggled += OnFullscreenToggled;
        VSyncToggle.Toggled += OnVSyncToggled;
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        float dbVolume = Mathf.LinearToDb(value / 100f);
        AudioServer.SetBusVolumeDb(_musicBusIdx, dbVolume);
    }
    
    private void OnSoundVolumeChanged(float value)
    {
        float dbVolume = Mathf.LinearToDb(value / 100f);
        AudioServer.SetBusVolumeDb(_sfxBusIdx, dbVolume);
    }
    
    private void OnResolutionSelected(int index, string resolution)
    {
        // 解析分辨率字符串并应用
        string[] parts = resolution.Split('x');
        if (parts.Length == 2)
        {
            int width = int.Parse(parts[0]);
            int height = int.Parse(parts[1]);
            DisplayServer.WindowSetSize(new Vector2I(width, height));
        }
    }
    
    private void OnFullscreenToggled(bool isOn)
    {
        DisplayServer.WindowSetMode(isOn ? 
            DisplayServer.WindowMode.Fullscreen : 
            DisplayServer.WindowMode.Windowed);
    }
    
    private void OnVSyncToggled(bool isOn)
    {
        DisplayServer.WindowSetVsyncMode(isOn ? 
            DisplayServer.VSyncMode.Enabled : 
            DisplayServer.VSyncMode.Disabled);
    }
    
    public override void _ExitTree()
    {
        // 取消订阅，防止内存泄漏
        MusicVolumeSlider.ValueChanged -= OnMusicVolumeChanged;
        SoundVolumeSlider.ValueChanged -= OnSoundVolumeChanged;
        ResolutionOption.OptionSelected -= OnResolutionSelected;
        FullscreenToggle.Toggled -= OnFullscreenToggled;
        VSyncToggle.Toggled -= OnVSyncToggled;
    }
}
```

## 关键优势

1. **编辑器实时预览**：修改属性立即看到效果
2. **零硬编码**：所有内容通过Inspector配置
3. **类型安全**：C#编译时检查
4. **事件驱动**：解耦的Signal Up模式
5. **易于维护**：逻辑集中在Helper类中
