---
inclusion: manual
---

# CoreComponents Architecture

<instructions>
This document defines the component architecture rules. Follow these patterns when creating entities and components.
</instructions>

---

## Core Rules

### Entity Pattern
```csharp
[Entity]
public partial class MyEntity : CharacterBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}
```

**Rules:**
- Must be `partial class`
- Must have `[Entity]` attribute
- Call `InitializeEntity()` in `_Ready()`
- No business logic - container only

---

### Component Pattern
```csharp
[Component(typeof(CharacterBody3D))]
[ComponentDependency(typeof(InputComponent))]
public partial class MovementComponent : Node
{
    public override void _Ready()
    {
        InitializeComponent();
    }
    
    public void OnEntityReady()
    {
        // inputComponent is auto-generated
        inputComponent.OnMove += HandleMove;
    }
}
```

**Rules:**
- Must be `partial class`
- Must have `[Component(typeof(ParentType))]`
- Call `InitializeComponent()` in `_Ready()`
- Subscribe to events in `OnEntityReady()`, not `_Ready()`
- Use `parent` to access entity (auto-generated)
- Use `componentName` to access dependencies (auto-generated, lowercase first letter)

---

## Communication Patterns

### Pattern 1: Event-Driven (Preferred)
```csharp
// Component A: Emit events
public event Action<Vector2> OnMove;
OnMove?.Invoke(direction);

// Component B: Subscribe
[ComponentDependency(typeof(ComponentA))]
public void OnEntityReady()
{
    componentA.OnMove += HandleMove;
}
```

**When to use:** Components need to notify others of state changes.

---

### Pattern 2: Direct Call
```csharp
[ComponentDependency(typeof(HealthComponent))]
public void ApplyDamage(float amount)
{
    healthComponent.TakeDamage(amount);
}
```

**When to use:** One component directly controls another.

---

### Pattern 3: Shared State via Parent
```csharp
public override void _Process(double delta)
{
    float speed = parent.Velocity.Length();
    // Use speed to switch animations
}
```

**When to use:** Multiple components read entity state.

---

## Component Lifecycle

```
1. Entity._Ready() calls InitializeEntity()
2. All components' _Ready() called → InitializeComponent()
3. Dependencies resolved
4. All components' OnEntityReady() called
5. Components start processing
```

**Critical:** Subscribe to events in `OnEntityReady()`, not `_Ready()`. Dependencies are null in `_Ready()`.

---

## Dependency Injection

### Declaration
```csharp
[ComponentDependency(typeof(InputComponent))]
public partial class MovementComponent : Node
```

### Usage
```csharp
public void OnEntityReady()
{
    // inputComponent is auto-generated (lowercase first letter)
    inputComponent.OnMove += HandleMove;
}
```

**Rules:**
- Never use `GetNode()` for components
- Never declare dependency variables manually
- Variable name = type name with lowercase first letter

---

## Component Reusability

### Interface Compatibility
Make components work with different input sources:

```csharp
// PlayerInputComponent and AIInputComponent both emit:
public event Action<Vector2> OnMovementInput;

// MovementComponent subscribes to either:
[ComponentDependency(typeof(IInputComponent))] // or specific type
public void OnEntityReady()
{
    inputComponent.OnMovementInput += HandleMove;
}
```

**Result:** MovementComponent works with Player (keyboard) and Enemy (AI) without modification.

---

### Type Agnostic Components
Design components that work with any parent type:

```csharp
// HealthComponent works with:
// - CharacterBody3D (Player, Enemy)
// - RigidBody3D (Box)
// - StaticBody3D (Destructible walls)

[Component(typeof(Node3D))] // Use base type
public partial class HealthComponent : Node
```

---

## Scene Structure

### Player Example
```
Player3D (CharacterBody3D) [Entity]
├── PlayerInputComponent
├── MovementComponent
├── CharacterRotationComponent
├── CameraControlComponent
├── AnimationControllerComponent
└── CameraPivot/SpringArm3D/Camera3D
```

### Enemy Example
```
Enemy (CharacterBody3D) [Entity]
├── AIInputComponent          ← Different input
├── MovementComponent         ← Reused
├── CharacterRotationComponent ← Reused
├── AnimationControllerComponent ← Reused
└── HealthComponent
```

**Key:** Same components, different input source.

---

## Anti-Patterns

### ❌ Don't: Entity as Mediator
```csharp
// Bad: Entity forwards events
public override void _Ready()
{
    input.OnMove += (dir) => movement.Move(dir);
}
```

### ✅ Do: Components Subscribe Directly
```csharp
// Good: Component subscribes itself
[ComponentDependency(typeof(InputComponent))]
public void OnEntityReady()
{
    inputComponent.OnMove += HandleMove;
}
```

---

