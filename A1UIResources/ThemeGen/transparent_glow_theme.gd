@tool
extends ProgrammaticTheme

const UPDATE_ON_SAVE = true
const VERBOSITY = Verbosity.QUIET

# Transparent Glow Theme - Based on gravity.tres structure
# Transparent backgrounds + white glow effects + text outlines

# Colors matching gravity.tres values but with transparency
var bg_normal = Color(0.0, 0.0, 0.0, 0.2)                      # Normal: 20% transparent black
var bg_hover = Color(0.125911, 0.125911, 0.125911, 0.6)        # Hover: 60% dark gray
var bg_pressed = Color(0.95, 0.95, 0.95, 0.9)                  # Pressed: 90% white (flash)
var bg_panel = Color(0.0619267, 0.0619267, 0.0619267, 0.4)    # Panel: 40% very dark

# Borders
var border_light = Color(0.95, 0.95, 0.95, 0.6)
var border_bright = Color(0.95, 0.95, 0.95, 1.0)
var border_dark = Color(0.125911, 0.125911, 0.125911, 0.8)

# Text
var text_light = Color(0.95, 0.95, 0.95, 1.0)
var text_dark = Color(0.125911, 0.125911, 0.125911, 1.0)
var text_outline_black = Color(0.0, 0.0, 0.0, 0.8)

# Glow
var glow_white = Color(1.0, 1.0, 1.0, 0.6)
var glow_bright = Color(0.95, 0.95, 1.0, 0.8)

var default_font_size = 16

func setup():
	set_save_path("res://A1UIResources/ThemeGen/generated/transparent_glow_theme.tres")

func define_theme():
	define_default_font_size(default_font_size)
	
	# BUTTON - Exact gravity.tres structure with transparency + glow
	var button_normal = stylebox_flat({
		bg_color = bg_normal,
		border_color = border_light,
		border_ = border_width(2),
		corner_ = corner_radius(8),
		content_ = content_margins(16, 4, 16, 4),
		corner_detail = 1
	})
	
	var button_hover = stylebox_flat({
		bg_color = bg_hover,
		border_color = border_bright,
		border_ = border_width(2),
		corner_ = corner_radius(8),
		content_ = content_margins(16, 4, 16, 4),
		corner_detail = 1,
		# WHITE GLOW on hover
		shadow_color = glow_white,
		shadow_size = 10,
		shadow_offset = Vector2(0, 0)
	})
	
	var button_pressed = stylebox_flat({
		bg_color = bg_pressed,
		border_color = border_dark,
		border_ = border_width(2),
		corner_ = corner_radius(8),
		content_ = content_margins(16, 4, 16, 4),
		corner_detail = 1,
		# Bright flash glow
		shadow_color = glow_bright,
		shadow_size = 12,
		shadow_offset = Vector2(0, 0)
	})
	
	define_style("Button", {
		normal = button_normal,
		hover = button_hover,
		pressed = button_pressed,
		focus = null,
		disabled = null,
		font_color = text_light,
		font_hover_color = text_light,
		font_pressed_color = text_dark,  # Color inversion like gravity.tres
		font_focus_color = text_light,
		font_outline_color = text_outline_black,
		font_outline_size = 2
	})
	
	# PANEL / PANEL CONTAINER
	var panel_style = stylebox_flat({
		bg_color = bg_panel,
		corner_ = corner_radius(4),
		corner_detail = 1
	})
	
	define_style("Panel", {
		panel = panel_style
	})
	
	define_style("PanelContainer", {
		panel = panel_style
	})
	
	# TAB CONTAINER - Exact gravity.tres structure with glow
	var tab_selected = stylebox_flat({
		bg_color = Color(0.945281, 0.945281, 0.945281, 0.9),
		border_color = Color(0.0195315, 0.0195315, 0.0195315, 0.8),
		border_ = border_width(2, 2, 2, 0),
		corner_ = corner_radius(8, 8, 0, 0),
		content_ = content_margins(8, 0, 8, 0),
		corner_detail = 1,
		# Glow on selected
		shadow_color = glow_bright,
		shadow_size = 8,
		shadow_offset = Vector2(0, -2)
	})
	
	var tab_hovered = stylebox_flat({
		bg_color = Color(0.125536, 0.125536, 0.125536, 0.6),
		border_color = Color(0.945281, 0.945281, 0.945281, 1.0),
		border_ = border_width(2, 2, 2, 0),
		corner_ = corner_radius(8, 8, 0, 0),
		content_ = content_margins(8, 0, 8, 0),
		corner_detail = 1,
		# Glow on hover
		shadow_color = glow_white,
		shadow_size = 6,
		shadow_offset = Vector2(0, -1)
	})
	
	var tab_unselected = stylebox_flat({
		bg_color = Color(0.0, 0.0, 0.0, 0.3),
		border_color = Color(0.945281, 0.945281, 0.945281, 0.6),
		border_ = border_width(2, 2, 2, 0),
		corner_ = corner_radius(8, 8, 0, 0),
		content_ = content_margins(8, 0, 8, 0),
		corner_detail = 1
	})
	
	define_style("TabContainer", {
		panel = panel_style,
		tab_selected = tab_selected,
		tab_hovered = tab_hovered,
		tab_unselected = tab_unselected,
		font_selected_color = Color(0.0195315, 0.0195315, 0.0195315, 1.0),
		font_hovered_color = Color(0.945281, 0.945281, 0.945281, 1.0),
		font_unselected_color = Color(0.945281, 0.945281, 0.945281, 1.0),
		font_outline_color = text_outline_black,
		font_outline_size = 1
	})
	
	# LABEL - White text with black outline
	define_style("Label", {
		font_color = text_light,
		font_outline_color = text_outline_black,
		font_outline_size = 2
	})
