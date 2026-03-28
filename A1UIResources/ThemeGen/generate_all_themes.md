# 生成所有主题

由于需要在 Godot 编辑器中运行，请按照以下步骤操作：

## 方法1: 自动生成（推荐）

1. **确保插件已启用**
   - 打开 Godot 编辑器
   - Project → Project Settings → Plugins
   - 确保 "ThemeGen Save Sync" 已勾选

2. **打开并保存每个主题脚本**
   - 在 Godot 中打开 `modern_game_theme.gd`
   - 按 `Ctrl+S` 保存
   - 主题会自动生成到 `generated/modern_game_theme.tres`
   - 对其他3个主题重复此操作

## 方法2: 手动运行

1. 在 Godot 编辑器中打开主题脚本
2. 按 `Ctrl+Shift+X` 或选择 File → Run
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
