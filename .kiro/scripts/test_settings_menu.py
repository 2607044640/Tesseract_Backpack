"""
Create a simple test scene to launch the settings menu
"""
import sys
sys.path.append("c:/Godot/KiroWorkingSpace/.kiro/scripts/ui_builder")

from godot_ui_builder import UIBuilder

# Create test scene
ui = UIBuilder("TestSettingsMenu", scene_uid="uid://test123settings")
root = ui.create_control("TestScene", fullscreen=True)

# Add settings menu instance
settings = root.add_instance("SettingsMenu",
                            scene_path="res://A1UIScenes/SettingsMenu.tscn",
                            scene_uid="uid://c8j3k9m2n5p7q")

# Generate and save
print("\n=== Test Scene Structure ===")
print(ui.generate_tree_view())

output_path = "c:/Godot/3d-practice/A1UIScenes/TestSettingsMenu.tscn"
ui.save(output_path)

print(f"\n✅ Test scene created at: {output_path}")
print("\nTo test:")
print("1. Open Godot editor")
print("2. Run TestSettingsMenu.tscn (F6)")
print("3. Test the Audio and Video tabs")
print("4. Check console output for event logs")
