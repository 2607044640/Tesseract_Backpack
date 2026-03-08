# Godot 4 C# 3D Practice Project

一个使用 Godot 4.6.1 和 C# 开发的 3D 游戏项目，采用组件化架构设计。

## 项目信息

- **引擎版本**: Godot 4.6.1 Stable (Mono)
- **编程语言**: C# (.NET)
- **架构模式**: 组件化架构 (Component-based Architecture)
- **设计原则**: 组合优于继承 (Composition over Inheritance)

## 项目结构

```
3d-practice/
├── Scripts/                          # C# 脚本
│   ├── Animation/                    # 动画系统
│   │   ├── AnimationSet.cs          # 动画集资源（包含所有动画和速度配置）
│   │   └── CharacterAnimationConfig.cs  # 角色动画配置
│   └── Player3D.cs                   # 玩家控制器（Mediator）
│
├── addons/                           # 插件和组件
│   ├── CoreComponents/               # 核心组件（自研）
│   │   ├── PlayerInputComponent.cs  # 输入组件
│   │   ├── MovementComponent.cs     # 移动组件
│   │   └── AnimationControllerComponent.cs  # 动画控制组件
│   ├── alignment-tool/               # 对齐工具插件
│   ├── asset_placer/                 # 资源放置插件
│   ├── godot-jolt/                   # Jolt 物理引擎
│   ├── mixamo_animation_retargeter/  # Mixamo 动画重定向插件
│   ├── phantom_camera/               # Phantom Camera 相机系统
│   └── Todo_Manager/                 # 待办事项管理器
│
├── Scenes/                           # 场景文件
│   ├── Player3D.tscn                # 玩家场景
│   └── Kuno1.02/                    # 角色模型场景
│
├── Animations/                       # 动画资源
│   └── Player_CharacterAnimationConfig.tres  # 玩家动画配置
│
├── project.godot                     # Godot 项目配置
├── 3dPractice.sln                   # Visual Studio 解决方案
└── 3dPractice.csproj                # C# 项目文件
```

## 核心组件系统

### 组件化架构

本项目采用严格的组件化架构，遵循以下原则：

1. **单一职责原则**: 每个组件只负责一个功能
2. **Call Down, Signal Up**: 父节点调用子节点方法，子节点通过事件向上通信
3. **兄弟隔离**: 组件之间不直接引用，通过父节点（Mediator）协调
4. **依赖注入**: 使用 `[Export]` 属性在编辑器中配置引用

### 核心组件

#### 1. PlayerInputComponent
- **职责**: 读取玩家输入并通过 C# 事件广播
- **事件**:
  - `OnMovementInput(Vector2)` - 移动输入（WASD）
  - `OnJumpJustPressed()` - 跳跃输入（空格）
- **特性**: 
  - 支持输入启用/禁用
  - 预留输入缓冲、长按蓄力、按键重映射功能

#### 2. MovementComponent
- **职责**: 处理物理计算和角色移动
- **功能**:
  - 重力应用
  - 跳跃处理
  - 基于相机方向的移动
  - 速度控制
- **配置**:
  - `Speed` - 移动速度（默认 5.0）
  - `JumpVelocity` - 跳跃速度（默认 4.5）
  - `Gravity` - 重力加速度
- **特性**: 
  - 自动获取父节点 CharacterBody3D
  - 自动查找相机引用
  - 预留土狼时间、冲刺、击退功能

#### 3. AnimationControllerComponent
- **职责**: 根据角色状态自动播放动画
- **功能**:
  - 根据 `IsOnFloor()` 判断空中/地面状态
  - 根据速度判断移动/静止状态
  - 根据输入判断冲刺状态
  - 自动应用动画速度配置
- **配置**:
  - `AnimConfig` - 动画配置资源
  - `AnimationBlendTime` - 动画过渡时间（默认 0.2 秒）
- **支持的动画**:
  - Idle, Walk, Run, Sprint
  - JumpStart, JumpLoop, Fall, Land
  - Attack1, Attack2, Attack3
  - Dodge, Hit, Death

#### 4. Player3D (Mediator)
- **职责**: 协调所有组件，处理相机控制和角色朝向
- **功能**:
  - 自动查找并连接子组件
  - 订阅输入事件并调用移动组件
  - 处理相机旋转（鼠标控制）
  - 更新角色朝向（面向移动方向）

### 动画系统

#### AnimationSet (Resource)
动画集资源，集中管理角色的所有动画和播放速度。

**特性**:
- 每个动画都有独立的速度配置（0.0 - 5.0，步进 0.1）
- 自动设置循环模式（移动动画循环，一次性动画不循环）
- 支持通过动画名称获取速度

**配置示例**:
```csharp
IdleAnimation: mixamo_com
IdleAnimationSpeed: 1.0

RunAnimation: mixamo_com
RunAnimationSpeed: 1.2

JumpStartAnimation: mixamo_com
JumpStartAnimationSpeed: 1.5
```

