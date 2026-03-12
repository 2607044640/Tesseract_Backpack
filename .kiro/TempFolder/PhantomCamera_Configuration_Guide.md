# PhantomCamera3D 配置指南

## 场景结构确认

### 主场景（3dPractice.tscn）✓
```
Node3D (Root)
└── Camera3D (真实相机)
    └── PhantomCameraHost (GDScript)
```

### Player 场景（Player3D.tscn）✓
```
Player3D (CharacterBody3D)
├── Components
│   ├── PlayerInputComponent
│   ├── MovementComponent
│   ├── CharacterRotationComponent
│   ├── AnimationControllerComponent
│   └── CameraControlComponent
├── CollisionShape3D
├── KunoSkin (模型)
└── PhantomCamera3D ← 虚拟相机
```

## PhantomCamera3D Inspector 配置

### 必需属性
1. **Follow Mode**: `Third Person` (值 = 6)
2. **Follow Target**: `NodePath("..")` 指向父节点 Player3D
3. **Priority**: `10` (数值越高优先级越高)

### 距离和弹簧属性
4. **Follow Distance**: `4.0` (相机跟随距离)
5. **Spring Length**: `4.0` (SpringArm 长度，应与 Follow Distance 相同)

### 初始位置和旋转
6. **Transform**:
   - Position: `(0, 2.5, 4.0)` - 相机在角色后上方
   - Rotation: 约 `(-30°, 0°, 0°)` - 向下倾斜看向角色

### 可选属性
7. **Follow Damping**: `false` (如需平滑跟随可启用)
8. **Collision Mask**: `1` (碰撞检测层)
9. **Margin**: `0.01` (碰撞边距)

## CameraControlComponent Inspector 配置

### 必需设置
1. **PCam Path**: `"PhantomCamera3D"` (默认值)
2. **Mouse Sensitivity**: `0.05` (鼠标灵敏度)

### 视角限制
3. **Min Pitch**: `-89.9` (最低视角，几乎垂直向下)
4. **Max Pitch**: `50` (最高视角，向上 50 度)
5. **Min Yaw**: `0` (左右旋转最小值)
6. **Max Yaw**: `360` (左右旋转最大值，完整循环)

## 调试检查清单

### 启动时检查
- [ ] 控制台输出 "CameraControlComponent: PhantomCamera 系统已初始化 ✓"
- [ ] 没有错误信息 "PhantomCamera3D 节点未找到"
- [ ] 鼠标被捕获（看不到鼠标光标）

### 运行时检查
- [ ] 移动鼠标时相机跟随旋转
- [ ] 上下视角被正确限制（不能翻转）
- [ ] 左右旋转可以 360 度循环
- [ ] 按 ESC 可以释放鼠标

### 常见问题
1. **鼠标不能控制相机**
   - 检查 PhantomCamera3D 是否是 Player3D 的直接子节点
   - 检查 CameraControlComponent 的 PCamPath 是否正确
   - 查看控制台是否有初始化错误

2. **相机位置不对**
   - 调整 PhantomCamera3D 的 Transform Position
   - 调整 Follow Distance 和 Spring Length
   - 确保 Follow Target 指向 Player3D

3. **相机抖动**
   - 已启用 Physics Interpolation ✓
   - 如仍抖动，启用 Follow Damping

## 完成状态
- ✓ 删除了 CameraPivot 和 SpringArm3D
- ✓ PhantomCamera3D 正确配置为 Player3D 子节点
- ✓ CameraControlComponent 使用 PhantomCamera API
- ✓ 鼠标输入正确处理
- ✓ 编译成功无错误
