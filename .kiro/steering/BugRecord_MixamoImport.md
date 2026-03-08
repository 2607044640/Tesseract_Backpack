---
inclusion: manual
---

# Bug记录：Mixamo动画导入问题

## Bug #1: Resource save path not valid

**日期：** 2026-03-05

**错误信息：**
```
ERROR: editor/import/3d/resource_importer_scene.cpp:3029
Condition "!save_path.is_empty() && !DirAccess::exists(save_path.get_base_dir())" is true.
Returning: ERR_FILE_BAD_PATH

ERROR: Resource save path true not valid. Ensure parent directory has been created.

ERROR: Error importing 'res://Scenes/Fast run.fbx'.
```

**原因：**
在 `Fast Run.fbx.import` 文件中设置了动画导出路径，但目标目录不存在：
- 尝试1：`res://Scenes/animations/fast_run.res` ❌
- 尝试2：`res://player_Sophia/sophia_skin/animations/fast_run.res` ❌

**解决方案：**
1. 方法A：先创建目标文件夹
2. 方法B：不预设导出路径，让Godot自动处理（推荐）✅
   - 移除 `_subresources` 配置
   - 在Import面板手动提取动画

**Godot编辑器日志位置：**
- 编辑器Output面板（不保存到文件）
- 运行时日志：`%APPDATA%\Godot\app_userdata\<ProjectName>\logs\godot.log`
- 查看项目数据文件夹：Godot菜单 → Project → Open Project Data Folder

**状态：** ✅ 已修复（移除自动导出配置）

---

## Bug #2: BoneMap映射失败（预防性记录）

**可能症状：**
- 动画导入成功但播放时角色姿势扭曲
- 骨骼旋转异常
- 部分身体部位不动

**原因：**
Mixamo的骨骼命名与Godot的SkeletonProfileHumanoid不完全匹配

**解决方案：**
1. 在Import面板创建BoneMap时选择 `SkeletonProfileHumanoid`
2. 检查自动映射结果：
   - 绿色 = 正确 ✅
   - 黄色 = 需要手动调整 ⚠️
   - 红色 = 失败 ❌
3. 手动调整黄色/红色的骨骼映射
4. 常见需要手动映射的骨骼：
   - `mixamorig:Hips` → `Hips`
   - `mixamorig:Spine` → `Spine`
   - `mixamorig:LeftHand` → `LeftHand`

**预防措施：**
使用 [Mixamo Animation Retargeter插件](https://godotengine.org/asset-library/asset/3429) 自动化处理

**状态：** ⚠️ 待验证

---

## Bug #3: 动画In Place问题（预防性记录）

**可能症状：**
- 角色播放跑步动画时在地面滑动
- 移动速度与动画不匹配

**原因：**
Mixamo动画默认包含Root Motion（根运动），角色会在世界空间移动

**解决方案：**
1. 方法A：下载时勾选 `In Place` ✅（推荐）
2. 方法B：在Godot里禁用Root Motion
   - AnimationPlayer → 动画设置 → Root Motion Track = ""

**UE对比：**
类似UE的 `Enable Root Motion` 选项

**状态：** ⚠️ 待验证

---

## 学习笔记

### Godot动画系统层级
```
AnimationPlayer (存储动画数据)
    ↓
AnimationTree (状态机/混合树)
    ↓
Skeleton3D (骨骼系统)
    ↓
MeshInstance3D (模型)
```

### 关键概念
- **BoneMap**: 骨骼名称映射表，连接源骨骼和目标骨骼
- **SkeletonProfile**: 标准骨骼配置（如Humanoid人形）
- **AnimationLibrary**: 动画资源集合
- **Root Motion**: 动画驱动的角色位移

### 与UE的对比
| Godot | Unreal Engine | 说明 |
|-------|---------------|------|
| BoneMap | IK Retargeter | 骨骼重定向 |
| SkeletonProfile | Skeleton Asset | 骨骼配置 |
| AnimationPlayer | Animation Sequence | 动画播放器 |
| AnimationTree | Animation Blueprint | 动画状态机 |
| Root Motion Track | Root Motion | 根运动 |

---

## 相关文件
- 教程文档：`.kiro/steering/MixamoAnimationImport.md`
- 导入配置：`Scenes/Fast Run.fbx.import`
- 角色场景：`player_Sophia/sophia_skin/sophia_skin.tscn`
