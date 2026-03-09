# 🔧 Godot.Composition 工作原理

## 核心技术：C# Source Generator

Godot.Composition 使用 **C# 源生成器（Source Generator）** 技术，在**编译期**自动生成样板代码。

### 什么是 Source Generator？

Source Generator 是 C# 编译器（Roslyn）的一个功能，允许在编译时分析代码并生成新的 C# 源文件。

**编译流程：**
```
你的代码 (.cs)
    ↓
Roslyn 编译器扫描
    ↓
Source Generator 分析特性 ([Entity], [Component])
    ↓
自动生成额外的 partial class 代码
    ↓
合并原始代码 + 生成代码
    ↓
编译成 DLL
```

---

## Godot.Composition 生成了什么代码？

### 示例 1：Entity 类

**你写的代码：**
```csharp
[Entity]
public partial class Player3D : CharacterBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}
```

**Source Generator 自动生成的代码（简化版）：**
```csharp
public partial class Player3D
{
    private void InitializeEntity()
    {
        // 1. 扫描所有子节点，找到所有 Component
        var components = GetChildren().OfType<IComponent>();
        
        // 2. 为每个 Component 注入 parent 引用
        foreach (var comp in components)
        {
            comp.SetParent(this);
        }
        
        // 3. 解析组件依赖（ComponentDependency）
        foreach (var comp in components)
        {
            comp.ResolveDependencies(components);
        }
        
        // 4. 调用所有组件的 OnEntityReady()
        foreach (var comp in components)
        {
            comp.OnEntityReady();
        }
    }
}
```

---

### 示例 2：Component 类

**你写的代码：**
```csharp
[Component(typeof(CharacterBody3D))]
[ComponentDependency(typeof(PlayerInputComponent))]
public partial class MovementComponent : Node
{
    public override void _Ready()
    {
        InitializeComponent();
    }
    
    public void OnEntityReady()
    {
        // 直接使用 parent 和 playerInputComponent（魔法变量）
        playerInputComponent.OnMovementInput += HandleMovementInput;
    }
    
    private void HandleMovementInput(Vector2 dir)
    {
        parent.Velocity = new Vector3(dir.X, 0, dir.Y);
    }
}
```

**Source Generator 自动生成的代码（简化版）：**
```csharp
public partial class MovementComponent : IComponent
{
    // 魔法变量 1：parent（来自 [Component(typeof(CharacterBody3D))]）
    protected CharacterBody3D parent;
    
    // 魔法变量 2：playerInputComponent（来自 [ComponentDependency(typeof(PlayerInputComponent))]）
    protected PlayerInputComponent playerInputComponent;
    
    private void InitializeComponent()
    {
        // 等待 Entity 调用 SetParent
    }
    
    void IComponent.SetParent(Node parentNode)
    {
        parent = (CharacterBody3D)parentNode;
    }
    
    void IComponent.ResolveDependencies(IEnumerable<IComponent> allComponents)
    {
        // 自动查找 PlayerInputComponent
        playerInputComponent = allComponents.OfType<PlayerInputComponent>().FirstOrDefault();
        
        if (playerInputComponent == null)
        {
            GD.PushError("MovementComponent: 找不到依赖的 PlayerInputComponent");
        }
    }
}
```

---

## 生成的代码在哪里？

生成的代码位于项目的临时编译目录：

```
.godot/mono/temp/obj/Debug/net8.0/
    └── generated/
        └── Godot.SourceGenerators/
            └── Godot.SourceGenerators.CompositionGenerator/
                ├── Player3D.g.cs
                ├── MovementComponent.g.cs
                ├── PlayerInputComponent.g.cs
                └── ...
```

**注意：**
- 这些文件是**临时的**，每次编译都会重新生成
- 你**不需要**手动编辑或提交这些文件到 Git
- 如果编译失败，检查这些文件可以帮助调试

---

## 为什么必须使用 `partial class`？

Source Generator 只能为 `partial class` 添加代码。如果你忘记写 `partial`，编译器会报错：

```
错误 CS0260: 缺少 partial 修饰符
```

**原理：**
- `partial class` 允许一个类的定义分散在多个文件中
- 你写一部分（业务逻辑）
- Source Generator 写另一部分（样板代码）
- 编译器将它们合并成一个完整的类

---

## 关键优势

### 1. 零运行时开销
- 所有代码在**编译期**生成，不是反射
- 没有性能损失

### 2. 类型安全
- `parent` 和依赖变量都是**强类型**的
- 编译器会检查类型错误

### 3. IDE 支持
- 生成的变量可以被 IDE 识别（自动补全、跳转定义）
- 需要重新编译一次才能让 IDE 看到

### 4. 减少样板代码
- 不需要手动写 `GetNode<T>()`
- 不需要手动管理组件依赖
- 不需要手动调用 `OnEntityReady()`

---

## 调试技巧

### 查看生成的代码
1. 编译项目：`dotnet build`
2. 打开生成的文件：`.godot/mono/temp/obj/Debug/net8.0/generated/...`
3. 查看 Source Generator 为你的类生成了什么

### 常见错误

**错误 1：找不到 `parent` 变量**
- 原因：忘记写 `partial class`
- 解决：添加 `partial` 关键字

**错误 2：找不到依赖的组件变量（如 `playerInputComponent`）**
- 原因：忘记添加 `[ComponentDependency(typeof(...))]`
- 解决：在类上添加依赖声明

**错误 3：`OnEntityReady()` 没有被调用**
- 原因：忘记在 Entity 的 `_Ready()` 中调用 `InitializeEntity()`
- 解决：确保 Entity 正确初始化

---

## 总结

Godot.Composition 的"魔法"本质上是**编译期代码生成**：

1. 你写简洁的声明式代码（特性 + partial class）
2. Source Generator 在编译时生成繁琐的样板代码
3. 编译器将两者合并成完整的类
4. 最终程序运行时没有任何额外开销

这就是为什么你可以直接使用 `parent` 和 `playerInputComponent` 这些"魔法变量"——它们在编译时已经被自动声明和赋值了。
