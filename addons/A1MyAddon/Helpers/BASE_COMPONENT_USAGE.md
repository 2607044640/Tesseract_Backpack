# BaseSettingComponentHelper 使用指南

## 概述

`BaseSettingComponentHelper` 是所有设置UI组件的基类，提供统一的：
- LabelText属性管理
- Reset功能
- 保存/加载功能（通过ConfigFile）

## 继承结构

```
BaseSettingComponentHelper (抽象基类)
├── SliderComponentHelper (滑块)
├── ToggleComponentHelper (开关)
├── DropdownComponentHelper (下拉菜单)
└── OptionComponentHelper (弹出菜单)
```

## 基类功能

### 1. 共同属性

```csharp
[Export] public string LabelText { get; set; }  // 标签文本
[Export] public string SettingKey { get; set; }  // 保存/加载的键名
```

### 2. 共同事件

```csharp
public event Action ResetRequested;  // Reset按钮点击时触发
```

### 3. 抽象方法（子类必须实现）

```csharp
protected abstract void InitializeSpecificNodes();  // 初始化特定控件节点
protected abstract void ConnectSignals();           // 连接特定控件信号
protected abstract void UpdateControl();            // 更新控件状态
protected abstract void DisconnectSignals();        // 断开信号连接

public abstract void ResetToDefault();              // 重置到默认值
public abstract Variant GetSettingValue();          // 获取当前值（用于保存）
public abstract void SetSettingValue(Variant value); // 设置值（用于加载）
```

## 使用示例

### 1. 在场景中配置SettingKey

```gdscript
# SettingsMenu.tscn
[node name="MasterVolume" instance=ExtResource("slider")]
LabelText = "Master :"
SettingKey = "master_volume"  # 用于保存/加载
MinValue = 0.0
MaxValue = 100.0
DefaultValue = 100.0
```

### 2. 使用SettingsManager统一管理

```csharp
public partial class SettingsMenu : Control
{
    [Export] public SliderComponentHelper MasterVolume { get; set; }
    [Export] public ToggleComponentHelper Fullscreen { get; set; }
    
    private SettingsManager _settingsManager;
    
    public override void _Ready()
    {
        // 创建设置管理器
        _settingsManager = new SettingsManager();
        AddChild(_settingsManager);
        
        // 配置SettingKey
        MasterVolume.SettingKey = "master_volume";
        Fullscreen.SettingKey = "fullscreen";
        
        // 注册组件
        _settingsManager.RegisterComponents(MasterVolume, Fullscreen);
        
        // 加载保存的设置
        _settingsManager.LoadSettings();
        
        // 订阅事件
        MasterVolume.ValueChanged += OnVolumeChanged;
    }
    
    private void OnVolumeChanged(float value)
    {
        // 应用设置
        AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(value / 100f));
        
        // 自动保存
        _settingsManager.SaveSettings();
    }
}
```

### 3. 手动保存/加载（不使用SettingsManager）

```csharp
// 保存
var config = new ConfigFile();
MasterVolume.SaveSetting(config, "Settings");
MusicVolume.SaveSetting(config, "Settings");
config.Save("user://settings.cfg");

// 加载
var config = new ConfigFile();
config.Load("user://settings.cfg");
MasterVolume.LoadSetting(config, "Settings");
MusicVolume.LoadSetting(config, "Settings");
```

## SettingsManager API

### 注册组件

```csharp
// 单个注册
_settingsManager.RegisterComponent(MasterVolume);

// 批量注册
_settingsManager.RegisterComponents(
    MasterVolume, MusicVolume, SFXVolume, Mute
);
```

### 保存/加载

```csharp
// 加载所有设置
_settingsManager.LoadSettings();

// 保存所有设置
_settingsManager.SaveSettings();

// 重置所有设置到默认值
_settingsManager.ResetAllSettings();
```

### 直接访问ConfigFile

```csharp
// 获取特定设置
Variant value = _settingsManager.GetSetting("master_volume", 100.0f);

// 设置特定值
_settingsManager.SetSetting("master_volume", 80.0f);
```

## 配置文件格式

保存的配置文件位于 `user://settings.cfg`，格式如下：

```ini
[Settings]
master_volume=100.0
music_volume=80.0
sfx_volume=80.0
mute=false
fullscreen=false
resolution=2
anti_aliasing=0
camera_shake=2
```

## 自动保存时机

SettingsManager会在以下时机自动保存：
1. 用户点击Reset按钮时
2. 场景退出时（_ExitTree）
3. 手动调用SaveSettings()时

## 创建自定义设置组件

```csharp
[Tool]
[GlobalClass]
public partial class MyCustomComponentHelper : BaseSettingComponentHelper
{
    // 1. 定义特定属性
    private float _customValue = 0f;
    [Export]
    public float CustomValue
    {
        get => _customValue;
        set
        {
            _customValue = value;
            UpdateControl();  // 调用基类方法
        }
    }
    
    // 2. 定义特定事件
    public event Action<float> CustomValueChanged;
    
    // 3. 定义内部引用
    private HSlider _customSlider;
    
    // 4. 实现抽象方法
    protected override void InitializeSpecificNodes()
    {
        _customSlider = GetNodeOrNull<HSlider>("CustomSlider");
    }
    
    protected override void ConnectSignals()
    {
        if (_customSlider != null)
            _customSlider.ValueChanged += OnSliderChanged;
    }
    
    protected override void UpdateControl()
    {
        if (_customSlider != null)
            _customSlider.Value = CustomValue;
    }
    
    protected override void DisconnectSignals()
    {
        if (_customSlider != null)
            _customSlider.ValueChanged -= OnSliderChanged;
    }
    
    public override void ResetToDefault()
    {
        if (_customSlider != null)
            _customSlider.Value = CustomValue;
    }
    
    public override Variant GetSettingValue()
    {
        return _customSlider?.Value ?? CustomValue;
    }
    
    public override void SetSettingValue(Variant value)
    {
        if (_customSlider != null)
            _customSlider.Value = (float)value;
    }
    
    private void OnSliderChanged(double value)
    {
        CustomValueChanged?.Invoke((float)value);
    }
}
```

## 注意事项

1. **SettingKey必须唯一**：每个组件的SettingKey必须在整个设置系统中唯一
2. **在_Ready中配置**：SettingKey应该在_Ready()中设置，在LoadSettings()之前
3. **自动保存**：SettingsManager会在退出时自动保存，无需手动调用
4. **类型转换**：GetSettingValue()和SetSettingValue()使用Variant类型，子类需要正确转换
5. **Reset事件**：点击Reset按钮会触发ResetRequested事件并自动保存

## 优势

1. **统一管理**：所有设置组件共享相同的基类和接口
2. **自动保存**：通过SettingsManager自动处理保存/加载
3. **类型安全**：每个子类定义自己的值类型和事件
4. **易于扩展**：添加新的设置组件只需继承基类
5. **零重复代码**：LabelText、Reset、保存/加载逻辑在基类中统一实现
