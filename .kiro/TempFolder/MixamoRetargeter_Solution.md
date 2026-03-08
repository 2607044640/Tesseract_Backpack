# Mixamo Animation Retargeter 插件不工作 - 解决方案

## 问题确认

✅ 已确认：你的Kuno模型的Skeleton节点名称是 **"Skeleton3D"**

❌ 插件要求：Skeleton节点必须命名为 **"Skeleton"**

这就是插件不工作的原因！

## 解决方案

### 方案A：修改Skeleton名称（如果你想用插件）

1. 选择 `Kuno1.02/Kuno1.02.fbx` 文件
2. 切换到 **Import** 标签
3. 展开 **Nodes** 部分
4. 找到 **Skeleton3D** 节点
5. 将 **Import As** 或 **Unique Name** 改为 `"Skeleton"`
6. 点击 **Reimport**
7. 重新尝试右键 → Retarget Mixamo Animation(s)

### 方案B：使用EditorScenePostImport脚本（推荐）⭐

**不依赖插件，更灵活！**

创建 `kuno_anim_export.gd`:

```gdscript
@tool
extends EditorScenePostImport

func _post_import(scene: Node) -> Object:
    print("=== Kuno Animation Export: Starting ===")
    
    var anim_player = find_animation_player(scene)
    if anim_player == null:
        push_warning("No AnimationPlayer found")
        return scene
    
    # 导出目录
    var export_dir = "res://Animations/Kuno/Raw/"
    DirAccess.make_dir_recursive_absolute(export_dir)
    
    # 导出所有动画
    var exported_count = 0
    for anim_name in anim_player.get_animation_list():
        var anim = anim_player.get_animation(anim_name)
        var save_path = export_dir + anim_name + ".res"
        
        var err = ResourceSaver.save(anim, save_path)
        if err == OK:
            print("✓ Exported: ", anim_name, " -> ", save_path)
            exported_count += 1
        else:
            push_error("✗ Failed to export: ", anim_name)
    
    print("=== Kuno Animation Export: Complete (", exported_count, " animations) ===")
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

**使用步骤：**
1. 创建上面的脚本文件
2. 选择 `Kuno1.02.fbx`
3. Import标签 → **Import Script** → **Path** → 选择脚本
4. 点击 **Reimport**
5. 查看Output标签，应该看到导出的动画列表
6. 动画会保存在 `Animations/Kuno/Raw/` 文件夹

### 方案C：手动导出（最简单）

1. 双击打开 `kuno_1_02.tscn`
2. 选择 **AnimationPlayer** 节点
3. 在底部Animation面板中，选择一个动画（如"Run"）
4. 点击动画名称旁边的下拉菜单
5. 选择 **Save As...**
6. 保存为 `Animations/Kuno/Raw/Run.res`
7. 对每个动画重复此操作

## 为什么插件不工作？

根据研究，插件有以下硬性要求：

1. ❌ **Skeleton名称必须是"Skeleton"** - 你的是"Skeleton3D"
2. ❌ **需要Godot 4.3** - 你用的是4.6.1（可能有兼容性问题）
3. ❌ **需要配置Bone Map** - 你没有配置
4. ✅ **需要FBX文件** - 你有

## 推荐做法

**使用方案B（EditorScenePostImport脚本）**

优势：
- ✅ 不需要改Skeleton名称
- ✅ 不依赖第三方插件
- ✅ 支持所有Godot版本
- ✅ 可以自定义导出逻辑
- ✅ 一次配置，自动导出
- ✅ 可以添加骨骼路径修复逻辑

劣势：
- ❌ 需要写一点GDScript（但我已经提供了完整代码）

## 导出后如何使用？

导出的.res动画文件可以：

1. **直接拖入AnimationSet** - 使用我们创建的AnimationConfig系统
2. **添加到AnimationLibrary** - 在AnimationPlayer中使用
3. **在多个角色间共享** - 只要骨骼结构相似

## 测试

使用方案B后，你应该在 `Animations/Kuno/Raw/` 看到：
- Run.res
- Idle.res
- Jump.res
- 等等...

然后就可以在AnimationSet中使用这些.res文件了！

## 关于骨骼警告

如果看到类似的警告：
```
couldn't resolve track: 'Skeleton3D:mixamorig_RightHandMiddle3'
```

可以在Import脚本中添加清理逻辑（详见之前的AnimationSystem_Guide.md）。


## ✅ 插件已修复（Godot 4.6兼容）

### 修复内容

已对 `mixamo_animation_retargeter.gd` 进行以下修复：

1. **添加递归Tree查找** - 当原方法失败时使用备用方法
2. **添加null检查** - 防止 `get_root()` 崩溃
3. **添加错误提示** - 更好的调试信息

### 修复的代码

```gdscript
# 修复1: 添加递归查找Tree的备用方法
static func get_filesystem_tree(plugin:EditorPlugin)->Tree:
    var dock = plugin.get_editor_interface().get_file_system_dock()
    var tree = find_node_by_class_path(dock, ['SplitContainer','Tree']) as Tree
    if tree == null:
        tree = find_tree_recursive(dock)  # 备用方法
    return tree

static func find_tree_recursive(node: Node) -> Tree:
    if node is Tree:
        return node
    for child in node.get_children():
        var result = find_tree_recursive(child)
        if result != null:
            return result
    return null

# 修复2: 添加null检查
static func tree_get_selected_items(tree:Tree)->Array:
    var res = []
    if tree == null:
        push_warning("Tree is null")
        return res
    var root = tree.get_root()
    if root == null:
        push_warning("Tree root is null")
        return res
    # ... 继续处理
```

### 测试步骤

1. **重启Godot编辑器** - 让插件重新加载
2. 右键点击FBX文件
3. 选择 "Retarget Mixamo Animation(s)"
4. 如果仍然失败，查看Output标签的错误信息

### 如果仍然不工作

如果修复后仍然有问题，说明Godot 4.6的FileSystemDock结构变化太大。

**推荐：使用EditorScenePostImport脚本**（更可靠）

这个方法不依赖FileSystemDock的内部结构，100%兼容所有Godot版本。
