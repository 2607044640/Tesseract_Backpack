# Animation System

## 概述

这个文件夹包含动画系统的核心类，用于集中管理角色动画配置。

## 核心类

### AnimationSet.cs
**类型：** Resource（类似 UE 的 Data Asset）

**用途：** 集中管理角色的所有动画

**功能：**
- 存储所有动画引用（Idle、Walk、Run、Sprint、Jump 等）
- 为每个动画配置播放速度
- 自动设置动画循环模式
- 提供动画速度查询接口

**使用方式：**
```csharp
var animSet = new AnimationSet();
animSet.IdleAnimation = idleAnim;
animSet.IdleAnimationSpeed = 1.0f;
animSet.SetupLoopModes(); // 自动设置循环
```

---

### CharacterAnimationConfig.cs
**类型：** Resource（类似 UE 的 Data Asset）

**用途：** 角色动画配置容器

**功能：**
- 引用 AnimationSet
- 将动画配置应用到 AnimationPlayer
- 自动创建 AnimationLibrary
- 批量添加动画到播放器

**使用方式：**
```csharp
var config = new CharacterAnimationConfig();
config.CharacterName = "Player";
config.AnimationSet = animSet;
config.ApplyToAnimationPlayer(animPlayer);
```

---

## 与 AnimationControllerComponent 的关系

```
CharacterAnimationConfig (Resource)
    ↓ 引用
AnimationSet (Resource)
    ↓ 使用
AnimationControllerComponent (Component)
    ↓ 控制
AnimationPlayer (Godot Node)
```

**工作流程：**
1. 在 Godot 编辑器中创建 `.tres` 资源文件
2. 配置 AnimationSet（设置动画和速度）
3. 配置 CharacterAnimationConfig（引用 AnimationSet）
4. 在 AnimationControllerComponent 中引用 Config
5. 运行时自动应用到 AnimationPlayer

---

## 优势

### 1. 数据驱动
- 动画配置与代码分离
- 可在编辑器中可视化编辑
- 支持多个角色共享配置

### 2. 类型安全
- 使用 C# 类而非字符串
- 编译期检查
- IDE 自动补全

### 3. 复用性
- 不同角色可以使用不同的 AnimationSet
- 同一个 AnimationSet 可以被多个角色使用
- 组件与数据解耦

### 4. 易于维护
- 集中管理所有动画
- 修改动画配置不需要改代码
- 支持热重载

---

## 示例资源文件

参考：`AnimationAssets/Player_CharacterAnimationConfig.tres`

这是一个完整的配置示例，展示了如何在 Godot 编辑器中设置动画。

---

## 扩展建议

### 添加新动画
1. 在 `AnimationSet.cs` 中添加新的 Export 属性
2. 在 `SetupLoopModes()` 中设置循环模式
3. 在 `GetAnimationSpeed()` 中添加速度查询
4. 在 `CharacterAnimationConfig.ApplyToAnimationPlayer()` 中添加到库

### 支持动画事件
```csharp
// 在 AnimationSet 中添加
[Export] public string Attack1HitFrame = "0.5";
```

### 支持动画混合
```csharp
// 在 CharacterAnimationConfig 中添加
[Export] public float DefaultBlendTime = 0.2f;
```

---

## 注意事项

1. **必须使用 [GlobalClass]**：这样才能在 Godot 编辑器中创建资源
2. **Resource 不是 Node**：不能添加到场景树，只能作为资源引用
3. **动画名称一致性**：AnimationSet 中的名称必须与 AnimationControllerComponent 中的匹配
4. **循环模式**：确保调用 `SetupLoopModes()` 来正确设置循环

---

## 相关文件

- **组件：** `../AnimationControllerComponent.cs`
- **资源：** `../AnimationAssets/`
- **示例：** `../Examples/Player3D_Example.cs.txt`
