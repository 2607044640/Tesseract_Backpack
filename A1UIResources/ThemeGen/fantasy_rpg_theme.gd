@tool
extends ProgrammaticTheme

const UPDATE_ON_SAVE = true
const VERBOSITY = Verbosity.QUIET

# Fantasy RPG Theme - Medieval/Fantasy game style
var gold_color = Color("#ffd700")
var bronze_color = Color("#cd7f32")
var parchment = Color("#f4e8d0")
var dark_wood = Color("#3e2723")
var leather_brown = Color("#5d4037")
var stone_gray = Color("#546e7a")
var magic_blue = Color("#4fc3f7")
var health_red = Color("#e53935")
var mana_blue = Color("#1e88e5")
var text_dark = Color("#1a1a1a")
var text_light = Color("#f5f5f5")

var default_font_size = 16
var title_font_size = 22

func setup():
	set_save_path("res://A1UIResources/ThemeGen/generated/fantasy_rpg_theme.tres")

func define_theme():
	define_default_font_size(default_font_size)
	
	# Button - Medieval style with thick borders
	var button_normal = stylebox_flat({
		bg_color = leather_brown,
		border_color = bronze_color,
		border_ = border_width(3),
		corner_ = corner_radius(2),  # Sharp corners for medieval feel
		content_ = content_margins(20, 12, 20, 12)
	})
	
	var button_hover = inherit(button_normal, {
		bg_color = leather_brown.lightened(0.15),
		border_color = gold_color
	})
	
	var button_pressed = inherit(button_normal, {
		bg_color = leather_brown.darkened(0.2),
		content_ = content_margins(20, 14, 20, 10)  # Pressed effect
	})
	
	define_style("Button", {
		normal = button_normal,
		hover = button_hover,
		pressed = button_pressed,
		font_color = text_light,
		font_hover_color = gold_color
	})
	
	# Gold Button variant (for important actions)
	var gold_button = stylebox_flat({
		bg_color = bronze_color,
		border_color = gold_color,
		border_ = border_width(3),
		corner_ = corner_radius(2),
		content_ = content_margins(20, 12, 20, 12)
	})
	
	define_variant_style("GoldButton", "Button", {
		normal = gold_button,
		hover = inherit(gold_button, {
			bg_color = gold_color.darkened(0.2),
			border_color = gold_color.lightened(0.2)
		}),
		font_color = text_dark
	})
	
	# Panel - Parchment style
	define_style("PanelContainer", {
		panel = stylebox_flat({
			bg_color = parchment,
			border_color = dark_wood,
			border_ = border_width(4),
			corner_ = corner_radius(4),
			content_ = content_margins(20, 20, 20, 20)
		})
	})
	
	# Dark Panel variant (for inventory/equipment)
	define_variant_style("DarkPanel", "PanelContainer", {
		panel = stylebox_flat({
			bg_color = dark_wood,
			border_color = bronze_color,
			border_ = border_width(3),
			corner_ = corner_radius(4),
			content_ = content_margins(16, 16, 16, 16)
		})
	})
	
	# Label
	define_style("Label", {
		font_color = text_dark,
		font_shadow_color = Color(0, 0, 0, 0.3),
		shadow_offset_x = 2,
		shadow_offset_y = 2
	})
	
	# Title Label with gold color
	define_variant_style("TitleLabel", "Label", {
		font_size = title_font_size,
		font_color = gold_color,
		font_outline_color = dark_wood,
		outline_size = 2
	})
	
	# Light Label for dark backgrounds
	define_variant_style("LightLabel", "Label", {
		font_color = text_light
	})
	
	# LineEdit - Carved stone style
	var lineedit_normal = stylebox_flat({
		bg_color = stone_gray.darkened(0.3),
		border_color = dark_wood,
		border_ = border_width(3),
		corner_ = corner_radius(3),
		content_ = content_margins(12, 8, 12, 8)
	})
	
	define_style("LineEdit", {
		normal = lineedit_normal,
		focus = inherit(lineedit_normal, {
			border_color = gold_color
		}),
		font_color = text_light,
		font_placeholder_color = text_light.darkened(0.4),
		caret_color = gold_color
	})
	
	# ProgressBar - Health/Mana bars
	var progress_bg = stylebox_flat({
		bg_color = dark_wood,
		border_color = bronze_color,
		border_ = border_width(2),
		corner_ = corner_radius(3)
	})
	
	var progress_fill = stylebox_flat({
		bg_color = health_red,
		corner_ = corner_radius(2)
	})
	
	define_style("ProgressBar", {
		background = progress_bg,
		fill = progress_fill,
		font_color = text_light
	})
	
	# Health Bar variant
	define_variant_style("HealthBar", "ProgressBar", {
		fill = stylebox_flat({
			bg_color = health_red,
			corner_ = corner_radius(2)
		})
	})
	
	# Mana Bar variant
	define_variant_style("ManaBar", "ProgressBar", {
		fill = stylebox_flat({
			bg_color = mana_blue,
			corner_ = corner_radius(2)
		})
	})
	
	# CheckBox - Stone carved
	var checkbox_normal = stylebox_flat({
		bg_color = stone_gray.darkened(0.3),
		border_color = dark_wood,
		border_ = border_width(3),
		corner_ = corner_radius(2)
	})
	
	var checkbox_checked = stylebox_flat({
		bg_color = bronze_color,
		border_color = gold_color,
		border_ = border_width(3),
		corner_ = corner_radius(2)
	})
	
	define_style("CheckBox", {
		normal = checkbox_normal,
		hover = inherit(checkbox_normal, {
			border_color = bronze_color
		}),
		checked = checkbox_checked,
		font_color = text_light
	})
	
	# TabContainer - Book tabs
	var tab_selected = stylebox_flat({
		bg_color = parchment,
		border_color = dark_wood,
		border_ = border_width(3, 3, 0, 3),  # No bottom border
		corner_ = corner_radius(6, 6, 0, 0),  # Rounded top only
		content_ = content_margins(16, 10, 16, 10)
	})
	
	var tab_unselected = stylebox_flat({
		bg_color = parchment.darkened(0.2),
		border_color = dark_wood,
		border_ = border_width(3, 3, 0, 3),
		corner_ = corner_radius(6, 6, 0, 0),
		content_ = content_margins(16, 10, 16, 10)
	})
	
	define_style("TabContainer", {
		tab_selected = tab_selected,
		tab_unselected = tab_unselected,
		font_selected_color = text_dark,
		font_unselected_color = text_dark.lightened(0.3)
	})
