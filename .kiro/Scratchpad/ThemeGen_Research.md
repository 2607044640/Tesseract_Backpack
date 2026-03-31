# ThemeGen Plugin Research - Iteration 1

## Initial Context
- **Location**: `C:\Godot\3d-practice\A1UIResources\ThemeGen`
- **Current Implementation**: Custom GDScript-based theme generation system
- **Base Class**: `ProgrammaticTheme` (extends EditorScript with @tool)
- **Purpose**: Generate Godot Theme resources programmatically

## Current System Overview
The project uses a custom theme generation system with:
- Multiple theme presets (modern_game, light_minimal, dark_elegant, fantasy_rpg)
- GDScript files that extend `ProgrammaticTheme`
- Generated `.tres` files in `generated/` folder
- Preview scene for testing themes

## Key Features Observed
1. **Programmatic Definition**: Themes defined in code, not GUI
2. **Helper Functions**: `stylebox_flat()`, `inherit()`, `corner_radius()`, etc.
3. **Variant System**: Create theme variants (e.g., AccentButton from Button)
4. **Auto-generation**: `UPDATE_ON_SAVE = true` triggers regeneration
5. **Verbosity Control**: QUIET/VERBOSE output modes

## Questions to Research
1. Is "ProgrammaticTheme" a public plugin or custom implementation?
2. What are industry best practices for Godot theme management?
3. Are there alternative theme generation tools?
4. How do other projects handle theme variants and switching?
5. What's the relationship to Godot's built-in Theme system?

---
## Search 1: ThemeGen Plugin Identity ✓

