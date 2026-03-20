# Godot UI Builder - 使用指南

## 概述

这是一个专为AI设计的Python工具，用于程序化生成Godot UI，避免直接操作冗长的.tscn文本。

## 核心优势

✅ **清晰的树状图** - 一眼看懂UI结构  
✅ **简洁的API** - 链式调用，代码即文档  
✅ **自动处理细节** - unique_id、uid、parent路径自动生成  
✅ **支持Preset** - 引用外部资源（script、style等）

## 快速开始

### 1. 基础示例

```python
from godot_ui_builder import UIBuilder

# 创建构建器
ui = UIBuilder("MyUI")

# 创建根节点（全屏）
root = ui.create_control("Control", fullscreen=True)

# 添加背景
root.add_color_rect("Background_ColorRect", color=(0.2, 0.2, 0.2, 1))

# 添加垂直布局
vbox = root.add_vbox("VBoxContainer")

# 添加标题
vbox.add_label("Title_Label", text="Hello Godot", align="center", font_size=48)

# 添加按钮
vbox.add_button("Start_Button", text="Start Game", size_flags_h=4)

# 查看结构
print(ui.generate_tree_view())

# 保存
ui.save("A1UIScenes/MyUI.tscn")
```

### 2. 树状图输出

```
Control (Control) [root]
├── Background_ColorRect (ColorRect)
└── VBoxContainer (VBoxContainer)
    ├── Title_Label (Label) Hello Godot
    └── Start_Button (Button) Start Game
```

## API参考

### UIBuilder类

#### `__init__(scene_name, scene_uid=None)`
创建UI构建器

#### `create_control(name="Control", fullscreen=True)`
创建根Control节点
- `fullscreen=True`: 自动设置全屏锚点

#### `generate_tree_view()`
生成树状图（给AI看）

#### `generate_tscn()`
生成.tscn文本

#### `save(output_path)`
保存到文件

### UINode类 - 容器方法

#### `add_margin_container(name, uniform=None, left=None, top=None, right=None, bottom=None, script=None)`
添加MarginContainer
```python
# 统一边距
margin = root.add_margin_container("Margin", uniform=20)

# 单独设置
margin = root.add_margin_container("Margin", left=10, top=20, right=10, bottom=20)

# 使用MarginContainerHelper脚本
margin = root.add_margin_container(
    "Margin", 
    uniform=30,
    script="res://addons/MyAddon/Helpers/MarginContainerHelper.cs"
)
```

#### `add_panel_container(name)`
添加PanelContainer（带背景的容器）

#### `add_vbox(name, separation=None)`
添加VBoxContainer（垂直布局）

#### `add_hbox(name, separation=None)`
添加HBoxContainer（水平布局）

### UINode类 - 控件方法

#### `add_color_rect(name, color=(0.15, 0.15, 0.15, 1), offset=None)`
添加ColorRect（纯色背景）
```python
root.add_color_rect("BG", color=(0.2, 0.2, 0.2, 1))
```

#### `add_label(name, text="", align="left", font_size=None, min_size=None, size_flags_h=None)`
添加Label
```python
# 简单标签
vbox.add_label("Title", text="Hello", align="center")

# 带样式
vbox.add_label("Title", text="Hello", align="center", font_size=48, min_size=(400, 0))
```

**align参数:**
- `"left"` (0) - 左对齐
- `"center"` (1) - 居中
- `"right"` (2) - 右对齐

#### `add_progress_bar(name, value=0, size_flags_h=None, size_flags_v=None, min_size=None)`
添加ProgressBar
```python
hbox.add_progress_bar("HP_Bar", value=75, size_flags_h=3)
```

#### `add_button(name, text="", size_flags_h=None)`
添加Button
```python
vbox.add_button("Start_Button", text="Start", size_flags_h=4)
```

#### `add_separator(name, separation=None, style=None)`
添加HSeparator（分隔线）
```python
vbox.add_separator("Sep", separation=20, style="res://A1UIPresets/new_style_box_line.tres")
```

### 通用方法

#### `set_property(key, value)`
设置任意属性（链式调用）
```python
node.set_property("custom_property", 123).set_property("another", "value")
```

## 关键参数说明

### ProgressBar 重要属性
```python
# 必须设置这些属性才能让ProgressBar正确显示！
progress_bar = hbox.add_progress_bar(
    "HP_Bar",
    value=75,
    size_flags_h=3,              # Fill + Expand（填充并扩展）
    size_flags_v=4,              # Shrink Center（垂直居中）
    size_flags_stretch_ratio=2.94,  # 拉伸比例（控制宽度）
    show_percentage=True         # 显示百分比文字
)
```

**为什么这些参数重要：**
- `size_flags_h=3`: 让进度条填充可用空间
- `size_flags_stretch_ratio`: 控制相对于其他元素的宽度比例
- `show_percentage=True`: 显示"10%"这样的文字，否则只有空条

### MarginContainer 锚点模式
```python
# 当MarginContainer需要填充整个屏幕时，使用锚点模式
margin = root.add_margin_container(
    "Margin",
    uniform=30,
    use_anchors=True  # 使用锚点模式而不是容器模式
)
# 然后设置锚点属性
margin.set_property("anchors_preset", 15)  # 全屏预设
margin.set_property("anchor_right", 1.0)
margin.set_property("anchor_bottom", 1.0)
margin.set_property("grow_horizontal", 2)
margin.set_property("grow_vertical", 2)
```

### Label 对齐
```python
# 居中对齐的Label
label = vbox.add_label("Title", text="Hello", align="center")

# 如果在HBoxContainer中需要居中，还要设置horizontal_alignment
label.set_property("horizontal_alignment", 1)  # 0=左, 1=中, 2=右
```

## Size Flags 参考

常用的`size_flags_horizontal`值：

- `0` - 无标志（保持最小尺寸）
- `3` - Fill + Expand（填充并扩展）
- `4` - Shrink Center（收缩并居中）
- `6` - Shrink Begin + Shrink Center

## 完整示例

参考 `example_rebuild_settings.py` 查看如何重建复杂的SettingsUI。

## 工作流程

1. **创建UIBuilder** - 指定场景名称
2. **构建UI树** - 使用链式API添加节点
3. **查看树状图** - 验证结构是否正确
4. **保存文件** - 生成.tscn文件
5. **在Godot中测试** - 打开场景验证

## 注意事项

- 节点命名遵循 `[用途]_[类型]` 格式（如 `Title_Label`）
- unique_id 自动生成，无需手动指定
- 使用 `set_property()` 设置任何未封装的属性
- 外部资源（script、style）会自动收集并添加到文件头部

## 扩展

如需添加新的节点类型，在 `UINode` 类中添加对应的 `add_xxx()` 方法即可。

## 示例输出

运行 `example_rebuild_settings.py` 会输出：

```
============================================================
UI Structure Tree View:
============================================================
Control (Control) [root]
├── Background_ColorRect (ColorRect)
└── MarginContainerHelper (MarginContainer) [script]
    └── PanelContainer (PanelContainer)
        └── MarginContainerHelper (MarginContainer) [script]
            └── VBoxContainer (VBoxContainer)
                ├── Label (Label) Setting
                ├── HSeparator (HSeparator)
                ├── HBoxContainer (HBoxContainer)
                │   ├── Label2 (Label) text13243
                │   └── ProgressBar (ProgressBar)
                ├── HBoxContainer2 (HBoxContainer)
                │   ├── Label2 (Label) text132
                │   └── ProgressBar (ProgressBar)
                └── Button (Button) SeeMore
============================================================
✅ UI saved to: A1UIScenes/SettingsUI_rebuilt.tscn
```
