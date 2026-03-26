"""
Generate New Settings Menu with improved layout
Based on user screenshots showing Audio, Video, Controls, Game tabs
"""
import sys
sys.path.append("c:/Godot/KiroWorkingSpace/.kiro/scripts/ui_builder")

from godot_ui_builder import UIBuilder, UINode

# Extend UINode for TabContainer support
def add_tab_container(self, name: str):
    node = UINode(name, "TabContainer")
    node.properties["layout_mode"] = 2
    node.properties["size_flags_vertical"] = 3
    return self._add_child(node)

def add_tab(self, name: str):
    node = UINode(name, "MarginContainer", auto_suffix=False)
    node.properties["layout_mode"] = 2
    return self._add_child(node)

UINode.add_tab_container = add_tab_container
UINode.add_tab = add_tab

# Create settings menu
ui = UIBuilder("SettingsMenuV2", scene_uid="uid://d2k8m5n7p9q3r")
root = ui.create_control("SettingsMenuV2", fullscreen=True)

# Dark background
bg = root.add_color_rect("Background", color=(0.12, 0.12, 0.12, 1), use_anchors=True)

# Main margin (no script)
margin = root.add_margin_container("MainMargin", uniform=0, use_anchors=True, script=None)
margin.set_property("anchors_preset", 15)
margin.set_property("anchor_right", 1.0)
margin.set_property("anchor_bottom", 1.0)
margin.set_property("grow_horizontal", 2)
margin.set_property("grow_vertical", 2)

# Main VBox
main_vbox = margin.add_vbox("MainVBox", separation=0)

# TabContainer (fills most of the screen)
tabs = main_vbox.add_tab_container("Tabs")

# === AUDIO TAB ===
audio_tab = tabs.add_tab("Audio")
audio_margin = audio_tab.add_margin_container("AudioMargin", uniform=40, script=None)
audio_vbox = audio_margin.add_vbox("AudioContent", separation=25)

# Master volume
audio_vbox.add_instance("MasterVolume", 
                       scene_path="res://A1UIScenes/UIComponents/SliderComponent.tscn",
                       scene_uid="uid://dbaix0lcy10v2")

# Music volume
audio_vbox.add_instance("MusicVolume", 
                       scene_path="res://A1UIScenes/UIComponents/SliderComponent.tscn",
                       scene_uid="uid://dbaix0lcy10v2")

# Effects volume
audio_vbox.add_instance("EffectsVolume", 
                       scene_path="res://A1UIScenes/UIComponents/SliderComponent.tscn",
                       scene_uid="uid://dbaix0lcy10v2")

# Audio toggle
audio_vbox.add_instance("AudioToggle", 
                       scene_path="res://A1UIScenes/UIComponents/ToggleComponent.tscn",
                       scene_uid="uid://dpf5ovda3xlpv")

# === VIDEO TAB ===
video_tab = tabs.add_tab("Video")
video_margin = video_tab.add_margin_container("VideoMargin", uniform=40, script=None)
video_vbox = video_margin.add_vbox("VideoContent", separation=25)

# Display Mode
video_vbox.add_instance("DisplayMode", 
                       scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                       scene_uid="uid://5b9ifgnj5kmv5d")

# Window Resolution
video_vbox.add_instance("WindowResolution", 
                       scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                       scene_uid="uid://5b9ifgnj5kmv5d")

# Window Zoom (slider)
video_vbox.add_instance("WindowZoom", 
                       scene_path="res://A1UIScenes/UIComponents/SliderComponent.tscn",
                       scene_uid="uid://dbaix0lcy10v2")

# VSync
video_vbox.add_instance("VSync", 
                       scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                       scene_uid="uid://5b9ifgnj5kmv5d")

# FPS Limit
video_vbox.add_instance("FPSLimit", 
                       scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                       scene_uid="uid://5b9ifgnj5kmv5d")

# FPS Count
video_vbox.add_instance("FPSCount", 
                       scene_path="res://A1UIScenes/UIComponents/ToggleComponent.tscn",
                       scene_uid="uid://dpf5ovda3xlpv")

# Anti-Alias
video_vbox.add_instance("AntiAlias", 
                       scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                       scene_uid="uid://5b9ifgnj5kmv5d")

# === CONTROLS TAB ===
controls_tab = tabs.add_tab("Controls")
controls_margin = controls_tab.add_margin_container("ControlsMargin", uniform=40, script=None)
controls_vbox = controls_margin.add_vbox("ControlsContent", separation=15)
controls_vbox.add_label("Placeholder", text="Input mapping coming soon...", align="center")

# === GAME TAB ===
game_tab = tabs.add_tab("Game")
game_margin = game_tab.add_margin_container("GameMargin", uniform=40, script=None)
game_vbox = game_margin.add_vbox("GameContent", separation=25)

# Auto-save
game_vbox.add_instance("AutoSave", 
                      scene_path="res://A1UIScenes/UIComponents/ToggleComponent.tscn",
                      scene_uid="uid://dpf5ovda3xlpv")

# Number Format
game_vbox.add_instance("NumberFormat", 
                      scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                      scene_uid="uid://5b9ifgnj5kmv5d")

# Language
game_vbox.add_instance("Language", 
                      scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                      scene_uid="uid://5b9ifgnj5kmv5d")

# Game Mode
game_vbox.add_instance("GameMode", 
                      scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                      scene_uid="uid://5b9ifgnj5kmv5d")

# Bottom buttons row
buttons_margin = main_vbox.add_margin_container("ButtonsMargin", left=40, right=40, bottom=20, script=None)
buttons = buttons_margin.add_hbox("Buttons", separation=20)
buttons.add_button("BackButton", text="Back", size_flags_h=4)
buttons.add_button("ResetButton", text="Reset", size_flags_h=4)

# Generate and save
print("\n=== New Settings Menu Structure ===")
print(ui.generate_tree_view())
print("\n=== Generating .tscn file ===")

output_path = "c:/Godot/3d-practice/A1UIScenes/SettingsMenuV2.tscn"
ui.save(output_path)

print(f"\n✅ New settings menu created at: {output_path}")
print("\nNext steps:")
print("1. Open SettingsMenuV2.tscn in Godot")
print("2. Configure component properties in Inspector")
print("3. Create SettingsMenuV2.cs controller")
print("4. Test the new layout")
