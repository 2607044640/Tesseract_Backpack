# ThemeGen 主题集合

这个文件夹包含了4个不同风格的 Godot UI 主题，使用 ThemeGen 插件生成。

## 主题列表

### 1. 现代游戏主题 (Modern Game Theme)
**文件**: `modern_game_theme.gd`
**风格**: 深色背景 + 青色/橙色强调色
**适用**: 现代科幻游戏、赛博朋克风格
**特点**:
- 深色背景 (#1a1a2e, #16213e)
- 青色主色调 (#00d4ff)
- 橙色次要色 (#ff6b35)
- 圆角按钮 (8px)
- 带边框设计

### 2. 简约亮色主题 (Light Minimal Theme)
**文件**: `light_minimal_theme.gd`
**风格**: Material Design 风格的简约亮色主题
**适用**: 休闲游戏、工具应用
**特点**:
- 白色/浅灰背景
- Material Blue 主色 (#2196f3)
- 扁平设计
- 底部边框输入框
- 包含 FlatButton 变体（透明背景按钮）

### 3. 优雅深色主题 (Dark Elegant Theme)
**文件**: `dark_elegant_theme.gd`
**风格**: 专业的深色主题，带阴影效果
**适用**: 策略游戏、管理游戏
**特点**:
- 多层次深色背景
- 紫色主色调 (#bb86fc)
- 青色强调色 (#03dac6)
- 阴影和高度效果
- 包含 AccentButton 和 ElevatedPanel 变体

### 4. 幻想RPG主题 (Fantasy RPG Theme)
**文件**: `fantasy_rpg_theme.gd`
**风格**: 中世纪/奇幻游戏风格
**适用**: RPG、冒险游戏
**特点**:
- 羊皮纸背景 (#f4e8d0)
- 木头和皮革材质
- 金色/青铜色边框
- 厚边框设计 (3-4px)
- 包含 HealthBar 和 ManaBar 变体

## 如何使用

### 生成主题文件

1. **在 Godot 编辑器中打开任一主题脚本**
   - 例如: `modern_game_theme.gd`

2. **运行脚本生成主题**
   - 方法1: 按 `Ctrl+Shift+X` (File → Run)
   - 方法2: 保存文件自动生成（已启用 UPDATE_ON_SAVE）

3. **生成的主题文件位置**
   - `A1UIResources/ThemeGen/generated/[主题名].tres`

### 应用主题到场景

1. 选择场景的根 Control 节点
2. 在 Inspector 中找到 `Theme` 属性
3. 加载生成的 `.tres` 文件
4. 所有子节点将自动应用该主题

### 使用主题变体

某些主题包含变体样式，使用方法：

```gdscript
# 在节点的 Inspector 中设置
# Theme Overrides → Theme Type Variation
# 输入变体名称，例如: "TitleLabel", "AccentButton", "HealthBar"
```

或在代码中：

```gdscript
$Label.theme_type_variation = "TitleLabel"
$Button.theme_type_variation = "GoldButton"
```

## 自定义主题

你可以修改任何 `.gd` 文件中的颜色变量来自定义主题：

```gdscript
# 例如在 modern_game_theme.gd 中
var primary_color = Color("#00d4ff")  # 改成你喜欢的颜色
var background_dark = Color("#1a1a2e")
```

保存后主题会自动重新生成。

## 主题变体对照表

| 主题 | 可用变体 |
|------|---------|
| Modern Game | TitleLabel |
| Light Minimal | FlatButton, TitleLabel, SubtitleLabel |
| Dark Elegant | AccentButton, ElevatedPanel, AccentLabel |
| Fantasy RPG | GoldButton, DarkPanel, TitleLabel, LightLabel, HealthBar, ManaBar |

## 故障排除

**问题**: 主题没有自动生成
- 确保 ThemeGen Save Sync 插件已启用 (Project Settings → Plugins)
- 检查脚本中是否有 `const UPDATE_ON_SAVE = true`

**问题**: 主题应用后没有变化
- 重新设置根节点的 Theme 属性
- 或重启 Godot 编辑器

**问题**: 某些控件样式不对
- 检查控件类型是否在主题中定义
- 某些自定义控件可能需要额外配置
