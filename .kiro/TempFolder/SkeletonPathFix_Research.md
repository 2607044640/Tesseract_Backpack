# Skeleton Path Fix Research - Animation Track Remapping

## Problem
Mixamo animations imported into Godot 4 don't play because skeleton bone paths in animations don't match the actual skeleton node path in the scene. User renamed skeleton to "GeneralSkeleton" but animations still reference old paths like "RootNode/Armature/Skeleton3D".

## Solution: EditorScenePostImport with track_set_path()

### Key Findings from Web Research

**Source 1: Godot Forum - Change path to object inside AnimationPlayer**
URL: https://forum.godotengine.org/t/change-path-to-object-inside-animationplayer/62572

Key Quote: "As a workaround, you could write an EditorScenePostImport script and use that script to edit the Animation Resource directly changing the track NodePath to the correct one with Animation.track_set_path()"

Context: User importing Mixamo animations had wrong paths (RootNode/Armature/Skeleton3D) and needed to manually edit each track. Solution is to use EditorScenePostImport script to automate the path correction.

**Source 2: Godot Official Docs - EditorScenePostImport**
URL: https://docs.godotengine.org/en/stable/classes/class_editorscenepostimport.html

Key Info:
- Imported scenes can be automatically modified after import
- Set "Custom Script Import" property to a tool script that inherits from EditorScenePostImport
- Implement `_post_import(scene: Node)` callback
- Must return the modified version of the scene

**Source 3: Godot Forum - Custom Import Process (Stepped Animation)**
URL: https://forum.godotengine.org/t/custom-import-process-extension-for-all-assets-stepped-animation-interpolation/113821

Code Pattern (paraphrased for compliance):
```gdscript
func _post_import(scene: Node) -> Object:
    iterate(scene)
    return scene

func iterate(node: Node):
    if node is AnimationPlayer:
        var animation_player = node as AnimationPlayer
        for anim_name in animation_player.get_animation_list():
            var animation = animation_player.get_animation(anim_name)
            # Process animation tracks here
    for child in node.get_children():
        iterate(child)
```

## Implementation Strategy

### Step 1: Find AnimationPlayer in imported scene
Recursively search the scene tree for AnimationPlayer nodes.

### Step 2: Iterate through all animations
For each animation in the AnimationPlayer, get the animation resource.

### Step 3: Fix track paths
For each track in the animation:
- Get current track path with `animation.track_get_path(track_idx)`
- Check if path contains old skeleton reference
- Replace with correct path using `animation.track_set_path(track_idx, new_path)`

### Step 4: Path remapping logic
Need to remap paths like:
- FROM: `Armature/Skeleton3D:bone_name` or `RootNode/Armature/Skeleton3D:bone_name`
- TO: `GeneralSkeleton:bone_name` (or whatever the actual skeleton path is)

### Key API Methods
- `animation.track_get_count()` - Get number of tracks
- `animation.track_get_path(idx)` - Get NodePath for track
- `animation.track_set_path(idx, path)` - Set new NodePath for track
- `NodePath` string format: "node/path:property" for bone animations

## Next Steps
1. Read current sophia_import.gd
2. Add track path remapping logic in _post_import
3. Determine correct skeleton path in scene
4. Test by reimporting sophia.glb


## Implementation Complete

### Changes Made to sophia_import.gd

1. **Added find_skeleton() function**
   - Recursively searches scene tree for Skeleton3D node
   - Returns the actual skeleton instance

2. **Enhanced fix_skeleton_paths() function**
   - Finds actual skeleton in scene and gets its path relative to AnimationPlayer's parent
   - Prints skeleton path for debugging
   - Collects sample track paths (first 3) for inspection
   - Handles multiple skeleton path patterns:
     * Pattern 1: Contains "Skeleton3D"
     * Pattern 2: Contains "Armature" and "Skeleton"
     * Pattern 3: Ends with "Skeleton" or "GeneralSkeleton"
   - Splits track paths by ":" to separate node path from property
   - Reconstructs paths using actual skeleton path found in scene
   - Prints detailed debug info including sample paths and fix count

3. **Better Debugging**
   - Shows actual skeleton path found
   - Displays sample track paths before fixing
   - Reports number of tracks fixed
   - Warns if no fixes were applied

### Testing Instructions

1. In Godot editor, select `player_Sophia/sophia_skin/model/sophia.glb`
2. Go to Import tab
3. Click "Reimport" button
4. Check Output tab for debug messages showing:
   - Skeleton path found
   - Sample track paths
   - Number of tracks fixed
5. Run the game and test animations (Idle, Run, FastRun)

### Expected Output in Console
```
=== Sophia Post-Import: Starting ===
Found AnimationPlayer with X animations
=== Fixing Skeleton Paths ===
Found Skeleton at path: rig/GeneralSkeleton
Processing animation: Idle (X tracks)
Processing animation: Run (X tracks)
...
Sample track paths found:
  - [original path examples]
✓ Fixed X animation tracks to use path: rig/GeneralSkeleton
✓ Saved animation: Idle -> res://player_Sophia/sophia_skin/animations/Idle.res
...
=== Sophia Post-Import: Complete (8/8 saved) ===
```
