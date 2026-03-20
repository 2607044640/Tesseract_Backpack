# UIBuilder 工具设计分析

## 目标
创建一个Python工具，让AI能够方便地操作Godot UI，而不是直接面对冗长的.tscn文本。

## 核心需求
1. ✅ 程序化生成UI，使用presets
2. ✅ 清晰的结构展示（树状图）
3. ✅ 简单的API调用方式
4. ✅ 自动处理unique_id和uid

## SettingsUI.tscn 结构分析

### 树状结构
```
Control (Control) [root] uid=282656748
├── Background_ColorRect (ColorRect) uid=476113691
│   └── color: (0.157, 0.157, 0.157, 1)
│   └── offset: (1, 1, 1151, 647)
└── MarginContainerHelper (MarginContainer) uid=1038258113
    └── [script: MarginContainerHelper.cs]
    └── UniformMargin=30
    └── PanelContainer (PanelContainer) uid=347572081
        └── MarginContainerHelper (MarginContainer) uid=1111745785
            └── UniformMargin=33
            └── VBoxContainer (VBoxContainer) uid=464242067
                ├── Label (Label) uid=68242471
                │   └── text="Setting", align=center
                ├── HSeparator (HSeparator) uid=569965811
                │   └── separation=56, style=new_style_box_line.tres
                ├── HBoxContainer (HBoxContainer) uid=572368773
                │   ├── Label2 (Label) uid=871086627
                │   │   └── text="text13243", min_size=(410,0), font_size=41
                │   └── ProgressBar (ProgressBar) uid=649965808
                │       └── value=10.17, size_flags_h=3
                ├── HBoxContainer2 (HBoxContainer) uid=356752561
                │   ├── Label2 (Label) uid=1413068703
                │   │   └── text="text132", min_size=(410,0), font_size=41
                │   └── ProgressBar (ProgressBar) uid=1088605006
                │       └── value=10.17, size_flags_h=3, min_size=(0,8)
                └── Button (Button) uid=1217054756
                    └── text="SeeMore", size_flags_h=4
```

### 关键观察

#### 1. 文件头部结构
```
[gd_scene format=3 uid="uid://cpurmg3xq1hd4"]

[ext_resource type="Script" uid="uid://bk83ics8idr7w" path="res://addons/MyAddon/Helpers/MarginContainerHelper.cs" id="1_dsrpe"]
[ext_resource type="StyleBox" uid="uid://dbfc62yrw0q43" path="res://A1UIPresets/new_style_box_line.tres" id="2_dsrpe"]
```

#### 2. 节点定义格式
```
[node name="NodeName" type="NodeType" parent="ParentPath" unique_id=123456789]
property1 = value1
property2 = value2
```

#### 3. 常用属性模式

**Layout相关:**
- `layout_mode = 2` (Container子节点)
- `layout_mode = 1` (锚点模式)
- `anchors_preset = 15` (全屏)
- `anchor_right = 1.0`, `anchor_bottom = 1.0`
- `grow_horizontal = 2`, `grow_vertical = 2`

**Size Flags:**
- `size_flags_horizontal = 3` (Fill + Expand)
- `size_flags_horizontal = 4` (Shrink Center)
- `size_flags_horizontal = 6` (Shrink Begin + Shrink Center)
- `size_flags_vertical = 4` (Shrink Center)

**Margin (通过theme_override):**
```
theme_override_constants/margin_left = 30
theme_override_constants/margin_top = 30
theme_override_constants/margin_right = 30
theme_override_constants/margin_bottom = 30
```

**Script引用:**
```
script = ExtResource("1_dsrpe")
UniformMargin = 30
metadata/_custom_type_script = "uid://bk83ics8idr7w"
```

#### 4. 常用节点类型
- Control (根节点)
- ColorRect (背景色)
- MarginContainer (边距控制)
- PanelContainer (带背景的容器)
- VBoxContainer (垂直布局)
- HBoxContainer (水平布局)
- Label (文本)
- ProgressBar (进度条)
- Button (按钮)
- HSeparator (水平分隔线)

#### 5. Preset资源
位置: `A1UIPresets/`
- `new_style_box_line.tres` - 分隔线样式
- `label_settings_title.tres` - 标题文本样式
- `margin_container_helper.tscn` - Margin容器预设

## 工具设计方案

### 方案选择: Python (推荐)
**原因:**
- AI更熟悉Python
- 字符串处理和文件操作更方便
- 不需要在Godot运行时执行
- 可以独立运行，生成.tscn文件

### 核心架构

#### 1. UINode类
```python
class UINode:
    def __init__(self, name, node_type, unique_id=None):
        self.name = name
        self.node_type = node_type
        self.unique_id = unique_id or random.randint(1, 2**31-1)
        self.parent_path = "."
        self.properties = {}
        self.children = []
```

