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
	set_save_path("res://A1UIResources/ThemeGen/generated/modern_game_theme.tres")

func define_theme():
	define_default_font_size(default_font_size)
	
	# Button styles
	var button_normal = stylebox_flat({
		bg_color = background_light,
		border_color = primary_color,
		border_ = border_width(2),
		corner_ = corner_radius(8),
		content_ = content_margins(16, 8, 16, 8)
	})
	
	var button_hover = inherit(button_normal, {
		bg_color = primary_color.darkened(0.3),
		border_color = primary_color.lightened(0.2)
	})
	
	var button_pressed = inherit(button_normal, {
		bg_color = primary_color.darkened(0.5),
		content_ = content_margins(16, 10, 16, 6)  # Pressed effect
	})
	
	var button_disabled = inherit(button_normal, {
		bg_color = background_medium,
		border_color = text_dim
	})
	
	define_style("Button", {
		normal = button_normal,
		hover = button_hover,
		pressed = button_pressed,
		disabled = button_disabled,
		font_color = text_color,
		font_hover_color = Color.WHITE,
		font_pressed_color = Color.WHITE,
		font_disabled_color = text_dim
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
