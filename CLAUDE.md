# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A turn-based mercenary corps management simulation built in Unity. The project is in early development; most classes outside `Assets/Scripts/Core/` are stubs. The game is written in Korean (comments, string literals, and field labels are in Korean).

## Unity Setup

- **Engine**: Unity 6 (URP 17.3.0)
- **Render Pipeline**: Universal Render Pipeline
- **Key Packages**: Input System, Unity Test Framework, AI Navigation, Timeline, uGUI 2.0

Open and run the project via the Unity Editor — there is no CLI build command for development. Tests are run through Unity's **Test Runner** window (`Window > General > Test Runner`).

## Architecture

### Event Bus (`Assets/Scripts/Core/GameEvents.cs`)
`GameEvents` is a static class that acts as the global event bus. All cross-system communication goes through it (turn phase changes, mercenary state updates, log messages). Systems subscribe to events rather than holding direct references to each other.

### Turn Flow (`Assets/Scripts/Core/TurnManager.cs`)
`TurnManager` is a singleton `MonoBehaviour` managing a three-phase loop:
1. **Planning** (`TurnPhase.Planning`) — UI assigns actions to mercenaries; ends when `EndPlanningPhase()` is called (wired to a UI button)
2. **Execution** (`TurnPhase.Execution`) — iterates registered mercenaries, calls `OnTurnExecute()` on each with a configurable `executionDelay` between them
3. **Resolution** (`TurnPhase.Resolution`) — brief pause before incrementing turn counter and returning to Planning

### Mercenary Abstraction (`Assets/Scripts/Core/IMercenary.cs`)
`IMercenary` is the sole interface between the data layer and the turn/UI systems. When adding new mercenary data implementations, implement this interface. The turn and UI systems must only depend on `IMercenary`, never on concrete types.

Key contract:
- `AssignAction(string actionId)` — called during Planning phase by UI
- `OnTurnExecute()` — called by `TurnManager` during Execution phase
- `IsIncapacitated` — when `true`, `TurnManager` skips the mercenary

### Mercenary Personality (`Assets/Scripts/Mercenary/DummyMercenaryData.cs`)
`DummyMercenaryData` is the current working implementation of `IMercenary`, used for prototyping. Each mercenary has a `PersonalityType` (신중형, 충동형, 낙천형, 비관형, 지도자형, 학자형, 냉정형) that determines their autonomous action when none is assigned by the player. Stamina decreases by 10 each turn; `IsIncapacitated` triggers at `Stamina <= 0`.

## Key Conventions

- Use `GameEvents` for all inter-system communication; do not create direct MonoBehaviour references between systems.
- All new mercenary types must implement `IMercenary` and register/unregister with `TurnManager` (`RegisterMercenary` / `UnregisterMercenary`).
- `MercenaryData.cs` and `UIManager.cs` are empty stubs — implement these as the project grows.
- Action IDs are plain strings (e.g., `"훈련"`, `"휴식"`); no enum exists yet.