#### CharacterAnimationConfig (Resource)
角色动画配置，类似 Unreal Engine 的 Data Asset。

**功能**:
- 引用 AnimationSet
- 应用动画到 AnimationPlayer
- 自动设置循环模式

## 使用的插件

### 1. Jolt Physics (godot-jolt)
- **用途**: 高性能物理引擎
- **替代**: Godot 默认物理引擎
- **配置**: `project.godot` 中设置 `3d/physics_engine="Jolt Physics"`

### 2. Phantom Camera (phantom_camera)
- **用途**: 高级相机系统
- **功能**: 相机跟随、平滑过渡、多相机管理
- **自动加载**: PhantomCameraManager

### 3. Mixamo Animation Retargeter (mixamo_animation_retargeter)
- **用途**: 自动重定向 Mixamo 动画到 Godot 骨骼
- **功能**: 批量导入和重定向动画

### 4. Alignment Tool (alignment-tool)
- **用途**: 场景编辑器中的对齐工具
- **功能**: 快速对齐、分布节点

### 5. Asset Placer (asset_placer)
- **用途**: 场景中快速放置资源
- **功能**: 批量放置、随机旋转/缩放

### 6. Todo Manager (Todo_Manager)
- **用途**: 项目待办事项管理
- **功能**: 在代码中添加 TODO 注释并统一管理

## 输入映射

项目使用以下输入映射（在 `project.godot` 中配置）：

| 动作 | 按键 | 手柄 |
|------|------|------|
| `move_forward` | W / ↑ | 左摇杆 ↑ |
| `move_backward` | S / ↓ | 左摇杆 ↓ |
| `move_left` | A / ← | 左摇杆 ← |
| `move_right` | D / → | 左摇杆 → |
| `jump` | Space | A 按钮 |
| `sprint` | Shift | L3 按钮 |
| `crouch` | Ctrl / C | L2 按钮 |
| `interact` | E / F | B 按钮 |
| `attack` | 鼠标左键 | RT 按钮 |
| `aim` | 鼠标右键 | LT 按钮 |

## 快速开始

### 1. 克隆项目
```bash
git clone <repository-url>
cd 3d-practice
```

### 2. 打开项目
- 使用 Godot 4.6.1 Mono 版本打开项目
- 等待 C# 项目自动生成

### 3. 构建 C# 项目
```bash
dotnet build 3dPractice.sln
```

### 4. 运行项目
- 在 Godot 编辑器中按 F5 运行
- 或点击右上角的播放按钮

## 创建新角色

### 1. 创建场景结构
```
YourCharacter (CharacterBody3D)
├── MovementComponent (Node)
├── PlayerInputComponent (Node)
├── AnimationControllerComponent (Node)
├── CollisionShape3D
├── CharacterModel (Node3D)
└── CameraPivot (Node3D)
    └── SpringArm3D
        └── Camera3D
```

### 2. 配置组件
- **Player3D**: 设置 `CharacterModelPath` 为你的模型节点路径
- **MovementComponent**: 调整 `Speed`, `JumpVelocity`, `Gravity`
- **AnimationControllerComponent**: 
  - 创建 AnimationSet 资源
  - 创建 CharacterAnimationConfig 资源
  - 在 Inspector 中指定 `AnimConfig`

### 3. 配置动画
1. 创建 `AnimationSet` 资源（右键 → 新建资源 → AnimationSet）
2. 在 AnimationSet 中设置所有动画和速度
3. 创建 `CharacterAnimationConfig` 资源
4. 在 CharacterAnimationConfig 中引用 AnimationSet
5. 在 AnimationControllerComponent 中引用 CharacterAnimationConfig

## 开发指南

### 添加新组件

1. 继承自 `Node`
2. 添加 `[GlobalClass]` 特性
3. 使用 `[Export]` 暴露配置属性
4. 使用 C# `event Action` 向上通信
5. 提供公开方法供父节点调用

示例：
```csharp
[GlobalClass]
public partial class HealthComponent : Node
{
    [Export] public float MaxHealth { get; set; } = 100f;
    
    public event Action OnDeath;
    public event Action<float> OnHealthChanged;
    
    private float _currentHealth;
    
    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        OnHealthChanged?.Invoke(_currentHealth);
        
        if (_currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }
}
```

### 架构原则

❌ **不要做**:
- 组件之间直接 `GetNode` 引用
- 在组件中硬编码特定角色的内容
- 创建深层继承结构
- 在单个脚本中混合多个职责

✅ **应该做**:
- 使用 `[Export]` 依赖注入
- 使用 C# 事件通信
- 保持组件通用和可复用
- 遵循单一职责原则
- 通过父节点（Mediator）协调组件

## 许可证

[LICENSE](LICENSE)

## 贡献

欢迎提交 Issue 和 Pull Request！

## 联系方式

如有问题，请通过 Issue 联系。
