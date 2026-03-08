# 动画配置系统 - 快速开始

## ✅ 已完成

1. **AnimationSet.cs** - 动画合集Resource类
2. **CharacterAnimationConfig.cs** - 角色动画配置Resource类
3. **Player3D.cs** - 已更新支持配置系统
4. **编译成功** - 无错误

## 🎯 你需要做的（在Godot编辑器中）

### 1. 创建AnimationSet资源（5分钟）

```
右键文件系统 → New Resource... → 搜索"AnimationSet"
保存为: Animations/Kuno_MovementSet.res
```

在Inspector中配置：
- Set Name: "Kuno移動" （可用日文）
- Idle Animation: 拖入Idle.res
- Run Animation: 拖入Run.res
- 其他动画槽位根据需要填充

### 2. 创建CharacterAnimationConfig（3分钟）

```
右键文件系统 → New Resource... → 搜索"CharacterAnimationConfig"
保存为: Animations/Kuno_AnimConfig.res
```

在Inspector中配置：
- Character Name: "Kuno" 或 "クノ"
- Movement Set: 拖入刚创建的Kuno_MovementSet.res
- Animation Player Path: "AnimationPlayer"
- Auto Play: ✓ 勾选
- Auto Play Animation: "Idle"

### 3. 应用到Player3D（1分钟）

1. 打开 `Scenes/Player3D.tscn`
2. 选择CharacterBody3D根节点
3. 在Inspector中找到 `Anim Config`
4. 拖入 `Kuno_AnimConfig.res`
5. 保存场景

### 4. 测试（1分钟）

运行游戏（F5），Console应该显示：
```
AnimationConfig applied. Available animations: Idle, Run, ...
```

## 📝 关于你的问题

### Q: AnimationPlayer需要添加吗？还是删除？
**A: 必须保留！** AnimationPlayer是播放动画的核心，不能删除。配置系统会在运行时将.res动画加载到AnimationPlayer中。

### Q: 如何添加已经做好的.res动画？
**A: 两种方式：**
1. **通过AnimationSet** - 在AnimationSet.res中拖入.res动画文件
2. **直接在AnimationPlayer** - 但这样就失去了配置系统的优势

推荐使用方式1，这样可以在多个角色间共享动画。

### Q: 动画合集应该是.res文件吗？
**A: 是的！** 
- AnimationSet.res（合集配置）
- CharacterAnimationConfig.res（角色配置）
- Idle.res, Run.res等（单个动画）

都是.res文件，可以在编辑器中可视化编辑。

### Q: 能出现日文吗？
**A: 完全可以！** Godot使用UTF-8，支持所有Unicode字符：
- 文件名: `クノ_移動セット.res` ✓
- SetName: "移動アニメーション" ✓
- CharacterName: "クノ" ✓
- 动画名: "走る", "ジャンプ" ✓

### Q: 如何修复骨骼路径警告？
**A: 创建Import脚本** - 详见 `AnimationSystem_Guide.md` 中的"修复骨骼路径警告"章节。

简单说：创建一个EditorScenePostImport脚本，在导入时自动删除无效的骨骼轨道。

## 🎨 类似UE的工作流程

```
UE:                          Godot:
Data Asset                → CharacterAnimationConfig.res
Animation Sequence        → Animation.res
Animation Blueprint       → AnimationPlayer + AnimationTree
Montage                   → AnimationSet.res（合集）
```

## 📂 推荐的文件结构

```
Animations/
├── Configs/
│   ├── Kuno_AnimConfig.res
│   └── Sophia_AnimConfig.res
├── Sets/
│   ├── Kuno_MovementSet.res
│   ├── Kuno_CombatSet.res
│   └── Shared_EmoteSet.res
└── Raw/
    ├── Movement/
    │   ├── Idle.res
    │   ├── Walk.res
    │   └── Run.res
    └── Combat/
        ├── Attack1.res
        └── Attack2.res
```

## 🚀 下一步

1. 在Godot中创建AnimationSet和Config资源
2. 配置并应用到Player3D
3. 测试运行
4. 如果有骨骼警告，创建Import脚本修复
5. 为其他角色创建配置，复用相同的动画.res

完整文档请查看 `AnimationSystem_Guide.md`
