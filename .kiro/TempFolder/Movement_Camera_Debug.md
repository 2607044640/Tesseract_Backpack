# 移动和相机问题调试清单

## 问题 1: WASD 方向反向（按 D 往左跑）

### 已添加的调试输出
在 `MovementComponent.cs` 中添加了调试输出：
- 输入方向 (Input)
- 相机的 forward 和 right 向量
- 最终计算的移动方向 (Final Direction)

### 测试步骤
1. 运行游戏
2. 按 WASD 键移动
3. 查看控制台输出，确认：
   - 按 D 键时，Input.X 应该是 1
   - 按 A 键时，Input.X 应该是 -1
   - 按 W 键时，Input.Y 应该是 -1
   - 按 S 键时，Input.Y 应该是 1

### 可能的修复方案
如果方向确实反了，可能需要：
- [ ] 将 `right * _currentInputDirection.X` 改为 `right * -_currentInputDirection.X`
- [ ] 或者将 `forward * _currentInputDirection.Y` 改为 `forward * -_currentInputDirection.Y`

---

## 问题 2: 相机穿模

### 当前配置
**Player3D.tscn 中的 PhantomCamera3D：**
- `collision_mask = 1` ✓
- `shape = SubResource("SphereShape3D_pcam")` ✓
- `margin = 0.2` ✓

**场景中的物体：**
- CSGCombiner3D: `use_collision = true`（默认 layer 1）
- CSGBox3D3_Ground: `use_collision = true`（默认 layer 1）

### 问题分析
PhantomCamera3D 的 `collision_mask = 1` 表示它会检测第 1 层的碰撞。
场景中的物体默认在第 1 层，理论上应该能检测到碰撞。

### 可能的原因
1. **碰撞形状太小**：`SphereShape3D` 的 `radius = 0.2` 可能太小
2. **margin 太小**：`margin = 0.2` 可能不够
3. **PhantomCamera 的碰撞检测未正确工作**

### 修复方案
- [ ] 增大 SphereShape3D 的半径（从 0.2 改为 0.5）
- [ ] 增大 margin（从 0.2 改为 0.5）
- [ ] 检查 PhantomCamera 插件的碰撞检测实现

---

## 测试命令

编译：
```cmd
dotnet build "3dPractice.sln"
```

查看运行时日志：
```powershell
Get-Content "$env:APPDATA\Godot\app_userdata\3dPractice\logs\godot.log" -Tail 50
```
