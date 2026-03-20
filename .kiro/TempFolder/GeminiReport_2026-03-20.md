# Gemini Update Report
**Date:** 2026-03-20
**Focus:** Godot State Charts Integration - "Power Switch" Architecture

## Changes Implemented

### Core Architecture Refactoring
- **ComponentExtensions.cs**: Completely refactored StateChart extensions
  - Removed: `GetStateChart()`, `ConnectToState()`, `GetState()`, `SetStateProperty()` (violated black-box principle)
  - Added: `SendStateEvent(string)` - Black-box event routing
  - Added: `BindComponentToState(Node, Node, string)` - Component lifecycle binding (power switch pattern)

- **MovementComponent.cs**: Purified to zero state logic
  - Removed: `_canMove` boolean flag
  - Removed: All `if (_canMove)` conditional branches
  - Added: `BindComponentToState(parent, "StateChart/Root/GameFlow/Exploration")` in `OnEntityReady()`
  - Result: Pure physics calculation, no state checks

- **PlayerInputComponent.cs**: Added state event triggering example
  - Added: `SendStateEvent("start_minesweeper")` on interact key press
  - Demonstrates input → state machine decoupling

### Documentation Created
- `.kiro/TempFolder/StateCharts_Research.md` - Plugin API research notes
- `.kiro/TempFolder/StateChart_PowerSwitch_Architecture.md` - Complete architecture guide
- `.kiro/TempFolder/StateCharts_Integration_Guide.md` - Initial integration guide (now outdated)
- `addons/CoreComponents/Examples/StateChart_Usage_Example.md` - Usage examples (now outdated)

## Technical Decisions

### 1. State Machine as "Power Switch" Pattern
**Decision:** State machine controls component lifecycle via `SetProcess()` calls, not boolean flags
**Reason:** 
- Eliminates all state conditionals from component code
- Components default to dormant, activated only when state machine enables them
- True separation of concerns: state machine = lifecycle manager, components = pure logic executors

### 2. Black-Box State Routing
**Decision:** Components only use `SendStateEvent()`, never access StateChart directly
**Reason:**
- Components remain ignorant of state machine implementation
- State transitions managed entirely in visual StateChart editor
- Easy to refactor state structure without touching component code

### 3. Godot State Charts Plugin (GDScript-based)
**Decision:** Use derkork/godot-statecharts plugin with C# wrapper classes
**Reason:**
- Mature plugin with visual editor integration
- Supports hierarchical states (Compound, Parallel, History)
- C# wrapper (`StateChart.Of()`, `StateChartState.Of()`) provides type safety
- Expression guards and delayed transitions built-in

### 4. Component Lifecycle Binding Pattern
**Decision:** `BindComponentToState()` automatically manages all Process flags
**Implementation:**
```csharp
// Default: dormant
component.SetProcess(false);
component.SetPhysicsProcess(false);
component.SetProcessInput(false);
component.SetProcessUnhandledInput(false);

// State entered: wake up
state.StateEntered += () => { /* enable all */ };

// State exited: sleep
state.StateExited += () => { /* disable all */ };
```

## New Dependencies

- **godot-statecharts** (v0.22.0+) - State machine plugin by derkork
  - GDScript implementation with C# wrapper classes
  - Installed via Godot Asset Library
  - Namespace: `GodotStateCharts`
  - Key classes: `StateChart`, `StateChartState`, `Transition`

## Questions for Gemini

### 1. Performance Analysis: SetProcess() Overhead
**Context:** Our "power switch" pattern calls `SetProcess(true/false)` on every state transition. Each component has 4 process flags toggled.

**Questions:**
- What's the performance cost of frequent `SetProcess()` calls in Godot 4.6?
- Is there internal caching, or does it rebuild process lists every time?
- For 50+ components switching states 10x/second, is this a bottleneck?
- Alternative: Should we use a single "enabled" flag and check it in `_Process()`?

### 2. Godot State Charts vs Alternatives
**Context:** We chose godot-statecharts (GDScript plugin) over pure C# state machines.

**Questions:**
- Performance comparison: GDScript state machine + C# wrapper vs pure C# FSM?
- Are there newer C#-native state machine plugins for Godot 4.6?
- How does godot-statecharts compare to AnimationTree's state machine for game logic?
- Any known issues with GDScript/C# interop in state-heavy games?

