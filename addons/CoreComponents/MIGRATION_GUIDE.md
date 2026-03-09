# 📦 项目搬运指南（Godot.Composition）

## 核心原则

Godot.Composition 是通过 **NuGet 包**引入的，生成的代码是**临时的**。

**搬运项目时：**
- ✅ 需要复制：`.csproj` 文件（包含 NuGet 包引用）
- ✅ 需要复制：所有 `.cs` 源代码文件
- ✅ 需要复制：`.tscn` 场景文件
- ❌ 不需要复制：`.godot/mono/temp/` 目录（生成的代码）
- ❌ 不需要复制：`bin/` 和 `obj/` 目录（编译输出）

---

## 方法 1：使用 NuGet（推荐）

### 步骤

1. **复制整个项目文件夹**
   ```cmd
   xcopy /E /I "C:\Godot\3d-practice" "D:\NewLocation\3d-practice"
   ```

2. **在新位置打开项目**
   - 用 Godot 编辑器打开 `project.godot`
   - Godot 会自动调用 `dotnet restore` 下载 NuGet 包

3. **首次编译**
   ```cmd
   cd D:\NewLocation\3d-practice
   dotnet build 3dPractice.sln
   ```

4. **验证**
   - 运行游戏，检查 Player3D 是否正常工作
   - 查看控制台输出：`Player3D Entity: 初始化完成 ✓`

### 前提条件

新环境需要安装：
- .NET 8.0 SDK 或更高版本
- Godot 4.6.1 或更高版本（Mono 版本）
- 网络连接（用于下载 NuGet 包）

---

## 方法 2：离线搬运（无网络环境）

如果目标环境没有网络，需要手动复制 NuGet 包。

### 步骤

1. **在源环境导出 NuGet 包**
   ```cmd
   cd C:\Godot\3d-practice
   dotnet restore
   ```

2. **找到 NuGet 缓存目录**
   ```cmd
   echo %USERPROFILE%\.nuget\packages
   ```
   通常在：`C:\Users\<用户名>\.nuget\packages\`

3. **复制 Godot.Composition 包**
   ```
   复制整个文件夹：
   C:\Users\<用户名>\.nuget\packages\godot.composition\
   ```

4. **在目标环境粘贴到相同位置**
   ```
   粘贴到：
   C:\Users\<新用户名>\.nuget\packages\godot.composition\
   ```

5. **复制项目并编译**
   ```cmd
   cd D:\NewLocation\3d-practice
   dotnet build 3dPractice.sln
   ```

---

## 方法 3：使用 Git（开发者推荐）

### .gitignore 配置

确保 `.gitignore` 包含：
```gitignore
# Godot 生成文件
.godot/
.mono/

# C# 编译输出
bin/
obj/
*.user

# NuGet 包（不提交）
packages/
```

### 克隆项目后

```cmd
git clone <repository-url>
cd 3d-practice
dotnet restore
dotnet build
```

Godot 会自动处理剩余的事情。

---

## 常见问题

### Q1：搬运后编译失败，提示找不到 `Godot.Composition`

**原因：** NuGet 包未下载

**解决：**
```cmd
dotnet restore
dotnet build
```

---

### Q2：编译成功，但运行时报错 `找不到 parent 变量`

**原因：** 生成的代码未被 IDE 识别

**解决：**
1. 关闭 Godot 编辑器
2. 删除 `.godot/mono/temp/` 目录
3. 重新打开 Godot 编辑器
4. 点击 "Build" 按钮重新编译

---

### Q3：在新电脑上，IDE（Rider/VS）提示找不到 `parent`

**原因：** IDE 缓存未更新

**解决：**
1. 在 IDE 中执行 "Reload Project"
2. 或者关闭 IDE，删除 `.idea/` 或 `.vs/` 目录，重新打开

---

### Q4：我想在多个项目中使用相同的组件

**方法 1：复制文件**
- 将 `addons/CoreComponents/` 文件夹复制到新项目
- 确保新项目的 `.csproj` 也引用了 `Godot.Composition`

**方法 2：创建共享库（高级）**
- 将组件打包成独立的 `.csproj` 项目
- 在多个项目中引用该项目

---

## 验证清单

搬运项目后，检查以下内容：

- [ ] `.csproj` 文件存在且包含 `<PackageReference Include="Godot.Composition" Version="1.3.1" />`
- [ ] 所有 `.cs` 文件都是 `partial class`
- [ ] Entity 类有 `[Entity]` 特性并调用 `InitializeEntity()`
- [ ] Component 类有 `[Component(typeof(...))]` 特性并调用 `InitializeComponent()`
- [ ] 运行 `dotnet build` 成功（0 errors）
- [ ] 在 Godot 中运行游戏，控制台输出正常

---

## 不同场景的搬运策略

### 场景 1：同一台电脑，不同文件夹
- 直接复制整个项目文件夹
- 打开 Godot 编辑器即可

### 场景 2：不同电脑，有网络
- 复制项目文件夹（或使用 Git）
- 在新电脑上运行 `dotnet restore`

### 场景 3：不同电脑，无网络
- 手动复制 NuGet 包到 `%USERPROFILE%\.nuget\packages\`
- 复制项目文件夹
- 运行 `dotnet build`

### 场景 4：团队协作（Git）
- 提交代码到 Git（不包含 `bin/`, `obj/`, `.godot/`）
- 团队成员克隆后运行 `dotnet restore`

---

## 总结

Godot.Composition 的搬运非常简单：

1. **复制项目文件**（包括 `.csproj`）
2. **运行 `dotnet restore`**（自动下载 NuGet 包）
3. **编译项目**（自动生成代码）

生成的代码是临时的，不需要手动管理。只要 `.csproj` 文件正确引用了 NuGet 包，一切都会自动工作。
