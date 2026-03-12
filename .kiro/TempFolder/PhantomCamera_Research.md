# PhantomCamera3D 使用研究笔记

## 核心概念
- PhantomCamera3D 是虚拟相机，不是真实相机
- 场景必须有一个真实的 Camera3D + PhantomCameraHost
- PhantomCamera3D 通过 SpringArm3D 实现第三人称跟随

## Third Person Follow Mode 配置

### 必需属性
- `follow_mode = 6` (ThirdPerson)
- `follow_target = NodePath("..")` 指向 Player3D
- `spring_length` - 相机距离（默认 1.0，建议 4.0）

### 旋转控制（关键）
使用以下方法控制相机旋转：
- `get_third_person_rotation()` - 获取欧拉角（弧度）
- `set_third_person_rotation(Vector3)` - 设置欧拉角（弧度）
- `get_third_person_rotation_degrees()` - 获取欧拉角（角度）
- `set_third_person_rotation_degrees(Vector3)` - 设置欧拉角（角度）

### 鼠标输入处理示例（GDScript）
```gdscript
func _unhandled_input(event) -> void:
  if event is InputEventMouseMotion:
    var pcam_rotation_degrees: Vector3
    pcam_rotation_degrees = pcam.get_third_person_rotation_degrees()
    
    # X 轴 = Pitch（上下）
    pcam_rotation_degrees.x -= event.relative.y * mouse_sensitivity
    pcam_rotation_degrees.x = clampf(pcam_rotation_degrees.x, min_pitch, max_pitch)
    
    # Y 轴 = Yaw（左右）
    pcam_rotation_degrees.y -= event.relative.x * mouse_sensitivity
    pcam_rotation_degrees.y = wrapf(pcam_rotation_degrees.y, min_yaw, max_yaw)
    
    pcam.set_third_person_rotation_degrees(pcam_rotation_degrees)
```

## C# 包装类使用

### 获取 PhantomCamera3D
```csharp
using PhantomCamera;

Node3D pcamNode = parent.GetNode<Node3D>("PhantomCamera3D");
PhantomCamera3D pCam = pcamNode.AsPhantomCamera3D();
```

注意：必须使用 `GetNode<Node3D>` 然后调用 `.AsPhantomCamera3D()`，不能直接 `GetNode<PhantomCamera3D>`

### 扩展方法位置
在 `PhantomCamera3DExtensions` 类中：
- `AsPhantomCamera3D(this Node3D node3D)`
- `GetThirdPersonRotation(this PhantomCamera3D pCam3D)`
- `SetThirdPersonRotation(this PhantomCamera3D pCam3D, Vector3 rotation)`

## 物理插值
- 已启用 `physics/common/physics_interpolation=true`
- 避免相机抖动（jitter）

## 场景结构
主场景：
```
Scene Root
└── Camera3D (真实相机)
    └── PhantomCameraHost (GDScript，自动管理)
```

Player 场景：
```
Player3D (CharacterBody3D)
├── PhantomCamera3D (虚拟相机)
├── Components
│   ├── PlayerInputComponent
│   ├── MovementComponent
│   └── CameraControlComponent
└── 其他节点
```

## 重要提示
1. PhantomCamera3D 应该是 Player3D 的直接子节点
2. 不需要 CameraPivot 或 SpringArm3D（PhantomCamera 内部处理）
3. 鼠标输入应该修改 PhantomCamera3D 的旋转，而不是 SpringArm
4. 使用弧度或角度都可以，但要保持一致
