# Transparent Glow Theme - Quick Start

## Generate Theme

### Option 1: Manual
1. Open `transparent_glow_theme.gd` in Godot
2. Press `Ctrl+Shift+X`
3. Check Output tab

### Option 2: Auto (Recommended)
1. Enable "ThemeGen Save Sync" plugin (Project Settings → Plugins)
2. Edit and save `transparent_glow_theme.gd`
3. Theme auto-generates

**Output**: `generated/transparent_glow_theme.tres`

---

## Apply Theme

### In Inspector
1. Select root Control node
2. Theme property → Load `transparent_glow_theme.tres`
3. Done!

### In C#
```csharp
var theme = GD.Load<Theme>("res://A1UIResources/ThemeGen/generated/transparent_glow_theme.tres");
GetTree().Root.Theme = theme;
```

---

## Test Scene

Open `transparent_theme_test.tscn` to preview all styled controls.

---

## Visual States

| State | Appearance |
|-------|------------|
| **Normal** | 25% transparent, thin border, white text + black outline |
| **Hover** | 70% opaque, bright glow (8px), strong presence |
| **Pressed** | 100% white flash, black text (inversion), inner shadow |

---

## Type Variations

Apply via Inspector → Theme Type Variation:
- `AccentButton` - More visible by default
- `TitleLabel` - Larger font, thicker outline
- `ElevatedPanel` - Prominent with glow

---

## Customization

Edit color variables in `transparent_glow_theme.gd`:
```gdscript
var transparent_subtle = Color(0.0, 0.0, 0.0, 0.25)  # Normal opacity
var transparent_medium = Color(1.0, 1.0, 1.0, 0.7)   # Hover opacity
var outline_size = 2  # Text outline thickness
```

Save → Auto-regenerates (if Save Sync enabled)

---

## Best Use Cases

✓ Overlay UIs, pause menus, HUDs  
✓ Futuristic/sci-fi games  
✓ Minimalist console-style interfaces  
✓ Transparent overlays on gameplay

⚠ Use with background blur for readability  
⚠ Test against various background colors