### ❌ Don't: Manual GetNode for Components
```csharp
// Bad
private InputComponent _input;
public override void _Ready()
{
    _input = GetNode<InputComponent>("../Input");
}
```

### ✅ Do: Use Dependency Injection
```csharp
// Good
[ComponentDependency(typeof(InputComponent))]
public void OnEntityReady()
{
    inputComponent.OnMove += HandleMove;
}
```

---

### ❌ Don't: Subscribe in _Ready()
```csharp
// Bad: Dependency might be null
public override void _Ready()
{
    InitializeComponent();
    otherComponent.OnEvent += Handler; // May crash
}
```

### ✅ Do: Subscribe in OnEntityReady()
```csharp
// Good: Dependencies guaranteed initialized
public void OnEntityReady()
{
    otherComponent.OnEvent += Handler;
}
```

---

## Component Design Checklist

**Single Responsibility:**
- [ ] Component does one thing only
- [ ] Name clearly describes purpose

**Dependencies:**
- [ ] Use `[ComponentDependency]` for component references
- [ ] Never use `GetNode()` for components
- [ ] Subscribe in `OnEntityReady()`

**Reusability:**
- [ ] Works with multiple entity types
- [ ] No hardcoded entity-specific logic
- [ ] Uses events for communication

**Configuration:**
- [ ] Expose parameters via `[Export]`
- [ ] Provide sensible defaults
- [ ] Add `[GlobalClass]` for editor visibility

**Cleanup:**
- [ ] Unsubscribe events in `_ExitTree()`
- [ ] Release resources properly

---

## Component Template

```csharp
using Godot;
using Godot.Composition;
using System;

[GlobalClass]
[Component(typeof(Node3D))]
[ComponentDependency(typeof(OtherComponent))]
public partial class MyComponent : Node
{
    #region Events
    public event Action<int> OnSomething;
    #endregion

    #region Export Properties
    [Export] public float Value { get; set; } = 1.0f;
    #endregion

    #region Lifecycle
    public override void _Ready()
    {
        InitializeComponent();
    }
    
    public void OnEntityReady()
    {
        otherComponent.OnEvent += HandleEvent;
    }
    
    public override void _ExitTree()
    {
        if (otherComponent != null)
            otherComponent.OnEvent -= HandleEvent;
    }
    #endregion

    #region Event Handlers
    private void HandleEvent(int value)
    {
        OnSomething?.Invoke(value);
    }
    #endregion
}
```

---

## Debugging

### Check Component Initialization
```csharp
public override void _Ready()
{
    InitializeComponent();
    GD.Print($"{GetType().Name}: Initialized");
}
```

### Check Dependency Injection
```csharp
public void OnEntityReady()
{
    if (otherComponent == null)
    {
        GD.PushError($"{GetType().Name}: Dependency failed");
        return;
    }
    GD.Print($"{GetType().Name}: Dependencies OK");
}
```

### Check Event Subscription
```csharp
private void HandleEvent(int value)
{
    GD.Print($"{GetType().Name}: Event received: {value}");
}
```

---

## Performance Considerations

### Event Subscriptions
Unsubscribe in `_ExitTree()` to prevent memory leaks:
```csharp
public override void _ExitTree()
{
    if (component != null)
        component.OnEvent -= Handler;
}
```

### Caching References
Cache frequently accessed nodes in `_Ready()`:
```csharp
private Node3D _cachedNode;

public override void _Ready()
{
    InitializeComponent();
    _cachedNode = parent.GetNode<Node3D>("Model");
}
```

### Conditional Processing
Skip processing when disabled:
```csharp
[Export] public bool IsEnabled { get; set; } = true;

public override void _Process(double delta)
{
    if (!IsEnabled) return;
    // Process logic
}
```

---

## Migration from Traditional Pattern

### Before (Mediator Pattern)
```csharp
public partial class Player : CharacterBody3D
{
    private InputComponent _input;
    private MovementComponent _movement;
    
    public override void _Ready()
    {
        _input = GetNode<InputComponent>("Input");
        _movement = GetNode<MovementComponent>("Movement");
        _input.OnMove += (dir) => _movement.Move(dir);
    }
}
```

### After (Godot.Composition)
```csharp
// Entity
[Entity]
public partial class Player : CharacterBody3D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}

// Component
[Component(typeof(CharacterBody3D))]
[ComponentDependency(typeof(InputComponent))]
public partial class MovementComponent : Node
{
    public void OnEntityReady()
    {
        inputComponent.OnMove += HandleMove;
    }
}
```

**Benefits:**
- Entity: 180 lines → 10 lines
- No manual GetNode
- No event forwarding
- Components self-manage dependencies

---

## Reference

- **Godot.Composition:** https://github.com/MysteriousMilk/Godot.Composition
- **Examples:** `addons/CoreComponents/Examples/`
- **Quick Start:** `addons/CoreComponents/QUICK_START.md`
