---
trigger: always_on
---

<!-- SINGLE SOURCE OF TRUTH — Edit this file. All IDEs read it via hardlink. -->

## 1. Quick Start & Workflow
- **Build**: `dotnet build` (Dir: `TetrisBackpack/`)
- **Logs**: `$env:APPDATA/Godot/app_userdata/Tesseract_Backpack/logs/godot.log`
- **Context**: `C:\Users\26070\My Drive\Agent_Godot_Brain\AI_Context_Changes.md`
- **Master Sync**: `AISpace\SyncMasterCopy\AI_Context_Master_Latest.txt` (full AI-ready bundle of useful project files, rules, and recent changes)
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
- **OpenAI Docs MCP**: Use `openaiDeveloperDocs` for OpenAI API, ChatGPT Apps SDK, and Codex questions without waiting for an explicit request.
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

## 4. Architecture & R3 Strictness

<prime_directive>
  <description>CRITICAL: Execution Strictness, KISS & Anti-Over-Engineering</description>
  <rationale>While component-based design is required, over-engineering breaks pixel-perfect layouts and ruins the Architect's intent.</rationale>
  <rules>
    1. EXACT BLUEPRINT OBEDIENCE: Execute hotfixes EXACTLY. Do NOT generalize into `[Export]` unless commanded.
    2. KISS & YAGNI: Keep It Simple. Never invent abstractions that complicate straightforward tasks.
    3. COMPONENT STRICTNESS: Composition > Inheritance. Components do ONE thing.
    4. NAMING ENFORCEMENT: Injected component variables MUST match their Type names.
  </rules>
</prime_directive>

<complex_pattern>
  <description>CRITICAL: Godot Lifecycle (_EnterTree vs _Ready) & Initialization</description>
  <rationale>Godot executes _EnterTree Top-Down but _Ready Bottom-Up. Using _Ready for parent data init causes NullReference in children.</rationale>
  <rules>
    1. INIT IN ENTER_TREE: All `Subject<T>` instantiations and core data (`new Subject<T>()`) MUST be done in the Parent's `public override void _EnterTree()`.
    2. SAFE CHILD READY: Children can safely subscribe to Parent Subjects in their own `_Ready()` because the Parent's `_EnterTree()` has already executed.
    3. TIMING: Use `await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);` instead of `CallDeferred`.
  </rules>
</complex_pattern>

### Component & R3 Cheat Sheet
- **Structure**: Entities = Mediators (NO logic). NO sibling cross-referencing.
- **Memory**: `CompositeDisposable _disposables` -> Dispose in `_ExitTree()`.
- **Perf**: `ValueTuples (a, b)` in `EveryUpdate` to prevent GC.
- **Streams**:
  - Physics (Velocity): `Observable.EveryPhysicsUpdate()`
  - UI Updates: Append `.ObserveOn(GodotProvider.MainThread)`
  - Button Clicks: `.ThrottleFirst(TimeSpan)` (NEVER `.Throttle()`)
  - Continuous I/O (Sliders): `.Debounce(TimeSpan)`
  - State Flags: Use `ReactiveProperty<T>`
  - Discard Payload: Chain `.AsUnitObservable()`

<workflow>
**Implementation Steps:**
1. `[Entity]` -> call `InitializeEntity()` in `_Ready()`.
2. `[Component(typeof(Parent))]` -> call `InitializeComponent()` in `_Ready()`.
3. Dependencies: Request via `[ComponentDependency(typeof(T))]`. Access via auto-generated `parent` and `camelCase` properties.
4. Subscribe: In `OnEntityReady()`. Append `.AddTo(_disposables)`.
</workflow>

## 5. Coding Standards

### Naming & Access
- **[Export]**: `TypeName_Purpose` (e.g., `OptionButton_Theme`).
- **Private**: `_camelCase`.
- **Nodes**: `%Name` -> `GetNodeOrNull<T>` + `GD.PushError`.

### Documentation Rules
- **FORBIDDEN**: XML comments (`/// <summary>`) or inline comments for standard Godot lifecycle (`_Ready`, `_Process`).
- **MANDATORY**: Inline `//` comments (in Chinese) explaining WHY for custom Rx streams, math formulas, and complex logic.
