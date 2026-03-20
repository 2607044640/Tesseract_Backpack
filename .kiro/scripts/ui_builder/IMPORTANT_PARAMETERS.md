# Godot UI 关键参数速查表

## 🚨 必须设置的参数（否则UI会很丑）

### 1. ProgressBar - 显示百分比

```python
# ❌ 错误：没有百分比显示
progress_bar = hbox.add_progress_bar("HP", value=75)

# ✅ 正确：显示百分比和正确布局
progress_bar = hbox.add_progress_bar(
    "HP",
    value=75,
    size_flags_h=3,              # Fill + Expand
    size_flags_v=4,              # Shrink Center
    size_flags_stretch_ratio=2.94,  # 拉伸比例
    show_percentage=True         # 显示百分比！
)
```

**效果对比：**
- 没有 `show_percentage`: 只有空条，看不到数值
- 有 `show_percentage`: 显示"75%"文字

### 2. MarginContainer - 全屏填充

```python
# ❌ 错误：不会填充整个屏幕
margin = root.add_margin_container("Margin", uniform=30)

# ✅ 正确：使用锚点模式填充屏幕
margin = root.add_margin_container(
    "Margin",
    uniform=30,
    use_anchors=True  # 关键！
)
margin.set_property("anchors_preset", 15)
margin.set_property("anchor_right", 1.0)
margin.set_property("anchor_bottom", 1.0)
margin.set_property("grow_horizontal", 2)
margin.set_property("grow_vertical", 2)
```

### 3. Label - 文字对齐

```python
# ❌ 错误：文字可能不居中
label = vbox.add_label("Title", text="Hello")

# ✅ 正确：明确设置对齐方式
label = vbox.add_label("Title", text="Hello", align="center")

# 如果在HBoxContainer中，还需要：
label.set_property("horizontal_alignment", 1)  # 1=居中
```

### 4. Container - Size Flags

```python
# ❌ 错误：容器不会自动填充空间
vbox = margin.add_vbox("Menu")

# ✅ 正确：让容器填充可用空间
vbox = margin.add_vbox("Menu")
vbox.set_property("size_flags_horizontal", 3)  # Fill + Expand
vbox.set_property("size_flags_vertical", 3)    # Fill + Expand
```

## 📊 Size Flags 数值含义

### Horizontal / Vertical Flags

| 值 | 名称 | 含义 | 使用场景 |
|----|------|------|----------|
| 0 | None | 保持最小尺寸 | 固定大小的元素 |
| 1 | Fill | 填充可用空间 | 需要填充但不扩展 |
| 2 | Expand | 扩展以占用额外空间 | 需要占用剩余空间 |
| 3 | Fill + Expand | 填充并扩展 | **最常用**：进度条、容器 |
| 4 | Shrink Center | 收缩并居中 | 按钮、固定大小元素 |
| 6 | Shrink Begin + Center | 收缩、靠左/上、居中 | 标签 |

### Stretch Ratio（拉伸比例）

```python
# 控制元素相对宽度
label = hbox.add_label("Name", text="HP:")  # 默认ratio=1.0
progress = hbox.add_progress_bar("HP", size_flags_stretch_ratio=2.94)  # ratio=2.94

# 结果：progress的宽度是label的2.94倍
```

## 🎨 常见UI模式

### 模式1: 全屏UI（带边距）

```python
ui = UIBuilder("MyUI")
root = ui.create_control("Control", fullscreen=True)

# 外边距（锚点模式）
margin = root.add_margin_container("Margin", uniform=50, use_anchors=True)
margin.set_property("anchors_preset", 15)
margin.set_property("anchor_right", 1.0)
margin.set_property("anchor_bottom", 1.0)
margin.set_property("grow_horizontal", 2)
margin.set_property("grow_vertical", 2)

# 面板（带背景）
panel = margin.add_panel_container("Panel")

# 内容
vbox = panel.add_vbox("Content")
```

### 模式2: 标签 + 进度条

```python
hbox = vbox.add_hbox("Row")

# 标签（固定宽度）
hbox.add_label(
    "Label",
    text="Health:",
    font_size=24,
    min_size=(100, 0),
    size_flags_h=6  # Shrink Begin + Center
)

# 进度条（填充剩余空间）
hbox.add_progress_bar(
    "Bar",
    value=75,
    size_flags_h=3,  # Fill + Expand
    size_flags_v=4,  # Shrink Center
    show_percentage=True
)
```

### 模式3: 居中按钮列表

```python
vbox = margin.add_vbox("Menu", separation=20)

# 标题
vbox.add_label("Title", text="Main Menu", align="center", font_size=48)

# 按钮（居中）
vbox.add_button("Start", text="Start Game", size_flags_h=4)  # Shrink Center
vbox.add_button("Quit", text="Quit", size_flags_h=4)
```

## 🔧 调试技巧

### 检查UI是否正确填充

1. **运行场景**：在Godot中运行UI场景
2. **调整窗口大小**：看UI是否响应式缩放
3. **检查进度条**：是否显示百分比文字
4. **检查容器**：是否填充整个可用空间

### 常见问题

**问题1: ProgressBar没有显示百分比**
```python
# 解决：添加 show_percentage=True
progress_bar.add_progress_bar("HP", value=75, show_percentage=True)
```

**问题2: UI不填充整个屏幕**
```python
# 解决：使用锚点模式
margin = root.add_margin_container("Margin", uniform=30, use_anchors=True)
margin.set_property("anchors_preset", 15)
margin.set_property("anchor_right", 1.0)
margin.set_property("anchor_bottom", 1.0)
```

**问题3: 元素挤在一起**
```python
# 解决：设置size_flags_h=3让元素填充空间
element.set_property("size_flags_horizontal", 3)
```

## 📝 完整示例

参考 `example_rebuild_settings.py` 查看完整的、正确的UI实现。

## 🎯 记住这些核心原则

1. **ProgressBar必须有 `show_percentage=True`**
2. **全屏容器必须用 `use_anchors=True`**
3. **填充空间用 `size_flags_h=3`**
4. **居中元素用 `size_flags_h=4`**
5. **使用 `size_flags_stretch_ratio` 控制相对宽度**
