---
trigger: always_on
---

<!-- SINGLE SOURCE OF TRUTH — Edit this file. All IDEs read it via hardlink. -->

## 1. Quick Start & Workflow
- **Build**: `dotnet build` (Dir: `TetrisBackpack/`)
- **Logs**: `$env:APPDATA/Godot/app_userdata/Tesseract_Backpack/logs/godot.log`
- **Context**: `C:\Users\26070\My Drive\Agent_Godot_Brain\AI_Context_Changes.md`
- **Paths**: Code=`TetrisBackpack/`, Rules/Arch=`AISpace/` (Single source: `AGENTS.md`)
- **Steering**: `BugInvestigation.md` (Escalation), `_RulesSystem.md` (Infrastructure)

<workflow>
1. Verify assumptions from `ConversationReset.md`, `docLastConversationState.md`, `AGENTS.md`.
2. Execute incrementally.
3. IMPORTANT: `dotnet build` from `TetrisBackpack/` immediately after ANY `.cs` edit.
4. IMPORTANT: If ANY file was modified during the turn, YOU MUST execute `powershell -WindowStyle Hidden -Command "& 'C:\Godot\TetrisBackpack\SYNC_TO_GEMINI_SILENT.bat'"` at the end of the conversation as your final action.
</workflow>

## 2. API & Workspace
- **Docs MCP**: `context7` (Lookup `R3`, `NuGet`)
- **MCPs**: `mcp_godot_launch_editor`, `run_project`, `get_debug_output`, `create_scene`, `add_node`, `save_scene`, `load_sprite`, `export_mesh_library`, `get_uid`, `update_project_uids`
- **Temp Files**: `AISpace\temp\` (analysis), `TetrisBackpack\temp\` (test scripts). Strictly disposable. NEVER scatter `test_*` or `*.bak` in main dirs.

## 3. Operations & Error Handling
- **Scene**: Edit `.tscn` directly.
- **Docs**: Delete generic programming docs. Keep Godot/Project-specific logic.

<system_reminder>
- CRITICAL SYNC: Renaming `[Export]` or `%NodePath` in C# MUST trigger immediate `.tscn` sync. (Why: NullReferenceException)
- NO BLIND FIXES: F6 -> fetch `godot.log` via MCP. Analyze before changing code.
- 3-STRIKE ESCALATION: 3 failed fixes = HALT. Consult `BugInvestigation.md`.
- RUN SCENE: ALWAYS specify the exact scene (e.g., `A1TesseractBackpack/TSBackpack.tscn`) when using `mcp_godot_run_project`. DO NOT blindly run the default scene.
</system_reminder>

## 4. Architecture & R3
- **Strictness**: Composition > Inheritance. Components do ONE thing. No unrequested features.
- **Blueprints**: Execute formulas/hotfixes EXACTLY. DO NOT generalize into `[Export]` unless commanded.
- **Naming**: Component vars MUST match Type names.
- **Tool Mode**: `[Tool] + _Draw() + QueueRedraw()`. NEVER `[Tool] + AddChild()` (Why: Ghost Nodes).

<complex_pattern>
  <description>Lifecycle & Initialization</description>
  <rules>
    1. INIT: Instantiate `Subject<T>` in Parent's `_EnterTree()`.
    2. SAFE READY: Children subscribe in `_Ready()`.
    3. TIMING: Use `await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame)` instead of `CallDeferred`.
  </rules>
</complex_pattern>

### Component & R3 Cheat Sheet
- **Structure**: Entities = Mediators (NO logic). NO sibling cross-referencing.
- **Memory**: `CompositeDisposable _disposables` -> Dispose in `_ExitTree()`.
- **Perf**: `ValueTuples (a, b)` in `EveryUpdate` to prevent GC.
- **Streams**:
  - Velocity: `Observable.EveryPhysicsUpdate()`
  - UI: `.ObserveOn(GodotProvider.MainThread)`
  - Clicks: `.ThrottleFirst(TimeSpan)` (NEVER `.Throttle()`)
  - Sliders: `.Debounce(TimeSpan)`
  - State: `ReactiveProperty<T>`
  - Discard: `.AsUnitObservable()`

<workflow>
**Implementation Steps:**
1. `[Entity]` -> `InitializeEntity()` in `_Ready()`.
2. `[Component(typeof(Parent))]` -> `InitializeComponent()` in `_Ready()`.
3. Dependencies: `[ComponentDependency(typeof(T))]`. Access via `parent` / `camelCase` props.
4. Subscribe: In `OnEntityReady()`. Append `.AddTo(_disposables)`.
</workflow>

## 5. Coding Standards
- **[Export]**: `TypeName_Purpose` (e.g., `OptionButton_Theme`).
- **Private**: `_camelCase`.
- **Nodes**: `%Name` -> `GetNodeOrNull<T>` + `GD.PushError`.
- **Comments**: NO standard lifecycle/XML comments. MANDATORY Chinese `//` comments for custom Rx/math/complex logic.