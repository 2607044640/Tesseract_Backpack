---
inclusion: manual
---

# Gemini Update Report Generator

<trigger_phrases>
"Generate Gemini report"
"Create update for Gemini"
"Summarize for Gemini"
</trigger_phrases>

<instructions>
Generate report to `.kiro/TempFolder/GeminiReport_YYYY-MM-DD.md` using template below.

Gather context from:
- Current conversation history
- `docLastConversationState.md`
- `.kiro/TempFolder/` task notes
- Modified files in session

Focus Questions for Gemini section - this is the primary value.
</instructions>

<report_template>
```markdown
# Gemini Update Report
**Date:** 2026-03-16
**Focus:** [Main system/feature worked on]

## Changes Implemented
- [Feature/system added with file paths]
- [Refactoring done]
- [Bugs fixed]

## Technical Decisions
- [Pattern/architecture chosen]: [Reason]
- [Library/tool added]: [Version, purpose]
- [Performance optimization]: [Approach]

## New Dependencies
- [Plugin name] (v[version]) - [Purpose]
- [NuGet package] - [Use case]

## Questions for Gemini
1. [Specific technical question about implementation]
2. [Request for best practices research]
3. [Alternative approaches to evaluate]
4. [Performance/security concerns to investigate]
5. [Latest updates on dependencies/tools]

## Current State
Working: [What's functional]
In Progress: [What's being developed]
Planned: [Next steps]
Tech Debt: [Known issues]
```
</report_template>

<examples>
**Changes Implemented:**
```
- MovementComponent.cs: Added gravity (9.8f) and jump physics
- AnimationControllerComponent.cs: Implemented FSM with 5 states
- Player3D.tscn: Integrated PhantomCamera3D for third-person view
```

**Technical Decisions:**
```
- Composition over Inheritance: Avoid deep hierarchies, enable component reuse
- PhantomCamera plugin (v2.x): GDScript camera with C# wrapper, collision detection
- Event-driven communication: Components emit Action<T>, subscribe in OnEntityReady()
```

**Questions for Gemini:**
```
1. Godot C# composition pattern vs Unity ECS - performance comparison?
2. PhantomCamera alternatives for Godot 4.6? Any newer solutions?
3. Best practices for animation state machines in Godot - AnimationTree vs custom FSM?
4. Event-driven components - memory leak risks with C# events in Godot?
5. Godot 4.6.1 C# limitations - any known issues with reflection or generics?
```
</examples>

