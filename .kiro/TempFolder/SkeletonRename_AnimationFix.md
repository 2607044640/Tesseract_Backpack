# Skeleton Rename Animation Track Path Fix

## Problem
When renaming Skeleton3D in Godot editor (e.g., "Skeleton3D" → "GeneralSkeleton"), animation track paths don't auto-update, causing "couldn't resolve track" warnings and animations fail to play.

## Root Cause
- In Godot 4.1: Renaming skeleton auto-updated animation track paths
- In Godot 4.3+: This auto-update functionality was lost (regression)
- Animation tracks still reference old skeleton name in their NodePaths
- Example: `rig/Skeleton3D:mixamorig_Hips` should be `rig/GeneralSkeleton:mixamorig_Hips`

## Solutions

### Solution 1: Manual Text Replacement (Quick Fix)
Source: [Godot Forum - DarkPhoenix](https://forum.godotengine.org/t/renamed-skeleton-how-to-change-target-skeleton-in-animationplayer/77730)

1. Open AnimationLibrary `.tres` file with text editor (Notepad)
2. Find and replace all instances: "Skeleton3D" → "GeneralSkeleton"
3. Save and reload in Godot

**Pros**: Fast for one-time fix
**Cons**: Must repeat after every reimport

### Solution 2: EditorScenePostImport Script (Automated)
Source: [Godot Forum - mrcdk](https://forum.godotengine.org/t/change-path-to-object-inside-animationplayer/62572)

Use `Animation.track_set_path()` in import script to automatically fix paths on every reimport.

**Implementation**:
```gdscript
func fix_animation_paths(anim_player: AnimationPlayer):
    for anim_name in anim_player.get_animation_list():
        var anim = anim_player.get_animation(anim_name)
        for track_idx in range(anim.get_track_count()):
            var track_path = anim.track_get_path(track_idx)
            var path_string = str(track_path)
            
            # Replace old skeleton name with new name
            if "Skeleton3D" in path_string:
                var new_path = path_string.replace("Skeleton3D", "GeneralSkeleton")
                anim.track_set_path(track_idx, NodePath(new_path))
                print("Fixed track: ", path_string, " → ", new_path)
```

**Pros**: Automatic on every reimport, no manual work
**Cons**: Requires script modification

## Chosen Solution
Use Solution 2 (EditorScenePostImport) for automatic fixing in `sophia_import.gd`

## References
1. https://forum.godotengine.org/t/renamed-skeleton-how-to-change-target-skeleton-in-animationplayer/77730
2. https://forum.godotengine.org/t/change-path-to-object-inside-animationplayer/62572
3. https://forum.godotengine.org/t/adding-an-animation-to-the-animationplayer-via-code/50043
