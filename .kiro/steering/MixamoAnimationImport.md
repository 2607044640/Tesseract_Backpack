---
inclusion: manual
---

# Mixamo动画导入到Godot教程

## 前置条件
- 已有角色模型（如Sophia）
- 从Mixamo下载的FBX动画文件（Without Skin）

## 步骤1：准备FBX文件

1. 将FBX文件放到项目目录（如 `Scenes/Fast Run.fbx`）
2. Godot会自动检测并导入

## 步骤2：配置导入设置

1. 在FileSystem面板选中FBX文件
2. 切换到 `Import` 面板（在Scene面板旁边）
3. 配置以下选项：

### Animation设置
- `Import` = true ✅
- `FPS` = 30
- `Trimming` = true（裁剪空白帧）

### 关键：创建BoneMap（骨骼映射）

4. 在Import面板找到 `Skeleton` 部分
5. 点击 `Create Bone Map`
6. 选择 `SkeletonProfileHumanoid`（人形骨骼配置）
7. Godot会自动映射Mixamo的骨骼名称
8. 检查映射：
   - 绿色 = 映射成功 ✅
   - 黄色 = 需要手动调整 ⚠️
   - 红色 = 映射失败 ❌

### 提取动画资源

9. 在Import面板找到 `Animation` 标签
10. 展开动画列表（通常是 `mixamo.com`）
11. 勾选 `Save to File`
12. 设置路径：`res://player_Sophia/sophia_skin/animations/fast_run.res`
13. 点击 `Reimport` 按钮

## 步骤3：添加到AnimationPlayer

1. 打开 `sophia_skin.tscn` 场景
2. 选中 `AnimationPlayer` 节点
3. 在Animation面板点击 `+` 添加动画
4. 选择刚才提取的 `fast_run.res`
5. 重命名为 `FastRun`

## 步骤4：添加到AnimationTree（可选）

### 方法A：添加到现有BlendTree
1. 选中 `AnimationTree` 节点
2. 双击 `Move` 状态的BlendTree
3. 右键 → Add Node → Animation
4. 选择 `FastRun` 动画
5. 连接到输出节点

### 方法B：创建新状态
1. 在StateMachine里右键 → Add State
2. 命名为 `FastRun`
3. 添加 `FastRun` 动画节点
4. 创建从 `Move` 到 `FastRun` 的转换

## 步骤5：代码控制（C#）

```csharp
// 在Player3D.cs里添加
[Export] public float RunSpeed = 10.0f;  // 跑步速度

// 在_PhysicsProcess里
float currentSpeed = Input.IsActionPressed("sprint") ? RunSpeed : Speed;

// 通知AnimationTree切换动画
var animTree = GetNode<AnimationTree>("SophiaSkin/AnimationTree");
animTree.Set("parameters/conditions/is_running", Input.IsActionPressed("sprint"));
```

## 常见问题

### Q: 动画播放但角色姿势错误？
A: BoneMap映射不正确，重新检查骨骼映射

### Q: 动画不播放？
A: 检查AnimationPlayer是否正确加载了动画资源

### Q: 角色在地面滑动？
A: Mixamo动画默认有Root Motion，需要：
- 下载时勾选 `In Place` ✅
- 或在代码里禁用Root Motion

### Q: 导入时报错 "Resource save path not valid"？
A: 确保目标文件夹存在，或使用已存在的文件夹路径

## UE对比

| Godot | Unreal Engine |
|-------|---------------|
| BoneMap | IK Retargeter |
| AnimationPlayer | Animation Sequence |
| AnimationTree | Animation Blueprint |
| SkeletonProfileHumanoid | UE5 Mannequin Skeleton |

## 参考资源

- [Godot官方文档 - Importing 3D Scenes](https://docs.godotengine.org/en/stable/tutorials/assets_pipeline/importing_3d_scenes/index.html)
- [Mixamo Animation Retargeter插件](https://godotengine.org/asset-library/asset/3429)
