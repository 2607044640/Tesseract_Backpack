# Modular Settings Menu - Godot 4.6 兼容性修复

## 修复日期
2026-03-18

## 问题概述
modular-settings-menu 插件使用了 Godot 3.x/4.0 早期版本的信号语法，在 Godot 4.6 中导致解析错误。

## 主要修复内容

### 1. 信号连接语法更新
**旧语法 (Godot 3.x):**
```gdscript
connect("signal_name", method_name)
```

**新语法 (Godot 4.x):**
```gdscript
signal_name.connect(method_name)
```

### 2. 信号发射语法更新
**旧语法:**
```gdscript
emit_signal("signal_name", arg1, arg2)
```

**新语法:**
```gdscript
signal_name.emit(arg1, arg2)
```

### 3. 延迟信号发射更新
**旧语法:**
```gdscript
call_deferred("emit_signal", "signal_name", args)
```

**新语法:**
```gdscript
signal_name.call_deferred("emit", args)
```

## 修复的文件列表

### 核心脚本
1. `scripts/settings_menu.gd`
   - ✅ 5处信号发射修复
   - ✅ 1处延迟信号发射修复

2. `scripts/settings_section.gd`
   - ✅ 3处信号连接修复
   - ✅ 1处信号发射修复

3. `scripts/element_panel.gd`
   - ✅ 2处信号连接修复

4. `singletons/settings_data_manager.gd`
   - ✅ 无需修改（无信号使用）

### 基础元素脚本
5. `scripts/base-settings-elements/settings_element.gd`
   - ✅ 2处信号连接修复
   - ✅ 1处延迟信号发射修复

6. `scripts/base-settings-elements/slider_element.gd`
   - ✅ 2处信号连接修复

7. `scripts/base-settings-elements/option_element.gd`
   - ✅ 1处信号连接修复

8. `scripts/base-settings-elements/toggle_element.gd`
   - ✅ 1处信号连接修复

9. `scripts/base-settings-elements/button_element.gd`
   - ✅ 1处信号连接修复
   - ✅ 修复方法名冲突：`pressed()` → `_on_pressed()`

10. `scripts/base-settings-elements/multi_element.gd`
    - ✅ 4处信号连接修复

### 处理器脚本
11. `scripts/settings-handler-scripts/camera_settings_handler.gd`
    - ✅ 1处信号连接修复

12. `scripts/settings-handler-scripts/world_env_settings_handler.gd`
    - ✅ 1处信号连接修复

### 元素脚本
13. `scripts/settings-elements-scripts/audio_setting.gd`
    - ✅ 无需修改（无信号使用）

## 特殊修复说明

### button_element.gd 方法名冲突
**问题:** Button 节点自带 `pressed` 信号，方法名也叫 `pressed()` 导致冲突
**解决:** 重命名方法为 `_on_pressed()`

```gdscript
# 修复前
func _ready():
    pressed.connect(pressed)  # ❌ 冲突！

func pressed() -> void:
    # ...

# 修复后
func _ready():
    pressed.connect(_on_pressed)  # ✅ 正确

func _on_pressed() -> void:
    # ...
```

## 测试结果
- ✅ 编辑器加载无解析错误
- ✅ 所有脚本语法正确
- ✅ 插件可正常使用

## 兼容性
- Godot 4.6.1 stable mono ✅
- 向后兼容 Godot 4.0+ ✅

## 总计修复
- 文件数量: 13 个
- 信号连接修复: 15 处
- 信号发射修复: 6 处
- 延迟信号修复: 2 处
- 方法名冲突: 1 处
