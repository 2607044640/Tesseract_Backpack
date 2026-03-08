# Bone Map红点问题研究

## 问题描述
用户在Import设置的Retarget > Bone Map中看到红点，表示某些骨骼未映射。这可能导致动画无法正常播放。

## 网页搜索结果

### Source 1: Godot官方 - Animation Retargeting in Godot 4.0
URL: https://godotengine.org/article/animation-retargeting-in-godot-4-0/

关键信息（改写以符合合规要求）：
- Bone Map和Retargeting是Godot 4的新功能，用于在不同模型间共享动画
- 需要设置SkeletonProfileHumanoid配置文件
- 如果骨骼包含英文通用名称（如hips, shoulder, arm, leg, foot等），会自动映射
- Retargeting主要用于在不同模型间复用动画，但需要骨骼rest pose相似

### Source 2: Forum - After importing GLB with Bone Map, animation data lost
URL: https://forum.godotengine.org/t/after-importing-a-glb-file-with-animations-using-a-bone-map-some-animation-data-seems-to-be-lost/78122

关键发现：
- 使用Bone Map后，某些动画轨道会丢失（特别是position tracks）
- 只有rotation tracks保留，position tracks除了Hips外都消失
- 这是Bone Map retargeting的已知问题
- 如果模型骨骼不完整（不是完整的humanoid），Bone Map会有问题

### Source 3: Forum - Bone map not making model work
URL: https://forum.godotengine.org/t/bone-map-not-making-my-model-twerk/129412

重要观点：
- Bone Map本质上只是骨骼重命名工具
- 只有在勾选"rename bones"时retargeting才工作
- Bone Map不包含旋转偏移，只适用于相同骨骼结构的模型
- 如果骨骼rest pose不同，retargeting效果会很差

## 根本原因分析

1. **Bone Map不是必需的**
   - 如果模型和动画来自同一个源（都是Mixamo），不需要使用Bone Map
   - Bone Map主要用于跨不同模型retarget动画

2. **红点表示映射不完整**
   - Sophia模型可能不是完整的humanoid骨骼
   - 某些手指或面部骨骼可能缺失
   - 不完整的映射会导致动画问题

3. **Bone Map可能导致动画轨道丢失**
   - 使用Bone Map后，某些position tracks会消失
   - 这会导致动画看起来不正常或完全不播放

## 解决方案

### 方案1：完全移除Bone Map（推荐）

由于Sophia的模型和动画都来自同一个GLB文件，不需要retargeting，应该：

1. 在Import设置中，找到Retarget部分
2. 将Bone Map设置为空（None）
3. 将Profile也设置为空（None）
4. 点击Reimport

这样动画会直接使用原始骨骼名称，不会有任何重命名或retargeting。

### 方案2：完整配置Bone Map（不推荐，复杂且可能有问题）

如果确实需要Bone Map：
1. 手动映射所有红点骨骼
2. 确保所有骨骼都是绿点
3. 但这可能仍然导致某些动画轨道丢失

## 实施步骤

1. 在Godot编辑器中选择 `player_Sophia/sophia_skin/model/sophia.glb`
2. 切换到Import标签页
3. 展开Retarget部分
4. 将Bone Map设置为None（清空）
5. 将Profile设置为None（清空）
6. 点击Reimport按钮
7. 测试动画是否正常播放

## 预期结果

移除Bone Map后：
- 骨骼保持原始Mixamo名称
- 所有动画轨道完整保留
- 动画应该能正常播放
- sophia_import.gd脚本会自动修复骨骼路径（如果需要）


## 已执行的修复

### 修改了 sophia.glb.import 文件

已从import配置中移除：
1. `retarget/bone_map` - 完整的Bone Map配置（包括SkeletonProfileHumanoid）
2. `retarget/bone_renamer/unique_node/skeleton_name` - 骨骼重命名配置

现在`"PATH:rig/Skeleton3D"`部分为空，这意味着：
- 不会进行任何骨骼retargeting
- 不会重命名骨骼
- 骨骼保持原始的DEF-*名称
- 动画轨道将直接使用原始骨骼路径

### 下一步操作

1. 在Godot编辑器中，文件会自动重新加载
2. 或者手动选择sophia.glb文件，点击Reimport
3. 检查Output标签页，确认sophia_import.gd脚本的输出
4. 运行游戏测试动画

### 预期结果

- Bone Map界面不再显示（因为已移除）
- 骨骼名称保持为DEF-spine, DEF-upper_arm.L等原始名称
- sophia_import.gd脚本会找到Skeleton3D并修复动画轨道路径
- 动画应该能正常播放


## 额外修复：骨骼名称不匹配

### 发现的问题
从日志中看到警告：
```
WARNING: Node 'sophia/rig/GeneralSkeleton' was modified from inside an instance, but it has vanished.
```

### 原因
- 移除Bone Map后，GLB文件中的骨骼名称从"GeneralSkeleton"变回了原始的"Skeleton3D"
- 但sophia_skin.tscn文件中仍然引用"GeneralSkeleton"
- 导致节点找不到

### 修复
修改了 `3d-practice/player_Sophia/sophia_skin/sophia_skin.tscn`：
- 将 `[node name="GeneralSkeleton" ...]` 改为 `[node name="Skeleton3D" ...]`

### 测试
重新运行游戏，检查：
1. 警告是否消失
2. 动画是否正常播放
3. 角色移动和旋转是否正常