### 3. Power Switch Pattern - Real-World Validation
**Context:** Our pattern: components dormant by default, state machine wakes them up.

**Questions:**
- Are there production Godot games using this lifecycle management pattern?
- Potential pitfalls: What happens if state transition fails mid-execution?
- Memory implications: Do dormant components still consume resources?
- Best practices for handling component initialization when waking from dormant state?

### 4. Parallel States Best Practices
**Context:** We plan to use Parallel States for independent concerns (GameMode, CharacterState, UIState).

**Questions:**
- Performance impact of Parallel States vs nested Compound States?
- How many parallel branches before performance degrades?
- Common mistakes when designing parallel state hierarchies?
- Debugging parallel states: tools/techniques for visualizing concurrent state?

### 5. StateChart + ECS Deep Integration
**Context:** We're combining godot-statecharts with Godot.Composition (ECS framework).

**Questions:**
- Has anyone integrated state machines with ECS in Godot C#?
- Should state machine be an entity component, or external system?
- How to handle state-driven component spawning/despawning?
- Best practices for state machine → component communication beyond lifecycle?

### 6. Expression Guards and C# Properties
**Context:** StateChart supports expression guards written in GDScript syntax.

**Questions:**
- Can expression guards access C# properties via reflection?
- Performance cost of expression evaluation per frame?
- Alternative: Should we use code-triggered transitions (`Transition.Take()`) instead?
- Best practices for complex conditional transitions in C# projects?

### 7. State Machine Debugging Tools
**Context:** Plugin provides StateChartDebugger (in-game) and editor debugger.

**Questions:**
- Are there better state visualization tools for Godot 4.6?
- Can we export state transition logs for automated testing?
- Tools for detecting state machine deadlocks or unreachable states?
- Integration with Godot's profiler for state-specific performance analysis?

### 8. Latest Godot State Charts Updates
**Context:** We're using v0.22.0, released ~2024.

**Questions:**
- Any critical updates or breaking changes in recent versions?
- Godot 4.6.1 compatibility issues?
- Roadmap: Is the plugin actively maintained?
- Community: Are there forks with C#-specific improvements?

## Current State

### Working
- ✅ ComponentExtensions with `SendStateEvent()` and `BindComponentToState()`
- ✅ MovementComponent purified (zero state conditionals)
- ✅ PlayerInputComponent sending state events
- ✅ Code compiles successfully (dotnet build passed)
- ✅ Architecture documented in `StateChart_PowerSwitch_Architecture.md`

### In Progress
- 🔄 Need to create actual StateChart scene structure in Godot editor
- 🔄 Test state transitions with real gameplay (Exploration ↔ Minesweeper ↔ MatchThree)
- 🔄 Verify component wake/sleep behavior in runtime

### Planned
- 📋 Create StateEvents constants class (eliminate magic strings)
- 📋 Refactor other components (CameraControl, AnimationController) to use power switch pattern
- 📋 Add StateChartDebugger to main scene for runtime visualization
- 📋 Create example scene demonstrating 3-mode game flow (Exploration/Minesweeper/MatchThree)

### Tech Debt
- ⚠️ Old integration guide files need deletion/update:
  - `.kiro/TempFolder/StateCharts_Integration_Guide.md` (outdated approach)
  - `addons/CoreComponents/Examples/InputToStateOperator.cs` (obsolete pattern)
  - `addons/CoreComponents/Examples/StateChart_Usage_Example.md` (outdated examples)
- ⚠️ No automated tests for state transitions yet
- ⚠️ StateChart scene structure not yet created (only code ready)

## Additional Context

### Project Type
3D + UI hybrid game (Match-3 + Minesweeper + 3D Exploration)

### Architecture Stack
- Godot 4.6.1 stable mono (C# only)
- Godot.Composition framework (ECS-style components)
- PhantomCamera3D (third-person camera)
- godot-statecharts (state machine)

### Key Architectural Principles
1. Composition over Inheritance
2. Components emit events, never call siblings
3. State machine = lifecycle manager (power switch)
4. Zero magic strings (use constants)
5. Black-box routing (components don't access state machine internals)