**Source**: [Godot Asset Library](https://godotassetlibrary.com/asset/6IOw3X/themegen), [GitHub: Inspiaaa/ThemeGen](https://github.com/Inspiaaa/ThemeGen)

### Key Findings:
1. **Official Plugin**: ThemeGen by Inspiaaa - "Possibly the best theming solution for Godot to date"
2. **GitHub Repository**: https://github.com/Inspiaaa/ThemeGen
3. **Supported Version**: Godot 4.0+
4. **Installation**: Via Godot Asset Library or manual `addons/` folder

### Core Features Confirmed:
- **GDScript-based theme definition** (matches current implementation)
- **Style reuse and recombination** (explains `inherit()` function)
- **Semantic color management** (color variables shared across components)
- **Live preview feature** - auto-regenerates on script modification
- **Multiple theme variations** (dark/light themes)

### Advantages Over UI Editor:
- Code-based = version control friendly
- Reusable style components
- Color sharing between components
- Programmatic theme generation

### Updated Understanding:
The project IS using the official ThemeGen plugin. The `ProgrammaticTheme` base class comes from this addon.

---
## Search 2: ThemeGen GitHub Documentation Deep Dive ✓

**Source**: [GitHub: Inspiaaa/ThemeGen](https://github.com/Inspiaaa/ThemeGen)

### Complete API Reference:

#### Core Methods:
1. **`setup()`** - Initialize theme, set save path, define color variables
2. **`define_theme()`** - Main theme definition method
3. **`define_style(node_name, properties_dict)`** - Style a node type
4. **`define_variant_style(variant_name, base_name, properties_dict)`** - Create theme variations
5. **`define_default_font(font_resource)`** - Set default font
6. **`define_default_font_size(size)`** - Set default font size

#### StyleBox Helper Functions:
- **`stylebox_flat(props)`** - Create StyleBoxFlat
- **`stylebox_line(props)`** - Create StyleBoxLine
- **`stylebox_empty(props)`** - Create StyleBoxEmpty
- **`stylebox_texture(props)`** - Create StyleBoxTexture
- **`inherit(base, overrides...)`** - Inherit and override StyleBox properties
- **`merge(a, b, c...)`** - Merge multiple StyleBoxes (semantic alternative to inherit)

#### Shortcut Functions (Repetitive Properties):
- **`border_width(all)` or `border_width(left, top, right, bottom)`**
- **`corner_radius(all)` or `corner_radius(tl, tr, br, bl)`**
- **`expand_margins(all)` or `expand_margins(left, top, right, bottom)`**
- **`content_margins(all)` or `content_margins(left, top, right, bottom)`**
- **`texture_margins(all)` or `texture_margins(left, top, right, bottom)`**

#### Advanced Features:
- **Multiple Theme Variants**: Create `setup_light_theme()`, `setup_dark_theme()` functions
- **Custom Theme Generator**: Use `set_theme_generator(custom_function)` to override `define_theme()`
- **Direct Theme Access**: `current_theme` property for custom properties
- **Built-in Styles Reference**: `styles.Button.normal` to reference parent styles in variants

#### Configuration Constants:
```gdscript
const UPDATE_ON_SAVE = true  # Auto-regenerate on save
const VERBOSITY = Verbosity.QUIET  # SILENT, QUIET, NORMAL
```

#### Underscore Suffix Pattern:
```gdscript
border_ = border_width(2)  # Expands to border_width_left/top/right/bottom
corner_ = corner_radius(8)  # Expands to corner_radius_top_left/etc.
content_ = content_margins(10)  # Expands to content_margin_left/etc.
```

### Live Preview System:
1. **Manual**: Run script with Ctrl+Shift+X
2. **Automatic**: Enable "ThemeGen Save Sync" plugin in Project Settings
3. **Requirement**: Add `const UPDATE_ON_SAVE = true` to script

### Theme Variant Workflow:
- Each `setup_*()` function = one theme variant
- Shared `define_theme()` for common structure
- Override with `set_theme_generator()` for unique variants

---
## Search 3: Godot Theme System Best Practices ✓

**Source**: [UhiyamaLab - Unified UI Design](https://uhiyama-lab.com/en/notes/godot/theme-system-unified-ui/)

### Theme System Architecture:

#### Five Theme Item Types:
1. **Color** - UI element colors (text, backgrounds)
2. **Constant** - Numerical values (margins, padding, spacing)
3. **Font** - Font resources (family, size, antialiasing)
4. **Icon** - Textures/icons for UI elements
5. **StyleBox** - Most important: backgrounds, borders, corners, shadows, padding

#### Theme Inheritance Hierarchy (CSS-like cascade):
1. Node-specific overrides (`add_theme_*_override()`) - Highest priority
2. Node's theme property (direct assignment)
3. Parent node's theme property (inherited)
4. Project settings default theme (global)
5. Godot editor default theme (fallback)

### Best Practices vs Anti-Patterns:

| Anti-Pattern ❌ | Best Practice ✓ |
|----------------|-----------------|
| Override everything with `add_theme_*_override()` | Leverage theme inheritance |
| Giant single theme file | Split themes by concern (base, game UI, menu UI) |
| Directly modify theme resources | Always `.duplicate()` before modifying |
| Individual node settings without themes | Theme-centric design (90% themes, 10% overrides) |

### Dynamic Theme Switching Pattern:
```gdscript
# Singleton pattern for dark/light mode
const LIGHT_THEME = preload("res://themes/light_theme.tres")
const DARK_THEME = preload("res://themes/dark_theme.tres")

func apply_theme():
    get_tree().root.theme = selected_theme
```

### Performance Considerations:
- Theme system has minimal performance impact
- Excessive per-node overrides (thousands+) can increase rendering load
- Use `preload()` for themes (loaded at scene load time)
- Consider `load()` for async loading of large themes

### Key Insight:
"Using the theme system for serious UI in Godot is not 'recommended' but 'essential.'" Themes guarantee consistency at the design level and transform UI from ordinary to exceptional.

---
## Search 4: C# Integration & Runtime Theme Management ✓

**Sources**: Godot Forums, GitHub Issues, LobeHub Skills

### C# Theme API Methods:

#### Global Theme Assignment:
```csharp
// Apply theme to entire UI tree
GetTree().Root.Theme = loadedTheme;

// Apply to specific control
myControl.Theme = loadedTheme;
```

#### Runtime Overrides (Per-Node):
```csharp
// StyleBox override
button.AddThemeStyleboxOverride("normal", customStyleBox);

// Color override
label.AddThemeColorOverride("font_color", new Color(1, 0, 0));

// Font size override
button.AddThemeFontSizeOverride("font_size", 24);

// Remove override
button.RemoveThemeStyleboxOverride("normal");
```

#### Important: Resource Duplication
```csharp
// WRONG - modifies shared resource
var stylebox = button.GetThemeStylebox("normal");
stylebox.BgColor = Colors.Red; // Affects ALL buttons!

// CORRECT - duplicate first
var stylebox = button.GetThemeStylebox("normal").Duplicate() as StyleBoxFlat;
stylebox.BgColor = Colors.Red;
button.AddThemeStyleboxOverride("normal", stylebox);
```

### C# Theme Switching Pattern:
```csharp
public partial class ThemeManager : Node
{
    private Theme _lightTheme = GD.Load<Theme>("res://themes/light.tres");
    private Theme _darkTheme = GD.Load<Theme>("res://themes/dark.tres");
    
    public void SwitchTheme(bool isDark)
    {
        GetTree().Root.Theme = isDark ? _darkTheme : _lightTheme;
    }
}
```

### Integration with ThemeGen:
- ThemeGen generates `.tres` files (GDScript-based generation)
- C# loads generated themes via `GD.Load<Theme>()`
- C# handles runtime switching and per-node overrides
- Separation of concerns: GDScript for generation, C# for runtime logic

### Common Pitfalls:
1. **Infinite Loop**: Calling `AddStyleboxOverride()` inside `_Notification(NOTIFICATION_THEME_CHANGED)` causes stack overflow
2. **Shared Resources**: Always duplicate before modifying theme resources
3. **Override Priority**: Node overrides > Node theme > Parent theme > Project default

---
## Search 5: Theme Type Variations System ✓

**Source**: [Godot Official Docs - Theme Type Variations](https://docs.godotengine.org/en/stable/tutorials/ui/gui_theme_type_variations.html)

### What Are Type Variations?
Theme type variations extend a base type (e.g., Button) to create specialized variants (e.g., GrayButton, AccentButton) without duplicating all properties. They inherit from the base and override only specific properties.

### Why Use Type Variations?
**Problem**: Setting theme overrides on individual nodes is tedious and hard to manage.
**Solution**: Define reusable variants in the theme once, apply them via `Theme Type Variation` property.

### Creating Type Variations:

#### In Theme Editor (GUI):
1. Open theme resource in theme editor
2. Click "+" next to Type dropdown
3. Name your variation (e.g., "AccentButton")
4. Switch to wrench/screwdriver tab
5. Set "Base Type" (e.g., "Button")
6. Edit properties - only override what's different

#### In ThemeGen (Code):
```gdscript
define_variant_style("AccentButton", "Button", {
    normal = stylebox_flat({ bg_color = accent_color }),
    font_color = Color.WHITE
})
```

### Using Type Variations:

#### In Inspector:
- Select node → Theme → Theme Type Variation → Enter "AccentButton"
- Dropdown shows project-wide theme variations
- Manual input for non-project themes

#### In C#:
```csharp
button.ThemeTypeVariation = "AccentButton";
```

### Inheritance Chain:
Type variations can chain:
- `CheckButton` → `Button` → `BaseButton` → `Control`
- Custom: `DangerButton` → `AccentButton` → `Button`

### Property Resolution Priority:
1. Node theme overrides (highest)
2. Type variation properties
3. Base type properties
4. Parent theme
5. Project default theme
6. Godot default (lowest)

### Best Practice Workflow:
1. Define base styles for all standard controls (Button, Label, etc.)
2. Create type variations for specialized use cases (AccentButton, TitleLabel)
3. Apply variations via `Theme Type Variation` property
4. Use node overrides only for truly unique cases

### Integration with Project Architecture:
- **ThemeGen**: Generate base types + variations programmatically
- **C# Helpers**: Runtime switching between theme variations
- **Component System**: Theme variations align with component-based UI design

---
## Final Synthesis & Recommendations


### Comprehensive Understanding:

#### ThemeGen Plugin Architecture:
1. **Base Class**: `ProgrammaticTheme` (extends EditorScript with @tool)
2. **Generation Flow**: `setup_*()` → `define_theme()` → `.tres` output
3. **Helper System**: Shortcut functions for repetitive properties
4. **Live Preview**: Auto-regeneration on save via Save Sync plugin
5. **Multi-Variant**: Multiple `setup_*()` functions = multiple themes

#### Current Project Implementation:
- **Location**: `3d-practice/A1UIResources/ThemeGen/`
- **Themes**: modern_game, light_minimal, dark_elegant, fantasy_rpg
- **Output**: `generated/*.tres` files
- **Features Used**: 
  - `UPDATE_ON_SAVE = true`
  - `VERBOSITY = Verbosity.QUIET`
  - Helper functions: `stylebox_flat()`, `inherit()`, `corner_radius()`, etc.
  - Variant system: AccentButton, ElevatedPanel, AccentLabel

#### Integration Points:

**GDScript (ThemeGen) ↔ C# (Runtime)**:
```
ThemeGen (.gd) → Generate → .tres files
                              ↓
C# Code → GD.Load<Theme>() → Runtime switching
                              ↓
C# Helpers → AddThemeOverride() → Per-node customization
```

#### Recommended Workflow:

1. **Theme Definition** (GDScript/ThemeGen):
   - Define color palettes in `setup_*()`
   - Create base styles in `define_theme()`
   - Generate type variations for specialized controls
   - Run Ctrl+Shift+X or enable Save Sync

2. **Theme Management** (C#):
   - Load generated themes: `GD.Load<Theme>("res://path/to/theme.tres")`
   - Global switching: `GetTree().Root.Theme = theme`
   - Section switching: `uiRoot.Theme = theme`

3. **Runtime Customization** (C#):
   - Apply type variations: `control.ThemeTypeVariation = "AccentButton"`
   - Per-node overrides: `control.AddThemeStyleboxOverride()`
   - Always duplicate before modifying: `stylebox.Duplicate()`

#### Best Practices for This Project:

✓ **DO**:
- Use ThemeGen for all theme generation (leverage existing setup)
- Create type variations for reusable UI patterns
- Use C# for runtime theme switching logic
- Split themes by concern (base, game UI, menu UI)
- Leverage inheritance hierarchy (90% base, 10% overrides)

✗ **DON'T**:
- Manually edit generated `.tres` files (will be overwritten)
- Use excessive per-node overrides (defeats theme purpose)
- Directly modify shared theme resources (always duplicate)
- Mix theme definition logic between GDScript and C#

#### Performance Notes:
- Theme switching is lightweight (single property assignment)
- Type variations have no runtime overhead
- Per-node overrides scale linearly with node count
- Preload themes for instant switching

---
## Research Complete ✓

**Total Searches**: 5/5
**Coverage**: Plugin identity, API reference, best practices, C# integration, type variations
**Next Step**: Create rule file in `KiroWorkingSpace/.kiro/steering/`
