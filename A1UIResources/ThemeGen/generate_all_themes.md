# 生成所有主题

## 方法1: 批量生成（推荐）⚡

**一键生成所有4个主题：**

1. 在 Godot 编辑器中打开 `generate_all.gd`
2. 按 `Ctrl+Shift+X` 运行
3. 查看 Output 标签，所有主题会自动生成

**就这么简单！**

## 方法2: 单独生成

如果只想生成某个主题：

1. 打开对应的主题脚本（如 `modern_game_theme.gd`）
2. 按 `Ctrl+Shift+X` 运行
3. 查看 Output 标签确认生成成功

## 主题文件列表

生成后，你会在 `A1UIResources/ThemeGen/generated/` 文件夹中看到：

- `modern_game_theme.tres` - 现代游戏风格
- `light_minimal_theme.tres` - 简约亮色主题
- `dark_elegant_theme.tres` - 优雅深色主题
- `fantasy_rpg_theme.tres` - 幻想RPG主题

## 预览主题

1. 打开 `theme_preview.tscn` 场景
2. 选择根节点 `ThemePreview`
3. 在 Inspector 中的 Theme 属性加载生成的主题文件
4. 查看效果

## 快速测试脚本

如果你想一次性生成所有主题，可以创建一个脚本：

```gdscript
@tool
extends EditorScript

func _run():
	var themes = [
		"res://A1UIResources/ThemeGen/modern_game_theme.gd",
		"res://A1UIResources/ThemeGen/light_minimal_theme.gd",
		"res://A1UIResources/ThemeGen/dark_elegant_theme.gd",
        "res://A1UIResources/ThemeGen/fantasy_rpg_theme.gd"
	]
	
	for theme_path in themes:
		var script = load(theme_path)
		if script:
			var instance = script.new()
			print("Generating: ", theme_path)
```

保存为 `generate_all.gd` 并运行。
