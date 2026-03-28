@tool
extends ProgrammaticTheme

const UPDATE_ON_SAVE = true
const VERBOSITY = Verbosity.QUIET

# Dark Elegant Theme - Professional dark theme
var accent_purple = Color("#bb86fc")
var accent_teal = Color("#03dac6")
var background_darkest = Color("#121212")
var background_dark = Color("#1e1e1e")
var background_elevated = Color("#2d2d2d")
var text_high = Color("#e1e1e1")
var text_medium = Color("#b3b3b3")
var text_disabled = Color("#6b6b6b")
var error_color = Color("#cf6679")

var default_font_size = 16

func setup():
	set_save_path("res://A1UIResources/ThemeGen/generated/dark_elegant_theme.tres")

func define_theme():
	define_default_font_size(default_font_size)
	
	# Button with elevation effect
	var button_normal = stylebox_flat({
		bg_color = background_elevated,
		corner_ = corner_radius(6),
		content_ = content_margins(18, 10, 18, 10),
		shadow_color = Color(0, 0, 0, 0.4),
		shadow_size = 4,
		shadow_offset = Vector2(0, 2)
	})
	
	var button_hover = inherit(button_normal, {
		bg_color = background_elevated.lightened(0.1),
		shadow_size = 6,
		shadow_offset = Vector2(0, 3)
	})
	
	var button_pressed = inherit(button_normal, {
		bg_color = background_elevated.darkened(0.1),
		shadow_size = 2,
		shadow_offset = Vector2(0, 1)
	})
	
	define_style("Button", {
		normal = button_normal,
		hover = button_hover,
		pressed = button_pressed,
		font_color = text_high,
		font_hover_color = accent_teal
	})
	
	# Accent Button variant
	var accent_button = stylebox_flat({
		bg_color = accent_purple,
		corner_ = corner_radius(6),
		content_ = content_margins(18, 10, 18, 10),
		shadow_color = accent_purple.darkened(0.5),
		shadow_size = 4,
		shadow_offset = Vector2(0, 2)
	})
	
	define_variant_style("AccentButton", "Button", {
		normal = accent_button,
		hover = inherit(accent_button, {
			bg_color = accent_purple.lightened(0.1)
		}),
		pressed = inherit(accent_button, {
			bg_color = accent_purple.darkened(0.1)
		}),
		font_color = Color("#000000")
	})
	
	# Panel with subtle border
	define_style("PanelContainer", {
		panel = stylebox_flat({
			bg_color = background_dark,
			border_color = background_elevated,
			border_ = border_width(1),
			corner_ = corner_radius(10),
			content_ = content_margins(20, 20, 20, 20)
		})
	})
	
	# Elevated Panel variant
	define_variant_style("ElevatedPanel", "PanelContainer", {
		panel = stylebox_flat({
			bg_color = background_elevated,
			corner_ = corner_radius(10),
			content_ = content_margins(20, 20, 20, 20),
			shadow_color = Color(0, 0, 0, 0.5),
			shadow_size = 8
		})
	})
	
	# Label
	define_style("Label", {
		font_color = text_high,
		font_shadow_color = Color(0, 0, 0, 0.3),
		shadow_offset_x = 1,
		shadow_offset_y = 1
	})
	
	define_variant_style("AccentLabel", "Label", {
		font_color = accent_purple
	})
	
	# LineEdit with glow on focus
	var lineedit_normal = stylebox_flat({
		bg_color = background_darkest,
		border_color = background_elevated,
		border_ = border_width(2),
		corner_ = corner_radius(6),
		content_ = content_margins(10, 8, 10, 8)
	})
	
	var lineedit_focus = inherit(lineedit_normal, {
		border_color = accent_teal,
		shadow_color = accent_teal.darkened(0.5),
		shadow_size = 4
	})
	
	define_style("LineEdit", {
		normal = lineedit_normal,
		focus = lineedit_focus,
		font_color = text_high,
		font_placeholder_color = text_medium,
		caret_color = accent_teal,
		selection_color = accent_purple.darkened(0.5)
	})
	
	# ProgressBar with gradient effect
	define_style("ProgressBar", {
		background = stylebox_flat({
			bg_color = background_darkest,
			corner_ = corner_radius(8)
		}),
		fill = stylebox_flat({
			bg_color = accent_purple,
			corner_ = corner_radius(8)
		}),
		font_color = text_high
	})
	
	# CheckBox
	var checkbox_normal = stylebox_flat({
		bg_color = background_darkest,
		border_color = text_medium,
		border_ = border_width(2),
		corner_ = corner_radius(4)
	})
	
	var checkbox_checked = stylebox_flat({
		bg_color = accent_purple,
		border_color = accent_purple,
		border_ = border_width(2),
		corner_ = corner_radius(4)
	})
	
	define_style("CheckBox", {
		normal = checkbox_normal,
		hover = inherit(checkbox_normal, {
			border_color = accent_teal
		}),
		checked = checkbox_checked,
		font_color = text_high
	})
	
	# ScrollBar
	define_style("VScrollBar", {
		scroll = stylebox_flat({
			bg_color = background_darkest
		}),
		grabber = stylebox_flat({
			bg_color = background_elevated,
			corner_ = corner_radius(4)
		}),
		grabber_highlight = stylebox_flat({
			bg_color = accent_purple.darkened(0.3),
			corner_ = corner_radius(4)
		})
	})
