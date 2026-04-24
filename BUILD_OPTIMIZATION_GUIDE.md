# Godot 4.6.1 Mono 编译加速优化指南

## ✅ 已完成的优化

### 1. .csproj 编译配置优化
已在 `3dPractice.csproj` 中添加以下性能优化配置：

```xml
<!-- 禁用共享编译服务器，防止 dotnet build server 进程残留导致挂起 -->
<UseSharedCompilation>false</UseSharedCompilation>

<!-- 开启并行编译，充分利用多核 CPU -->
<BuildInParallel>true</BuildInParallel>

<!-- 关闭不必要的分析器，加快增量编译速度 -->
<RunAnalyzersDuringBuild>false</RunAnalyzersDuringBuild>

<!-- 关闭 XML 文档生成（开发阶段不需要） -->
<GenerateDocumentationFile>false</GenerateDocumentationFile>
```

**注意**: 项目中已安装 3 个代码分析器（Roslynator、SonarAnalyzer、StyleCop），`RunAnalyzersDuringBuild=false` 会在编译时跳过它们，显著加快速度。如果需要代码质量检查，可以手动运行 `dotnet build /p:RunAnalyzersDuringBuild=true`。

### 2. 编译缓存清理
已执行以下清理操作：
- ✅ 关闭 dotnet build server（防止进程残留）
- ✅ 删除 `.godot/mono` 缓存目录（Godot 会自动重建）

### 3. 自动化脚本
已创建两个便捷脚本：

#### `AddDefenderExclusions.ps1`
- **用途**: 自动将项目目录、dotnet SDK、Godot 编辑器加入 Windows Defender 白名单
- **执行方式**: 右键 PowerShell → "以管理员身份运行" → 执行脚本
- **排除路径**:
  - `C:\Godot\3d-practice`
  - `C:\Godot\KiroWorkingSpace`
  - `C:\Program Files\dotnet`
  - `C:\GodotEngine\Godot_v4.6.1-stable_mono_win64` (Godot 编辑器)

#### `CleanBuildCache.bat`
- **用途**: 一键清理编译缓存（关闭 build server + 删除 .godot/mono + 删除 bin/obj）
- **执行时机**: 修改 .csproj 后，或遇到编译卡顿时
- **执行方式**: 双击运行

---

## 📋 下一步操作

### 必须执行（关键）
1. **添加 Windows Defender 排除项**:
   ```powershell
   # 右键 PowerShell → 以管理员身份运行
   cd C:\Godot\3d-practice
   .\AddDefenderExclusions.ps1
   ```
   
   **为什么重要**: Windows Defender 实时扫描会拦截 MSBuild 生成的每一个临时文件，这是卡顿的最大外因。

2. **重新打开 Godot 编辑器并测试**:
   - 打开 Godot 4.6.1 编辑器
   - 点击 "Build" 按钮（编辑器会自动重建 .godot/mono 缓存）
   - 观察编译时间是否改善

### 可选操作
- 如果将来更换 Godot 引擎版本或位置，需要：
  1. 编辑 `AddDefenderExclusions.ps1`，更新 `$exclusionPaths` 中的引擎路径
  2. 以管理员身份重新运行脚本
  
  或手动添加新路径：
  ```powershell
  Add-MpPreference -ExclusionPath "新的Godot引擎路径"
  ```

- 如果仍然卡顿，尝试禁用所有代码分析器（编辑 .csproj）：
  ```xml
  <!-- 在 ItemGroup 中注释掉以下包 -->
  <!-- <PackageReference Include="Roslynator.Analyzers" ... /> -->
  <!-- <PackageReference Include="SonarAnalyzer.CSharp" ... /> -->
  <!-- <PackageReference Include="StyleCop.Analyzers" ... /> -->
  ```

---

## 🔍 故障排查

### 问题 1: 编译仍然卡顿
**可能原因**:
- Windows Defender 排除项未生效（需要管理员权限）
- 其他杀毒软件干扰（如 360、火绒）
- 磁盘 I/O 瓶颈（机械硬盘）

**解决方案**:
1. 验证 Defender 排除项是否生效：
   ```powershell
   Get-MpPreference | Select-Object -ExpandProperty ExclusionPath
   ```
2. 关闭其他杀毒软件的实时监控
3. 将项目移动到 SSD（如果当前在机械硬盘）

### 问题 2: 编译后运行时报错
**可能原因**: 缓存清理后，Godot 需要重新生成 C# 绑定

**解决方案**:
1. 在 Godot 编辑器中点击 "Build" → "Rebuild"
2. 如果仍然报错，执行 `CleanBuildCache.bat` 后重新打开编辑器

### 问题 3: dotnet build-server shutdown 报错
**可能原因**: build server 未运行（正常现象）

**解决方案**: 忽略此错误，继续执行后续步骤

---

## 📊 预期效果

| 优化项 | 预期提升 |
|--------|---------|
| 禁用共享编译服务器 | 减少 10-20 秒挂起时间 |
| 关闭代码分析器 | 减少 30-50% 编译时间 |
| Windows Defender 排除 | 减少 50-70% I/O 等待时间 |
| 并行编译 | 多核 CPU 利用率提升 20-40% |

**总体预期**: 编译时间从 1 分钟降至 10-20 秒（取决于硬件配置）

---

## 📝 维护建议

1. **定期清理缓存**: 每次修改 .csproj 或更新 NuGet 包后，运行 `CleanBuildCache.bat`
2. **监控 Defender 日志**: 如果编译仍然慢，检查 Windows 安全中心 → 保护历史记录，查看是否有文件被扫描
3. **代码分析器**: 开发阶段关闭，提交代码前手动运行一次完整分析

---

**最后更新**: 2026-04-23  
**优化执行者**: Kiro AI Assistant
