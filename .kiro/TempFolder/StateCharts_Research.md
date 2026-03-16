# Godot State Charts Plugin - C# API Reference

**Plugin:** godot-statecharts by derkork
**Documentation:** https://derkork.github.io/godot-statecharts/
**Repository:** https://github.com/derkork/godot-statecharts

## Core Concepts

State Charts are enhanced finite state machines that solve state explosion problems. Built with GDScript but provides C# wrapper classes for type-safe interaction.

## C# Wrapper Classes

### StateChart Wrapper

```csharp
using GodotStateCharts;

// Get the state chart node and wrap it
var stateChartNode = GetNode("StateChart");
var stateChart = StateChart.Of(stateChartNode);

// One-liner version
var stateChart = StateChart.Of(GetNode("StateChart"));
```

**Main Methods:**
- `SendEvent(string eventName)` - Trigger state transitions
- `SetExpressionProperty(string name, Variant value)` - Set properties for expression guards

### StateChartState Wrapper

```csharp
using GodotStateCharts;

// Wrap a state node for signal connections
var state = StateChartState.Of(GetNode("%ActiveState"));

// Connect to signals using SignalName constants
state.Connect(StateChartState.SignalName.StateEntered, Callable.From(OnStateEntered));
state.Connect(StateChartState.SignalName.StateExited, Callable.From(OnStateExited));
state.Connect(StateChartState.SignalName.StateProcessing, Callable.From(OnStateProcessing));
```

### Transition Wrapper

```csharp
using GodotStateCharts;

// Trigger transitions from code
var transition = Transition.Of(GetNode("StateChart/MyState/MyTransition"));
transition.Take(); // Execute immediately
transition.Take(immediately: false); // Respect delay
```

## State Signals

All states emit these signals:

- `state_entered()` - When state is activated
- `state_exited()` - When state is deactivated
- `state_processing(delta)` - Every frame while active (respects pause)
- `state_physics_processing(delta)` - Every physics frame while active
- `event_received(event)` - When event is received by active state
- `state_input(input_event)` - Input while state is active
- `state_unhandled_input(input_event)` - Unhandled input while active
- `transition_pending(initial_delay, remaining_delay)` - For delayed transitions

Compound states also have:
- `child_state_entered()` - Any child state entered
- `child_state_exited()` - Any child state exited

## State Types

1. **Atomic State** - Leaf state, no children
2. **Compound State** - Has children, only one active at a time (requires Initial State)
3. **Parallel State** - Has children, all active simultaneously
4. **History State** - Pseudo-state that restores last active state

## Transitions

### Event-Based Transitions
- Set "Event" field on transition node
- Call `stateChart.SendEvent("event_name")` to trigger

### Automatic Transitions
- Leave "Event" field empty
- Evaluated on state enter, event send, or property change
- Usually combined with guards

### Delayed Transitions
- Set "Delay" field (supports expressions)
- Pending transitions cancelled if state exits or another transition triggers

### Guards
- Expression guards: Use GDScript expressions
- Check expression properties: `player_health < 50`
- Set properties: `stateChart.SetExpressionProperty("player_health", 100)`

## Important Notes

⚠️ **Initialization Timing:**
- Initial state entered ONE FRAME after `_Ready()`
- Use `call_deferred` if sending events in `_Ready()`:
  ```csharp
  Callable.From(() => stateChart.SendEvent("init")).CallDeferred();
  ```

⚠️ **Event Bubbling:**
- Events sent to active leaf states first
- If not handled, bubbles up to parent states
- Stops when consumed or reaches root

⚠️ **Expression Guards:**
- Written in GDScript syntax even in C# projects
- Initialize all properties before first guard evaluation
- Can set initial values in StateChart inspector (v0.16.0+)

## Debugging

### In-Game Debugger
- Add "StateChartDebugger" control node to scene
- Set "Initial node to watch" property
- Shows current state, expression properties, transition delays
- Tracks state change history

```csharp
using GodotStateCharts;

var debugger = StateChartDebugger.Of(GetNode("StateChartDebugger"));
debugger.DebugNode(unit); // Change watched node
debugger.AddHistoryEntry("Custom event"); // Add history entry
```

### In-Editor Debugger
- Set "Track in Editor" on StateChart node
- View in editor debugger panel while game runs
- Limited compared to in-game debugger (no expression properties)

## Integration Pattern for Components

Recommended approach:
1. Add StateChart as child of entity
2. Components get reference via extension method
3. Components send events based on input/logic
4. Components subscribe to state signals to enable/disable behavior
5. Use parallel states to separate concerns (movement, animation, combat)

## Sources

Content rephrased for compliance with licensing restrictions from:
- https://derkork.github.io/godot-statecharts/usage/events-and-transitions
- https://derkork.github.io/godot-statecharts/usage/nodes
- https://derkork.github.io/godot-statecharts/usage/debugging
- https://github.com/derkork/godot-statecharts
