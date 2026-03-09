# CoreComponents 索引

## 📖 文档导航

### 🚀 快速开始
**文件：** [QUICK_START.md](QUICK_START.md)

**适合：** 新手，想快速创建实体

**内容：**
- 5 分钟创建新实体
- 组件速查表
- 实体配方（Player、Enemy、Box、NPC）
- 调试技巧

**使用场景：**
- "我想快速创建一个玩家"
- "我需要一个 AI 敌人"
- "我想知道有哪些组件可用"

---

### 📚 完整文档
**文件：** [README.md](README.md)

**适合：** 想深入了解框架

**内容：**
- 核心概念详解（Entity、Component）
- 完整的组件清单
- 组件通信模式
- 最佳实践和常见错误
- 目录结构说明

**使用场景：**
- "我想理解 Godot.Composition 的工作原理"
- "我想知道如何设计组件"
- "我遇到了问题，需要查找解决方案"

---

## 💻 示例代码

### 🎮 Player3D 示例
**文件：** [Examples/Player3D_Example.cs.txt](Examples/Player3D_Example.cs.txt)

**内容：**
- 完整的玩家实体代码
- 场景结构说明
- 组件依赖关系图

**使用方法：**
1. 复制代码到你的 Scripts/ 目录
2. 重命名为 `.cs` 文件
3. 创建对应的场景结构

---

### 👾 Enemy 示例
**文件：** [Examples/Enemy_Example.cs](Examples/Enemy_Example.cs)

**内容：**
- Enemy 实体
- AIInputComponent（巡逻、追击）
- HealthComponent（生命值系统）

**关键特性：**
- 展示组件复用
- AI 输入与玩家输入的接口统一
- 完整的 AI 决策逻辑

**使用方法：**
1. 复制 AIInputComponent 和 HealthComponent 到项目
2. 创建 Enemy 实体
3. 复用 MovementComponent 等组件

---

### 📦 Box 示例
**文件：** [Examples/Box_Example.cs](Examples/Box_Example.cs)

**内容：**
- Box 实体（可推动、可破坏）
- PushableComponent
- BreakableComponent（破碎效果）

**关键特性：**
- 物理对象的组件使用
- 复用 HealthComponent
- 破碎粒子和音效

**使用方法：**
1. 复制 PushableComponent 和 BreakableComponent
2. 创建 RigidBody3D 场景
3. 配置破碎效果资源

---

### 🛠️ 组件模板
**文件：** [Examples/ComponentTemplate.cs](Examples/ComponentTemplate.cs)

**内容：**
- 完整的组件代码模板
- 所有必需的代码结构
- 详细的注释说明

**使用方法：**
1. 复制模板文件
2. 重命名为你的组件名
3. 填充具体逻辑
4. 删除不需要的部分

---

## 🗂️ 目录结构

```
CoreComponents/
├── INDEX.md                           ← 你在这里
├── README.md                          ← 完整文档
├── QUICK_START.md                     ← 快速开始
├── ARCHITECTURE.md                    ← 架构设计模式
├── HOW_IT_WORKS.md                    ← Godot.Composition 工作原理
├── MIGRATION_GUIDE.md                 ← 项目搬运指南
│
├── Examples/                          ← 示例代码
│   ├── Player3D_Example.cs.txt
│   ├── Enemy_Example.cs
│   ├── Box_Example.cs
│   └── ComponentTemplate.cs
│
├── Animation/                         ← 动画系统
│   ├── AnimationSet.cs                ← 动画集合（类似UE Data Asset）
│   └── CharacterAnimationConfig.cs    ← 角色动画配置
│
├── AnimationAssets/                   ← 动画资源文件
│   ├── AnimationFBX/                  ← FBX 动画文件
│   ├── AnimationRes/                  ← 动画资源
│   └── Player_CharacterAnimationConfig.tres
│
└── [核心组件文件]
    ├── PlayerInputComponent.cs
    ├── MovementComponent.cs
    ├── CharacterRotationComponent.cs
    ├── CameraControlComponent.cs
    └── AnimationControllerComponent.cs
```

---

## 🎯 使用流程

### 流程 1：创建新的 Player
```
1. 阅读 QUICK_START.md → "创建新的 Player 实体"
2. 参考 Player3D_Example.cs.txt
3. 复制场景结构
4. 测试运行
```

### 流程 2：创建新的 Enemy
```
1. 阅读 QUICK_START.md → "常见实体配方"
2. 参考 Enemy_Example.cs
3. 复制 AIInputComponent 和 HealthComponent
4. 复用 MovementComponent 等组件
5. 测试运行
```

