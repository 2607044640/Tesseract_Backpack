# Godot 4 Animation Loop Mode 研究

## 问题
fast_run动画只播放一次就停止，需要设置循环播放。

## 关键发现

### 1. Godot 4的重要变化
在Godot 4中，loop设置存储在**Animation资源本身**，而不是AnimationPlayer节点。

### 2. 两种设置方法

#### 方法A：编辑器GUI设置（推荐用于.res文件）
1. 在FileSystem中双击打开 `fast_run.res`
2. 在Inspector中找到 `Loop Mode` 下拉菜单
3. 选项：
   - `None` - 不循环（默认）
   - `Linear` - 线性循环（推荐用于跑步/行走）
   - `Ping Pong` - 来回播放

**注意：** 从截图看，Loop Mode显示为"None"，需要点击下拉菜单改为"Linear"

#### 方法B：代码设置（C#）
```csharp
var anim = animPlayer.GetAnimation("Sprint");
if (anim != null)
{
    anim.LoopMode = Animation.LoopModeEnum.Linear;
}
```

### 3. Loop Mode选项说明
- `LOOP_NONE` (0) - 播放一次后停止
- `LOOP_LINEAR` (1) - 连续循环播放
- `LOOP_PINGPONG` (2) - 来回播放（正向→反向→正向...）

### 4. 最佳实践
- 移动动画（Idle/Walk/Run/Sprint）应该设置为 `Linear`
- 一次性动画（Jump/Attack/Hit）应该设置为 `None`
- 代码设置会永久保存到.res文件中

## 解决方案
在LoadQuickTestAnimations()方法中添加循环设置：
```csharp
if (SprintAnimation != null)
{
    SprintAnimation.LoopMode = Animation.LoopModeEnum.Linear;
    library.AddAnimation("Sprint", SprintAnimation);
}
```

## 参考来源
- [Godot 4 Animation Loop Fix](https://openillumi.com/en/en-godot4-animation-loop-fix-loop_mode/)
- [Stack Overflow: Loop Animation](https://stackoverflow.com/questions/75796006/)
