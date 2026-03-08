# 动画配置系统使用指南

## 系统概述

这是一个类似UE Data Asset的动画管理系统，支持：
- ✅ 使用.res文件存储动画配置
- ✅ 多角色共享动画配置
- ✅ 动画合集管理（移动、战斗、表情等）
- ✅ 支持日文命名
- ✅ 可视化配置（Inspector面板）

## 核心组件

### 1. AnimationSet（动画合集）
**文件**: `Scripts/Animation/AnimationSet.cs`

动画合集Resource，包含一组相关动画：
- 基础移动：Idle, Walk, Run, Sprint
- 跳跃空中：JumpStart, JumpLoop, Fall, Land
- 战斗：Attack1, Attack2, Attack3
- 其他：Dodge, Hit, Death

### 2. CharacterAnimationConfig（角色动画配置）
**文件**: `Scripts/Animation/CharacterAnimationConfig.cs`

角色的完整动画配置，包含：
- MovementSet（移动动画集）
- CombatSet（战斗动画集）
- EmoteSet（表情动画集）
- AnimationPlayer路径
- 播放设置（混合时间、自动播放等）

## 使用步骤

### 步骤1：创建AnimationSet资源

1. 在Godot编辑器中，右键点击文件系统
2. 选择 `New Resource...`
3. 搜索并选择 `AnimationSet`
4. 保存为 `.res` 文件，例如：`Animations/Kuno_MovementSet.res`

### 步骤2：配置AnimationSet

在Inspector中：
1. 设置 `Set Name`（可以用日文，如"移動アニメーション"）
2. 将动画.res文件拖拽到对应槽位：
   - Idle Animation → 拖入 `Idle.res`
   - Run Animation → 拖入 `Run.res`
   - 等等...

### 步骤3：创建CharacterAnimationConfig

1. 右键 → `New Resource...`
2. 选择 `CharacterAnimationConfig`
3. 保存为 `.res`，例如：`Animations/Kuno_AnimConfig.res`

### 步骤4：配置CharacterAnimationConfig

在Inspector中：
1. `Character Name`: 输入角色名（支持日文，如"クノ"）
2. `Movement Set`: 拖入刚创建的MovementSet.res
3. `Combat Set`: 如果有战斗动画，创建并拖入
4. `Animation Player Path`: 设置为 `"AnimationPlayer"`（或实际路径）
5. `Default Blend Time`: 设置动画混合时间（如0.2秒）
6. `Auto Play`: 勾选
7. `Auto Play Animation`: 设置为 `"Idle"`

### 步骤5：应用到Player3D

1. 选择Player3D场景中的CharacterBody3D节点
2. 在Inspector中找到 `Anim Config` 属性
3. 将创建的CharacterAnimationConfig.res拖入

## 关于AnimationPlayer

### 是否需要AnimationPlayer？
**是的，必须保留！** AnimationPlayer是播放动画的核心组件。

### AnimationPlayer的作用
- 播放动画
- 管理动画混合
- 控制播放速度
- 处理动画事件

### 配置系统如何工作
1. AnimationPlayer保留在角色场景中（如kuno_1_02.tscn）
2. 配置系统会在运行时将.res动画加载到AnimationPlayer
3. 这样多个角色可以共享相同的动画.res文件

## 关于.res动画文件

### 如何获取.res动画？

**方法1：从GLB/FBX导出**
使用EditorScenePostImport脚本自动导出：
```gdscript
# sophia_import.gd 示例
func _post_import(scene: Node) -> Object:
    var anim_player = find_animation_player(scene)
    for anim_name in anim_player.get_animation_list():
        var anim = anim_player.get_animation(anim_name)
        ResourceSaver.save(anim, "res://Animations/" + anim_name + ".res")
    return scene
```

**方法2：手动保存**
1. 在AnimationPlayer中选择动画
2. 右键 → `Save As...`
3. 保存为.res文件

### .res文件的优势
- 可以在多个角色间共享
- 可以独立编辑和版本控制
- 支持运行时动态加载
- 减少场景文件大小

## 修复骨骼路径警告

### 警告原因
```
couldn't resolve track: 'Skeleton3D:mixamorig_RightHandMiddle3'
```
这是因为动画引用了模型中不存在的骨骼（通常是手指细节骨骼）。

### 解决方案：创建Import脚本

创建 `kuno_import.gd`:
```gdscript
@tool
extends EditorScenePostImport

func _post_import(scene: Node) -> Object:
    var anim_player = find_animation_player(scene)
    if anim_player == null:
        return scene
    
    # 获取实际存在的骨骼列表
    var skeleton = find_skeleton(scene)
    if skeleton == null:
        return scene
    
    var valid_bones = []
    for i in range(skeleton.get_bone_count()):
        valid_bones.append(skeleton.get_bone_name(i))
    
    # 清理无效的动画轨道
    for anim_name in anim_player.get_animation_list():
        var anim = anim_player.get_animation(anim_name)
        clean_invalid_tracks(anim, valid_bones)
    
    return scene

func clean_invalid_tracks(anim: Animation, valid_bones: Array):
    for track_idx in range(anim.get_track_count() - 1, -1, -1):
        var track_path = str(anim.track_get_path(track_idx))
        if ":" in track_path:
            var bone_name = track_path.split(":")[1]
            if bone_name not in valid_bones:
                anim.remove_track(track_idx)
                print("Removed invalid track: ", track_path)

func find_animation_player(node: Node) -> AnimationPlayer:
    if node is AnimationPlayer:
        return node
    for child in node.get_children():
        var result = find_animation_player(child)
        if result != null:
            return result
    return null

func find_skeleton(node: Node) -> Skeleton3D:
    if node is Skeleton3D:
        return node
    for child in node.get_children():
        var result = find_skeleton(child)
        if result != null:
            return result
    return null
```

然后在Kuno1.02.fbx的Import设置中：
1. 切换到Import标签
2. `Import Script` → `Path` → 选择 `kuno_import.gd`
3. 点击Reimport

## 示例配置结构

```
Animations/
├── Sets/
│   ├── Kuno_MovementSet.res      # 移動アニメーション
│   ├── Kuno_CombatSet.res        # 戦闘アニメーション
│   └── Sophia_MovementSet.res    # 另一个角色
├── Raw/
│   ├── Idle.res
│   ├── Run.res
│   ├── Jump.res
│   └── Attack1.res
└── Configs/
    ├── Kuno_AnimConfig.res       # クノの設定
    └── Sophia_AnimConfig.res     # ソフィアの設定
```

## 多角色共享示例

```
角色A（Kuno）:
  AnimConfig → Kuno_AnimConfig.res
    MovementSet → Kuno_MovementSet.res
      Idle → Idle.res  ←─┐
      Run → Run.res   ←─┤  共享相同的.res文件
                        │
角色B（Sophia）:         │
  AnimConfig → Sophia_AnimConfig.res
    MovementSet → Sophia_MovementSet.res
      Idle → Idle.res  ←─┘
      Run → Run.res   ←─┘
```

## 日文支持

完全支持日文命名：
- ✅ SetName: "移動アニメーション"
- ✅ CharacterName: "クノ"
- ✅ 文件名: `クノ_移動セット.res`
- ✅ 动画名: "走る", "ジャンプ"

Godot使用UTF-8编码，完全支持Unicode字符。

## 测试

运行游戏后，Console应该显示：
```
AnimationConfig applied. Available animations: Idle, Run, Sprint, Jump, ...
```

如果看到这个，说明配置系统工作正常！
