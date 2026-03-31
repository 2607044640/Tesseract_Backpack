# Transparent Glow Theme - Usage Guide

## Overview
A transparent UI theme inspired by AAA console games (NieR: Automata style) with three distinct visual states that create a "Quietly Awaiting → Wake Up → Execute Command" progression.

**Generated File**: `A1UIResources/ThemeGen/generated/transparent_glow_theme.tres`

---

## Visual Design Philosophy

### 1. Normal State - "Quietly Awaiting Command"
- **Transparency**: 20-30% opacity, barely visible
- **Border**: Thin 1px white border (40% opacity)
- **Text**: Pure white with black outline (2px)
- **Shadow**: None - minimal presence

### 2. Hover State - "I'm Watching You!"
- **Transparency**: 70% opacity, strong visual change
- **Border**: 2px bright white border
- **Glow**: White halo effect (8px shadow, no offset)
- **Text**: White with black outline, high contrast

### 3. Pressed State - "Execute Command!"
- **Background**: 100% opaque white (flash effect)
- **Text**: Black (color inversion for maximum contrast)
- **Shadow**: Inner shadow simulation
- **Border**: 2px bright white

---

## How to Generate

### Method 1: Manual Generation
1. Open `transparent_glow_theme.gd` in Godot editor
2. Press `Ctrl+Shift+X` to run
3. Check Output tab for confirmation
4. Find generated file: `generated/transparent_glow_theme.tres`

### Method 2: Auto-Generation (Recommended)
1. Enable "ThemeGen Save Sync" plugin (Project Settings → Plugins)
2. Edit `transparent_glow_theme.gd`
3. Save file (Ctrl+S)
4. Theme auto-regenerates

---

## Applying the Theme

### Global Application (Entire UI)
```csharp
// In your main UI manager or autoload
var transparentTheme = GD.Load<Theme>("res://A1UIResources/ThemeGen/generated/transparent_glow_theme.tres");
GetTree().Root.Theme = transparentTheme;
```

### Section Application (Specific UI)
```csharp
// Apply to specific control subtree
menuRoot.Theme = GD.Load<Theme>("res://A1UIResources/ThemeGen/generated/transparent_glow_theme.tres");
```

### Inspector Application
1. Select root Control node of your UI
2. Inspector → Theme → Load `transparent_glow_theme.tres`
3. All children inherit automatically

---

## Available Components

### Styled Controls
- **Button** - Full transparent glow treatment
- **AccentButton** (variant) - Slightly more visible by default
- **Label** - White text with black outline
- **TitleLabel** (variant) - Larger font, thicker outline
- **PanelContainer** - Transparent backdrop
- **ElevatedPanel** (variant) - More visible with glow
- **LineEdit** - Input with glow on focus
- **CheckBox** - Minimal with glow on hover
- **ProgressBar** - Transparent track, glowing fill
- **VScrollBar/HScrollBar** - Minimal presence
- **TabContainer** - Floating tabs with glow
- **OptionButton** - Dropdown with glow
- **PopupMenu** - Floating menu with shadow
- **Tree** - Hierarchical view with glow selection
- **ItemList** - Selection list with glow

---

## Using Type Variations

### In Inspector
1. Select Button node
2. Theme → Theme Type Variation → Type "AccentButton"
3. Button now uses accent variant

### In C#
```csharp
button.ThemeTypeVariation = "AccentButton";
label.ThemeTypeVariation = "TitleLabel";
panel.ThemeTypeVariation = "ElevatedPanel";
```

---

## Customization Tips

### Adjusting Transparency Levels
Edit the color variables in `transparent_glow_theme.gd`:
```gdscript
var transparent_subtle = Color(0.0, 0.0, 0.0, 0.25)  # Normal state
var transparent_medium = Color(1.0, 1.0, 1.0, 0.7)   # Hover state
var opaque_white = Color(1.0, 1.0, 1.0, 1.0)         # Pressed state
```

### Adjusting Glow Intensity
```gdscript
var glow_white = Color(1.0, 1.0, 1.0, 0.8)  # Glow color
# In hover state:
shadow_size = 8,  # Increase for stronger glow
```

### Adjusting Text Outline
```gdscript
var outline_size = 2  # Increase for thicker outline
var outline_black = Color(0.0, 0.0, 0.0, 0.8)  # Adjust opacity
```

---

## Best Practices

### ✓ DO:
- Use this theme for overlay UIs, pause menus, HUDs
- Combine with background blur for better readability
- Test against various background colors/scenes
- Use AccentButton for primary actions
- Apply to entire UI tree for consistency

### ✗ DON'T:
- Use on busy backgrounds without blur (readability issues)
- Manually edit the generated `.tres` file
- Modify shared theme resources without duplicating
- Over-use per-node overrides

---

## Performance Notes

- Transparent rendering is GPU-efficient in Godot
- Shadow/glow effects have minimal performance impact
- Theme switching is instant (single property change)
- Suitable for real-time games

---

## Preview

To preview the theme:
1. Open `theme_preview.tscn`
2. Select root node
3. Inspector → Theme → Load `transparent_glow_theme.tres`
4. Test hover/press interactions

---

## Integration with ThemeSwitcher

```csharp
// In ThemeSwitcherComponentHelper.cs
[Export] public Theme TransparentTheme { get; set; }

public void ApplyTransparentTheme()
{
    GetTree().Root.Theme = TransparentTheme;
}
```

Then assign the generated theme in the Inspector.

---

## Technical Details

### Color Palette
- **Text**: Pure white (#FFFFFF) with black outline
- **Background Normal**: Black 25% opacity
- **Background Hover**: White 70% opacity
- **Background Pressed**: White 100% opacity (flash)
- **Glow**: White 80% opacity, 8px radius
- **Border Normal**: White 40% opacity, 1px
- **Border Hover**: White 100% opacity, 2px

### Font Settings
- **Default Size**: 16px
- **Outline Size**: 2px
- **Outline Color**: Black 80% opacity

### Shadow/Glow Settings
- **Hover Glow**: 8px radius, no offset (radiates evenly)
- **Focus Glow**: 6px radius, bright white
- **Pressed Shadow**: 4px radius, inner shadow simulation

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Theme not generating | Press Ctrl+Shift+X in theme script |
| Changes not visible | Reload scene or restart Godot editor |
| Text hard to read | Increase outline_size or background opacity |
| Glow too subtle | Increase shadow_size in hover states |
| Glow too strong | Decrease shadow_size or shadow_color alpha |

---

## Future Enhancements

Consider adding:
- Animated transitions (requires custom Control scripts)
- Ripple effect on press (requires shader or custom drawing)
- Dynamic scale on hover (requires Tween in Control script)
- Particle effects on interaction
- Sound feedback integration