#### 2. UIBuilder类
```python
class UIBuilder:
    def __init__(self, scene_name):
        self.scene_name = scene_name
        self.root = None
        self.ext_resources = []
    
    def create_control(self, name="Control"):
        # 创建根节点
    
    def add_ext_resource(self, res_type, path):
        # 添加外部资源引用
    
    def generate_tree_view(self):
        # 生成树状图（给AI看）
    
    def generate_tscn(self):
        # 生成.tscn文本
    
    def save(self, output_path):
        # 保存到文件
```

#### 3. UINode扩展方法
```python
def add_color_rect(self, name, color=(0.15, 0.15, 0.15, 1)):
def add_margin_container(self, name, uniform=None, script=None):
def add_panel_container(self, name):
def add_vbox(self, name):
def add_hbox(self, name):
def add_label(self, name, text="", align="left", font_size=None):
def add_progress_bar(self, name, value=0, size_flags_h=None):
def add_button(self, name, text=""):
def add_separator(self, name, separation=None, style=None):
```

### API使用示例
```python
from godot_ui_builder import UIBuilder

# 创建构建器
ui = UIBuilder("SettingsUI")

# 创建根节点
root = ui.create_control("Control")

# 添加背景
bg = root.add_color_rect("Background_ColorRect", color=(0.157, 0.157, 0.157, 1))

# 添加外边距容器
margin = root.add_margin_container("MarginContainerHelper", uniform=30, 
                                   script="res://addons/MyAddon/Helpers/MarginContainerHelper.cs")

# 添加面板
panel = margin.add_panel_container("PanelContainer")

# 添加内边距
inner_margin = panel.add_margin_container("MarginContainerHelper", uniform=33)

# 添加垂直布局
vbox = inner_margin.add_vbox("VBoxContainer")

# 添加标题
vbox.add_label("Label", text="Setting", align="center")

# 添加分隔线
vbox.add_separator("HSeparator", separation=56, style="res://A1UIPresets/new_style_box_line.tres")

# 添加第一行
hbox1 = vbox.add_hbox("HBoxContainer")
hbox1.add_label("Label2", text="text13243", font_size=41)
hbox1.add_progress_bar("ProgressBar", value=10.17, size_flags_h=3)

# 添加第二行
hbox2 = vbox.add_hbox("HBoxContainer2")
hbox2.add_label("Label2", text="text132", font_size=41)
hbox2.add_progress_bar("ProgressBar", value=10.17, size_flags_h=3)

# 添加按钮
vbox.add_button("Button", text="SeeMore")

# 生成树状图（给AI看）
print(ui.generate_tree_view())

# 保存
ui.save("A1UIScenes/SettingsUI_rebuilt.tscn")
```

### 树状图输出格式
```
Control (Control) [root]
├── Background_ColorRect (ColorRect)
└── MarginContainerHelper (MarginContainer) [script]
    └── PanelContainer (PanelContainer)
        └── MarginContainerHelper (MarginContainer)
            └── VBoxContainer (VBoxContainer)
                ├── Label (Label) "Setting"
                ├── HSeparator (HSeparator)
                ├── HBoxContainer (HBoxContainer)
                │   ├── Label2 (Label) "text13243"
                │   └── ProgressBar (ProgressBar)
                ├── HBoxContainer2 (HBoxContainer)
                │   ├── Label2 (Label) "text132"
                │   └── ProgressBar (ProgressBar)
                └── Button (Button) "SeeMore"
```

## 实现步骤

### Phase 1: 核心功能
- [x] 分析现有UI结构
- [x] 实现UINode类
- [x] 实现UIBuilder类
- [x] 实现基础节点类型（Control, ColorRect, Container）
- [x] 实现.tscn文本生成
- [x] 实现树状图生成

### Phase 2: 扩展功能
- [x] 添加更多节点类型（Label, Button, ProgressBar等）
- [x] 支持preset资源引用
- [x] 支持script引用
- [x] 支持theme_override
- [x] 修复关键参数（show_percentage, use_anchors等）

### Phase 3: 测试与优化
- [x] 重建SettingsUI.tscn
- [x] 对比原始文件和生成文件
- [x] 在Godot中测试
- [x] 优化API易用性
- [x] 创建参数速查表

## 文件组织
```
.kiro/
├── TempFolder/
│   └── UIBuilder_Analysis.md (本文件，完成后删除)
└── scripts/
    └── ui_builder/
        ├── godot_ui_builder.py (核心构建器)
        ├── ui_presets.py (preset管理)
        └── example_rebuild_settings.py (重建示例)
```

## 下一步
1. 实现 `godot_ui_builder.py` 核心功能
2. 创建 `example_rebuild_settings.py` 示例
3. 测试生成的.tscn文件
