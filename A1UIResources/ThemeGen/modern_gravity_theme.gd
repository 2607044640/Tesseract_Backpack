@tool
extends ProgrammaticTheme

const UPDATE_ON_SAVE = true
const VERBOSITY = Verbosity.QUIET

# Modern Game Theme - Dark with vibrant accents
var primary_color = Color("#00d4ff")  # Cyan accent
var secondary_color = Color("#ff6b35")  # Orange accent
var background_dark = Color("#1a1a2e")
var background_medium = Color("#16213e")
var background_light = Color("#0f3460")
var text_color = Color("#e8e8e8")
var text_dim = Color("#a0a0a0")
var success_color = Color("#00ff88")
var warning_color = Color("#ffaa00")
var danger_color = Color("#ff3366")

var default_font_size = 16
var title_font_size = 24
var small_font_size = 12

func setup():
	set_save_path("res://A1UIResources/ThemeGen/generated/modern_gravity_theme.tres")

func define_theme():
	define_default_font_size(default_font_size)
	
	# Button styles - Gravity.tres style with transparent backgrounds + glow
	var button_normal = stylebox_flat({
		bg_color = Color(0.0, 0.0, 0.0, 0.2),  # Transparent black
		border_color = Color(0.95, 0.95, 0.95, 0.6),  # Light border
		border_ = border_width(2),
		corner_ = corner_radius(8),
		content_ = content_margins(16, 4, 16, 4),
		corner_detail = 1
	})
	
	var button_hover = stylebox_flat({
		bg_color = Color(0.125911, 0.125911, 0.125911, 0.6),  # Semi-transparent dark
		border_color = Color(0.95, 0.95, 0.95, 1.0),  # Bright border
		border_ = border_width(2),
		corner_ = corner_radius(8),
		content_ = content_margins(16, 4, 16, 4),
		corner_detail = 1,
		# WHITE GLOW on hover
		shadow_color = Color(1.0, 1.0, 1.0, 0.6),
		shadow_size = 10,
		shadow_offset = Vector2(0, 0)
	})
	
	var button_pressed = stylebox_flat({
		bg_color = Color(0.95, 0.95, 0.95, 0.9),  # Light flash
		border_color = Color(0.125911, 0.125911, 0.125911, 0.8),  # Dark border
		border_ = border_width(2),
		corner_ = corner_radius(8),
		content_ = content_margins(16, 4, 16, 4),
		corner_detail = 1,
		# Bright glow on press
		shadow_color = Color(0.95, 0.95, 1.0, 0.8),
		shadow_size = 12,
		shadow_offset = Vector2(0, 0)
	})
	
	var button_disabled = stylebox_flat({
		bg_color = Color(0.0, 0.0, 0.0, 0.1),
		border_color = Color(0.5, 0.5, 0.5, 0.3),
		border_ = border_width(2),
		corner_ = corner_radius(8),
		content_ = content_margins(16, 4, 16, 4),
		corner_detail = 1
	})
	
	define_style("Button", {
		normal = button_normal,
		hover = button_hover,
		pressed = button_pressed,
		disabled = button_disabled,
		font_color = Color(0.95, 0.95, 0.95, 1.0),  # Light text
		font_hover_color = Color(0.95, 0.95, 0.95, 1.0),
		font_pressed_color = Color(0.125911, 0.125911, 0.125911, 1.0),  # Dark text on press (inversion)
		font_disabled_color = Color(0.5, 0.5, 0.5, 0.5),
		font_outline_color = Color(0.0, 0.0, 0.0, 0.8),  # Black outline
		font_outline_size = 2
	})
	
	# Panel styles
	define_style("PanelContainer", {
		panel = stylebox_flat({
			bg_color = background_dark,
			border_color = primary_color.darkened(0.5),
			border_ = border_width(1),
			corner_ = corner_radius(12),
			content_ = content_margins(16, 16, 16, 16)
		})
	})
	
	define_style("Panel", {
		panel = stylebox_flat({
			bg_color = background_medium,
			corner_ = corner_radius(8)
		})
	})
	
	# Label styles
	define_style("Label", {
		font_color = text_color,
		font_shadow_color = Color(0, 0, 0, 0.5),
		shadow_offset_x = 1,
		shadow_offset_y = 1
	})
	
	# Title variant
	define_variant_style("TitleLabel", "Label", {
		font_size = title_font_size,
		font_color = primary_color
	})
	
	# LineEdit (text input)
	var lineedit_normal = stylebox_flat({
		bg_color = background_medium,
		border_color = background_light,
		border_ = border_width(2),
		corner_ = corner_radius(6),
		content_ = content_margins(8, 6, 8, 6)
	})
	
	define_style("LineEdit", {
		normal = lineedit_normal,
		focus = inherit(lineedit_normal, {
			border_color = primary_color
		}),
		font_color = text_color,
		font_placeholder_color = text_dim,
		caret_color = primary_color
	})
	
	# ProgressBar
	var progress_bg = stylebox_flat({
		bg_color = background_medium,
		corner_ = corner_radius(4)
	})
	
	var progress_fill = stylebox_flat({
		bg_color = primary_color,
		corner_ = corner_radius(4)
	})
	
	define_style("ProgressBar", {
		background = progress_bg,
		fill = progress_fill,
		font_color = text_color
	})
	
	# CheckBox
	var checkbox_unchecked = stylebox_flat({
		bg_color = background_medium,
		border_color = primary_color,
		border_ = border_width(2),
		corner_ = corner_radius(4)
	})
	
	define_style("CheckBox", {
		normal = checkbox_unchecked,
		hover = inherit(checkbox_unchecked, {
			border_color = primary_color.lightened(0.2)
		}),
		font_color = text_color
	})
