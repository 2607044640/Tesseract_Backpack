# Mixamo Animation Retargeter插件问题研究

## 问题描述
用户安装了"Mixamo Animation Retargeter"插件，右键点击FBX文件选择"Retarget Mixamo Animation(s)"没有任何反应。

## 网页搜索结果

### Source 1: GitHub - RaidTheory/Godot-Mixamo-Animation-Retargeter
URL: https://github.com/RaidTheory/Godot-Mixamo-Animation-Retargeter

**插件要求（改写以符合合规要求）：**
- 需要Godot 4.3版本
- 角色模型必须有名为"Skeleton"的Skeleton3D节点
- 角色骨骼必须使用Bone Mapping进行retarget
- 使用ufbx导入FBX文件
- 动画和骨骼必须共享相同的骨骼名称

**使用步骤：**
1. 使用ufbx导入Mixamo FBX文件
2. 右键点击FBX文件
3. 选择"Retarget Mixamo Animation"
4. 选择导出目标文件夹
5. 插件会自动更新导入设置并保存为.res文件

### Source 2: Godot Forum - Synty Studio Characters with Mixamo Animations
URL: https://forum.godotengine.org/t/synty-studio-characters-with-mixamo-animations/110613

**关键发现：**
引用原文："change Skeleton Name from GeneralSkeleton to Skeleton. This last step is optional but since I am using Plugin called 'Mixamo Animation Retargeter' it is mandatory."

**重要要求：**
- 必须将Skeleton3D节点名称改为"Skeleton"
- 这是使用Mixamo Animation Retargeter插件的强制要求
- 需要在Import设置中配置Bone Map
- 配置后必须Reimport

### Source 3: Godot Asset Library - Mixamo Animation Retargeter
URL: https://godotassetlibrary.com/asset/MlR3C6/mixamo-animation-retargeter

**插件功能：**
- 简化Godot 4.3项目中Mixamo动画的导入和retarget过程
- 自动retarget并提取动画资源用于AnimationLibrary
- 支持批量处理多个FBX文件

## 问题原因分析

### 1. 版本兼容性问题
- 插件设计用于Godot 4.3
- 用户使用Godot 4.6.1
- 可能存在API变化导致插件不工作

### 2. Skeleton名称不匹配 ⭐ 最可能的原因
- 插件要求Skeleton3D节点必须命名为"Skeleton"
- 用户的Kuno模型可能使用其他名称（如"Skeleton3D"或"GeneralSkeleton"）
- 这是插件的硬性要求

### 3. 缺少Bone Map配置
- 插件要求角色骨骼必须配置Bone Map
- 需要使用SkeletonProfileHumanoid
- 骨骼必须retarget到标准Godot骨骼名称

### 4. 导入器问题
- 插件可能要求使用ufbx导入器
- 如果使用其他导入器可能不工作

### 5. 插件未正确启用
- 需要在Project Settings -> Plugins中启用
- 可能需要重启编辑器

## 解决方案

### 方案1：修复Skeleton名称（推荐）

1. 选择Kuno1.02.fbx文件
2. 切换到Import标签
3. 展开Nodes部分
4. 找到Skeleton3D节点
5. 在"Unique Node Name"中改为"Skeleton"
6. 点击Reimport

### 方案2：配置Bone Map

1. 在Import设置中
2. 展开Retarget部分
3. 添加Bone Map
4. 选择SkeletonProfileHumanoid
5. 确保所有骨骼都映射（全绿点）
6. Reimport

### 方案3：检查插件状态

1. Project -> Project Settings
2. 切换到Plugins标签
3. 确认"Mixamo Animation Retargeter"已启用
4. 如果刚启用，重启Godot编辑器

### 方案4：使用EditorScenePostImport替代（推荐）

如果插件继续不工作，使用自定义Import脚本：

```gdscript
@tool
extends EditorScenePostImport

func _post_import(scene: Node) -> Object:
    var anim_player = find_animation_player(scene)
    if anim_player == null:
        return scene
    
    # 导出所有动画为.res文件
    var export_dir = "res://Animations/Kuno/"
    DirAccess.make_dir_recursive_absolute(export_dir)
    
    for anim_name in anim_player.get_animation_list():
        var anim = anim_player.get_animation(anim_name)
        var save_path = export_dir + anim_name + ".res"
        ResourceSaver.save(anim, save_path)
        print("Exported: ", save_path)
    
    return scene

func find_animation_player(node: Node) -> AnimationPlayer:
    if node is AnimationPlayer:
        return node
    for child in node.get_children():
        var result = find_animation_player(child)
        if result != null:
            return result
    return null
```

### 方案5：手动导出动画

1. 打开Kuno1.02.fbx场景
2. 选择AnimationPlayer节点
3. 在Animation面板中选择每个动画
4. 右键 -> Save As...
5. 保存为.res文件

## 推荐做法

**不依赖插件，使用EditorScenePostImport脚本：**

优势：
- ✅ 完全控制导出过程
- ✅ 不依赖第三方插件
- ✅ 可以自定义骨骼路径修复
- ✅ 支持所有Godot版本
- ✅ 可以批量处理

劣势：
- ❌ 需要编写GDScript代码
- ❌ 需要理解Import流程

## 总结

插件不工作的最可能原因：
1. **Skeleton名称不是"Skeleton"** - 这是强制要求
2. **版本不兼容** - 插件为4.3设计，用户用4.6
3. **缺少Bone Map配置** - 插件要求必须配置

**建议：使用EditorScenePostImport脚本替代插件**，这样更灵活且不依赖第三方。
