@tool
extends ProgrammaticTheme

const UPDATE_ON_SAVE = true
const VERBOSITY = Verbosity.QUIET

# Minimal Light Theme - Clean and simple
var primary_color = Color("#2196f3")  # Material Blue
var accent_color = Color("#ff5722")  # Deep Orange
var background = Color("#fafafa")
var surface = Color("#ffffff")
var border_color = Color("#e0e0e0")
var text_primary = Color("#212121")
var text_secondary = Color("#757575")
var text_disabled = Color("#bdbdbd")

var default_font_size = 15
var title_font_size = 20

func setup():
	set_save_path("res://A1UIResources/ThemeGen/generated/light_minimal_theme.tres")

func define_theme():
	define_default_font_size(default_font_size)
	
	# Button - Flat design
	var button_normal = stylebox_flat({
		bg_color = primary_color,
		corner_ = corner_radius(4),
		content_ = content_margins(20, 10, 20, 10)
	})
	
	var button_hover = inherit(button_normal, {
		bg_color = primary_color.lightened(0.1)
	})
	
	var button_pressed = inherit(button_normal, {
		bg_color = primary_color.darkened(0.1)
	})
	
	define_style("Button", {
		normal = button_normal,
		hover = button_hover,
		pressed = button_pressed,
		font_color = Color.WHITE,
		font_size = default_font_size
	})
	
	# Flat Button variant (no background)
	var flat_button_normal = stylebox_flat({
		bg_color = Color.TRANSPARENT,
		border_color = primary_color,
		border_ = border_width(2),
		corner_ = corner_radius(4),
		content_ = content_margins(16, 8, 16, 8)
	})
	
	define_variant_style("FlatButton", "Button", {
		normal = flat_button_normal,
		hover = inherit(flat_button_normal, {
			bg_color = primary_color.lightened(0.8)
		}),
		pressed = inherit(flat_button_normal, {
			bg_color = primary_color.lightened(0.6)
		}),
		font_color = primary_color
	})
	
	# Panel
	define_style("PanelContainer", {
		panel = stylebox_flat({
			bg_color = surface,
			border_color = border_color,
			border_ = border_width(1),
			corner_ = corner_radius(8),
			content_ = content_margins(16, 16, 16, 16)
		})
	})
	
	define_style("Panel", {
		panel = stylebox_flat({
			bg_color = surface,
			corner_ = corner_radius(4)
		})
	})
	
	# Label
	define_style("Label", {
		font_color = text_primary,
		line_spacing = 2
	})
	
	define_variant_style("TitleLabel", "Label", {
		font_size = title_font_size,
		font_color = text_primary
	})
	
	define_variant_style("SubtitleLabel", "Label", {
		font_color = text_secondary
	})
	
	# LineEdit
	var lineedit_normal = stylebox_flat({
		bg_color = surface,
		border_color = border_color,
		border_ = border_width(0, 0, 0, 2),  # Bottom border only
		content_ = content_margins(8, 8, 8, 8)
	})
	
	define_style("LineEdit", {
		normal = lineedit_normal,
		focus = inherit(lineedit_normal, {
			border_color = primary_color
		}),
		font_color = text_primary,
		font_placeholder_color = text_secondary,
		caret_color = primary_color
	})
	
	# ProgressBar
	define_style("ProgressBar", {
		background = stylebox_flat({
			bg_color = border_color,
			corner_ = corner_radius(2)
		}),
		fill = stylebox_flat({
			bg_color = primary_color,
			corner_ = corner_radius(2)
		}),
		font_color = text_primary
	})
	
	# TabContainer
	var tab_selected = stylebox_flat({
		bg_color = surface,
		border_color = primary_color,
		border_ = border_width(0, 0, 3, 0),  # Bottom border
		content_ = content_margins(12, 8, 12, 8)
	})
	
	var tab_unselected = stylebox_flat({
		bg_color = background,
		content_ = content_margins(12, 8, 12, 8)
	})
	
	define_style("TabContainer", {
		tab_selected = tab_selected,
		tab_unselected = tab_unselected,
		font_selected_color = primary_color,
		font_unselected_color = text_secondary
	})
