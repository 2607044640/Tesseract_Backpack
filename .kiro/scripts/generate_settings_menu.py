"""
Generate Settings Menu Scene with TabContainer
"""
import sys
sys.path.append("c:/Godot/KiroWorkingSpace/.kiro/scripts/ui_builder")

from godot_ui_builder import UIBuilder, UINode

# Extend UINode to add TabContainer support
def add_tab_container(self, name: str):
    """Add TabContainer"""
    node = UINode(name, "TabContainer")
    node.properties["layout_mode"] = 2
    node.properties["size_flags_vertical"] = 3  # Fill + Expand
    return self._add_child(node)

def add_tab(self, name: str):
    """Add a tab (MarginContainer) to TabContainer"""
    node = UINode(name, "MarginContainer", auto_suffix=False)
    node.properties["layout_mode"] = 2
    return self._add_child(node)

# Monkey patch the methods
UINode.add_tab_container = add_tab_container
UINode.add_tab = add_tab

# Create the settings menu
ui = UIBuilder("SettingsMenu", scene_uid="uid://c8j3k9m2n5p7q")
root = ui.create_control("SettingsMenu", fullscreen=True)

# Background
bg = root.add_color_rect("Background", color=(0.1, 0.1, 0.12, 1), use_anchors=True)

# Main margin
margin = root.add_margin_container("MainMargin", uniform=40, use_anchors=True, script=None)
margin.set_property("anchors_preset", 15)
margin.set_property("anchor_right", 1.0)
margin.set_property("anchor_bottom", 1.0)
margin.set_property("grow_horizontal", 2)
margin.set_property("grow_vertical", 2)

# Main VBox
main_vbox = margin.add_vbox("MainVBox", separation=20)

# Title
title = main_vbox.add_label("Title", text="Settings", align="center", font_size=48)

# TabContainer
tabs = main_vbox.add_tab_container("Tabs")

# === AUDIO TAB ===
audio_tab = tabs.add_tab("Audio")
audio_margin = audio_tab.add_margin_container("AudioMargin", uniform=20, script=None)
audio_vbox = audio_margin.add_vbox("AudioContent", separation=15)

# Audio sliders
audio_vbox.add_instance("MasterVolume", 
                       scene_path="res://A1UIScenes/UIComponents/SliderComponent.tscn",
                       scene_uid="uid://dbaix0lcy10v2")

audio_vbox.add_instance("MusicVolume", 
                       scene_path="res://A1UIScenes/UIComponents/SliderComponent.tscn",
                       scene_uid="uid://dbaix0lcy10v2")

audio_vbox.add_instance("SFXVolume", 
                       scene_path="res://A1UIScenes/UIComponents/SliderComponent.tscn",
                       scene_uid="uid://dbaix0lcy10v2")

# Mute toggle
audio_vbox.add_instance("Mute", 
                       scene_path="res://A1UIScenes/UIComponents/ToggleComponent.tscn",
                       scene_uid="uid://dpf5ovda3xlpv")

# === VIDEO TAB ===
video_tab = tabs.add_tab("Video")
video_margin = video_tab.add_margin_container("VideoMargin", uniform=20, script=None)
video_vbox = video_margin.add_vbox("VideoContent", separation=15)

# Fullscreen toggle
video_vbox.add_instance("Fullscreen", 
                       scene_path="res://A1UIScenes/UIComponents/ToggleComponent.tscn",
                       scene_uid="uid://dpf5ovda3xlpv")

# Resolution dropdown
video_vbox.add_instance("Resolution", 
                       scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                       scene_uid="uid://5b9ifgnj5kmv5d")

# Anti-Aliasing dropdown
video_vbox.add_instance("AntiAliasing", 
                       scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                       scene_uid="uid://5b9ifgnj5kmv5d")

# Camera Shake dropdown
video_vbox.add_instance("CameraShake", 
                       scene_path="res://A1UIScenes/UIComponents/DropdownComponent.tscn",
                       scene_uid="uid://5b9ifgnj5kmv5d")

# === GAME TAB ===
game_tab = tabs.add_tab("Game")
game_margin = game_tab.add_margin_container("GameMargin", uniform=20, script=None)
game_vbox = game_margin.add_vbox("GameContent", separation=15)
game_vbox.add_label("Placeholder", text="Game settings coming soon...", align="center")

# Back button
back_btn = main_vbox.add_button("BackButton", text="Back", size_flags_h=4)

# Generate and save
print("\n=== Settings Menu Structure ===")
print(ui.generate_tree_view())
print("\n=== Generating .tscn file ===")

output_path = "c:/Godot/3d-practice/A1UIScenes/SettingsMenu.tscn"
ui.save(output_path)

print(f"\n✅ Settings menu created at: {output_path}")
print("\nNext steps:")
print("1. Open the scene in Godot editor")
print("2. Configure each component's properties (LabelText, DefaultValue, etc.)")
print("3. Create SettingsMenu.cs to handle events")
