# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6.3 LTS (Editor Version 6000.3.8f1) UI Effects framework built on Universal Render Pipeline (URP). The project is in early development — the `UIFX/Runtime/Scripts/Core/` and `UIFX/Runtime/Scripts/FX/` directories are scaffolded but not yet populated.

## Common Commands

Unity has no CLI build commands — all building, running, and testing is done through the Unity Editor GUI. To open the project, launch Unity Hub and open the `C:\projects\Unity_UIFX` directory.

**Debugging**: Attach VS Code or Visual Studio debugger to the running Unity Editor process using `.vscode/launch.json` (configured for Unity debugger attachment on port 4024).

**Running tests**: Use Unity's Test Runner window (`Window > General > Test Runner`). The `com.unity.test-framework` package (1.6.0) is included but no tests are written yet.

**Build scenes**: Two scenes are configured in EditorBuildSettings:
- Index 0: `Assets/Scenes/SampleScene.unity`
- Index 1: `Assets/Scenes/Vedaant/Vedaant_Demo.unity`

## Architecture

### Core Runtime Scripts (`Assets/Scripts/`)

**`UIPathProgress.cs`** — The main effect component. Manages animated path progress visualization across a series of UI nodes connected by line segments.
- Holds references to node transforms and line Image components
- `SetRange(float start, float end)` / `SetProgress(float progress)` control which portion of the path is "filled"
- `AnimateRangeTo(float newStart, float newEnd, float duration)` runs a coroutine animation
- `Rebuild()` recreates line instances at runtime; `UpdateRender()` repaints without rebuilding
- Supports both linear and looping (circular) path topologies
- Line fill is distance-normalized: each segment's fill amount is computed relative to the total path length

**`MinMaxSliderController.cs`** — Thin binding layer between two UI Sliders and a `UIPathProgress` component. Enforces `startProgress <= endProgress` and syncs slider values bidirectionally.

### UIFX Package Structure (`Assets/UIFX/`)

```
UIFX/
├── Demo/
│   ├── Scenes/         # UIFX_Catalog.unity, Vedaant_Demo.unity
│   └── Scripts/        # (empty — demo scripts go here)
└── Runtime/
    ├── Materials/       # (empty — effect materials go here)
    ├── Prefabs/         # (empty — reusable effect prefabs go here)
    ├── Scripts/
    │   ├── Core/        # (empty — base classes, interfaces go here)
    │   └── FX/          # (empty — individual effect implementations go here)
    └── Shadows/         # (empty — shadow-specific effects go here)
```

New effects should be implemented under `UIFX/Runtime/Scripts/FX/` with base classes in `Core/`. Effect materials belong in `UIFX/Runtime/Materials/`.

### Rendering

- URP with separate PC and Mobile render pipeline assets (`Assets/Settings/PC_RPAsset.asset`, `Mobile_RPAsset.asset`)
- TextMesh Pro is included for UI text
- UI is built with Unity's UGUI system (`com.unity.ugui` 2.0.0)

### Prefabs

- `Assets/Prefabs/Vedaant/LineBase.prefab` — UI Image used as a path segment line
- `Assets/Prefabs/Vedaant/NodeBase.prefab` — UI node with a Fill child Image

These prefabs are the building blocks for `UIPathProgress` path visualizations.