### 流程 3：创建新的可交互物体
```
1. 阅读 QUICK_START.md → "常见实体配方"
2. 参考 Box_Example.cs
3. 复制 PushableComponent 和 BreakableComponent
4. 配置破碎效果
5. 测试运行
```

### 流程 4：创建自定义组件
```
1. 阅读 README.md → "创建新组件的步骤"
2. 复制 ComponentTemplate.cs
3. 填充具体逻辑
4. 在场景中测试
5. 添加到组件库
```

---

## 📋 组件速查

### 已实现的核心组件

| 组件 | 文件位置 | 用途 |
|------|----------|------|
| PlayerInputComponent | `addons/CoreComponents/` | 玩家输入 |
| MovementComponent | `addons/CoreComponents/` | 物理移动 |
| CharacterRotationComponent | `addons/CoreComponents/` | 角色朝向 |
| CameraControlComponent | `addons/CoreComponents/` | 相机控制 |
| AnimationControllerComponent | `addons/CoreComponents/` | 动画控制 |

### 示例中的组件（可复制使用）

| 组件 | 文件位置 | 用途 |
|------|----------|------|
| AIInputComponent | `Examples/Enemy_Example.cs` | AI 输入 |
| HealthComponent | `Examples/Enemy_Example.cs` | 生命值 |
| PushableComponent | `Examples/Box_Example.cs` | 可推动 |
| BreakableComponent | `Examples/Box_Example.cs` | 破碎效果 |

---

## 🔍 常见问题

### Q: 我应该从哪里开始？
**A:** 从 [QUICK_START.md](QUICK_START.md) 开始，5 分钟创建你的第一个实体。

### Q: 我想深入了解框架原理？
**A:** 阅读 [README.md](README.md) 的"核心概念"部分。

### Q: 如何创建 Enemy？
**A:** 参考 [Enemy_Example.cs](Examples/Enemy_Example.cs)，复制 AIInputComponent。

### Q: 如何创建自定义组件？
**A:** 使用 [ComponentTemplate.cs](Examples/ComponentTemplate.cs) 作为起点。

### Q: 组件之间如何通信？
**A:** 阅读 [README.md](README.md) 的"组件通信模式"部分。

### Q: 遇到编译错误怎么办？
**A:** 查看 [QUICK_START.md](QUICK_START.md) 的"常见错误"部分。

---

## 🎓 学习路径

### 第 1 天：快速上手
- [ ] 阅读 QUICK_START.md
- [ ] 创建第一个 Player 实体
- [ ] 测试移动和跳跃

### 第 2 天：理解原理
- [ ] 阅读 README.md 核心概念
- [ ] 理解 Entity 和 Component
- [ ] 理解依赖注入

### 第 3 天：组件复用
- [ ] 阅读 Enemy_Example.cs
- [ ] 创建 Enemy 实体
- [ ] 复用 MovementComponent

### 第 4 天：创建新组件
- [ ] 使用 ComponentTemplate.cs
- [ ] 创建自定义组件
- [ ] 集成到实体

### 第 5 天：高级应用
- [ ] 创建复杂的组件组合
- [ ] 实现自定义通信
- [ ] 优化和重构

---

## 🚀 快速链接

- **快速开始** → [QUICK_START.md](QUICK_START.md)
- **完整文档** → [README.md](README.md)
- **Player 示例** → [Examples/Player3D_Example.cs.txt](Examples/Player3D_Example.cs.txt)
- **Enemy 示例** → [Examples/Enemy_Example.cs](Examples/Enemy_Example.cs)
- **Box 示例** → [Examples/Box_Example.cs](Examples/Box_Example.cs)
- **组件模板** → [Examples/ComponentTemplate.cs](Examples/ComponentTemplate.cs)

---

## 📞 需要帮助？

1. **查看文档**：先查看 README.md 和 QUICK_START.md
2. **查看示例**：Examples/ 目录有完整的示例代码
3. **查看模板**：ComponentTemplate.cs 有详细的注释
4. **查看源码**：现有组件的实现可以作为参考

---

## 🎉 开始创建吧！

选择一个起点：

- 🚀 **我想快速开始** → [QUICK_START.md](QUICK_START.md)
- 📚 **我想深入学习** → [README.md](README.md)
- 💻 **我想看示例** → [Examples/](Examples/)
- 🛠️ **我想创建组件** → [ComponentTemplate.cs](Examples/ComponentTemplate.cs)

Happy Coding! 🎮
