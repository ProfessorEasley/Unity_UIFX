# Unity UIFX

A Unity 6.3 LTS UI effects framework built on the Universal Render Pipeline (URP). Each team member implemented a self-contained effect; all effects are accessible from a single **UIFX** top-level menu in the Unity Editor.

**Engine:** Unity 6000.3.8f1 (Unity 6.3 LTS) · **Pipeline:** URP · **UI system:** UGUI + TextMesh Pro

---

## Opening a Demo

Every effect has a demo scene reachable from the editor menu bar:

| Menu item | Effect | Author |
|---|---|---|
| **UIFX › Shimmer › Create Demo Scene** | UIShimmer | Jing Yang |
| **UIFX › Path Progress › Open Demo Scene** | UIPathProgress | Vedaant Rajeshirke |
| **UIFX › Shine Shader › Open Demo Scene** | Button Shine/Ripple | Sriram Subramanian |
| **UIFX › Sparkle Particles › Open Demo Scene** | uGUI Sparkle Particles | Aryan Shah |
| **UIFX › Coin Counter › Open Demo Scene** | Animated Coin Counter | Vasudha Padala |
| **UIFX › Sprite Glow › Open Demo Scene** | Sprite Glow Outline | Aryaman Kunwar |
| **UIFX › Dynamic Order › Open Demo Scene** | Dynamic Ordering System | Naveen Prakaasham Vairaprakasam |

If the editor is in Play mode when you click a menu item it exits play mode first, then loads the scene.

---

## Showcase Videos

The browser teaser gallery is available at [Assets/UIFX/Demo/Videos/index.html](Assets/UIFX/Demo/Videos/index.html). It condenses all seven showcase clips into feature tabs.

For Markdown renderers that allow inline HTML video, the same clips also appear below as a compact teaser grid. Videos are muted, looped, and set to autoplay where the browser permits it.

<table>
  <tr>
    <td width="50%">
      <strong>UIShimmer</strong><br>
      <video src="Assets/UIFX/Demo/Videos/UIShimmer/uishimmer-demo.webm" controls autoplay muted loop playsinline preload="metadata" width="100%"></video><br>
      <a href="Assets/UIFX/Demo/Videos/UIShimmer/README.md">Video README</a>
    </td>
    <td width="50%">
      <strong>UIPathProgress</strong><br>
      <video src="Assets/UIFX/Demo/Videos/PathProgress/path-progress-demo.webm" controls autoplay muted loop playsinline preload="metadata" width="100%"></video><br>
      <a href="Assets/UIFX/Demo/Videos/PathProgress/README.md">Video README</a>
    </td>
  </tr>
  <tr>
    <td width="50%">
      <strong>Button Shine/Ripple</strong><br>
      <video src="Assets/UIFX/Demo/Videos/ShineShader/shine-shader-hover.webm" controls autoplay muted loop playsinline preload="metadata" width="100%"></video><br>
      <a href="Assets/UIFX/Demo/Videos/ShineShader/README.md">Video README</a>
    </td>
    <td width="50%">
      <strong>uGUI Sparkle Particles</strong><br>
      <video src="Assets/UIFX/Demo/Videos/SparkleParticles/sparkle-particles-demo.webm" controls autoplay muted loop playsinline preload="metadata" width="100%"></video><br>
      <a href="Assets/UIFX/Demo/Videos/SparkleParticles/README.md">Video README</a>
    </td>
  </tr>
  <tr>
    <td width="50%">
      <strong>Animated Coin Counter</strong><br>
      <video src="Assets/UIFX/Demo/Videos/CoinCounter/coin-counter-demo.webm" controls autoplay muted loop playsinline preload="metadata" width="100%"></video><br>
      <a href="Assets/UIFX/Demo/Videos/CoinCounter/README.md">Video README</a>
    </td>
    <td width="50%">
      <strong>Sprite Glow Outline</strong><br>
      <video src="Assets/UIFX/Demo/Videos/SpriteGlow/sprite-glow-demo.webm" controls autoplay muted loop playsinline preload="metadata" width="100%"></video><br>
      <a href="Assets/UIFX/Demo/Videos/SpriteGlow/README.md">Video README</a>
    </td>
  </tr>
  <tr>
    <td colspan="2">
      <strong>Dynamic Ordering System</strong><br>
      <video src="Assets/UIFX/Demo/Videos/DynamicOrder/dynamic-order-demo.webm" controls autoplay muted loop playsinline preload="metadata" width="100%"></video><br>
      <a href="Assets/UIFX/Demo/Videos/DynamicOrder/README.md">Video README</a>
    </td>
  </tr>
</table>

---

## Effects

### UIShimmer — Jing Yang
`Assets/UIFX/Runtime/Scripts/FX/UIShimmerEffect.cs`

A click-triggered animated shimmer that sweeps across any UGUI `Image`. The effect runs via a custom shader (`UIFX/UIShimmer`) applied as a per-instance material. Parameters (color, angle, width, intensity, duration) are all configurable per-component.

- The shader material is created automatically when the editor loads — no manual setup required.
- **Demo:** six preset cards (Classic, Gold Glint, Wide Glow, etc.) plus a Trigger All button.

---

### UIPathProgress — Vedaant Rajeshirke
`Assets/Scripts/UIPathProgress.cs` · `Assets/Scripts/MinMaxSliderController.cs`

Animates a filled path across a series of UI nodes connected by line segments. Useful for quest trackers, tutorial flows, or level-select screens.

**Key API**
```csharp
pathProgress.SetRange(float start, float end);   // set both endpoints
pathProgress.SetProgress(float progress);         // advance the end point only
pathProgress.Rebuild();                           // recreate line instances
```

- Line fill is distance-normalised — each segment fills proportionally to its actual pixel length.
- Nodes pulse (scale bounce) when their segment is completed; the pulse re-fires if the slider is scrubbed back and forward.
- An optional `leadHead` transform tracks the tip of the progress line (e.g. for attaching a particle emitter).
- `MinMaxSliderController` binds two UI Sliders to `SetRange`, enforcing `start ≤ end`.

**Prefabs:** `Assets/Prefabs/Vedaant/LineBase.prefab`, `NodeBase.prefab`

---

### Button Shine Shader — Sriram Subramanian
`Assets/SriramShineShader/ButtonHoverRipple.cs` · `ButtonHoverScript.cs`

A hover-triggered diagonal shine/ripple effect for UGUI buttons. Attach `ButtonHoverRipple` (or `ButtonShinyControl`) to any `Image` that uses the `Diag_UI_ShinyButton_DiagonalFlash` material. On pointer-enter the material's `_ManualProgress` and `_HoverAmount` properties are driven over `rippleDuration` seconds; on pointer-exit the shine resets.

**Material:** `Assets/SriramShineShader/Diag_UI_ShinyButton_DiagonalFlash.mat`

---

### uGUI Sparkle Particles — Aryan Shah
`Assets/Aryan Particle System/Scripts/UISparkleLoop.cs`

A fully UGUI-based sparkle particle system where particles fly in from the screen edges toward a target `Image` fill bar. Each particle that arrives fills the bar slightly, creating a tactile "collecting" feel. Supports a live control panel (color buttons, speed/size sliders).

**Key API**
```csharp
sparkleLoop.AddFillChunk();             // trigger one fill increment + burst
sparkleLoop.ResetBar();                 // reset fill to zero and clear all particles
sparkleLoop.SetParticleColor(Color c);
sparkleLoop.SetSpeed(float s);
sparkleLoop.SetSize(float s);
sparkleLoop.OnFillComplete             // Action fired when bar reaches 1.0
```

No external particle system packages — everything runs on pooled UGUI `Image` components.

---

### Animated Coin Counter — Vasudha Padala
`Assets/VasudhaCoinCounter/UIFX/Scripts/UIFX_CoinCounter.cs`

Animates a TextMesh Pro integer label from its current value to a new target value with an ease-out cubic curve and a scale punch. `UIFX_CoinCounter2` adds a Y-axis tilt on top of the punch.

```csharp
counter.Play(int newValue);   // interrupt any running animation and animate to newValue
```

**Prefabs:** `Assets/VasudhaCoinCounter/UIFX/Prefabs/`

---

### Dynamic Ordering System — Naveen Prakaasham Vairaprakasam
`Assets/UIFX/Runtime/Scripts/FX/UIDynamicOrderSystem.cs` · `Assets/UIFX/Runtime/Scripts/FX/UIDynamicOrderItem.cs`

A runtime system that reorders a vertical list of UI components with animated transitions. Select an item, click a position button (1–5), and the item exits with a flash, remaining items slide to their new slots, and the moved item re-enters at its target position with a scale-up emphasis.

**Transition states:**
- **Entering** — fade-in + scale-up with ease-out-back overshoot, followed by a highlight pulse
- **Exiting** — rapid orange flash (3 pulses), then fade-out + scale-down
- **Translating** — smooth anchored-position interpolation with ease-out-cubic easing

**Key API**
```csharp
system.MoveSelectedToPosition(int index);  // move the selected item to a 0-based position
system.ResetOrder();                       // animate all items back to their original order
item.Select();                             // highlight with golden pulse + scale punch
item.Deselect();                           // clear highlight
```

All animations use native Unity coroutines — no DOTween dependency.

**Demo:** run **UIFX > Dynamic Order > Open Demo Scene** — the scene is auto-created on first open.

---

### Sprite Glow Outline — Aryaman Kunwar
`Assets/SpriteGlow/Runtime/SpriteGlowEffect.cs`

Adds an HDR outline around a `SpriteRenderer`'s sprite borders via a `MaterialPropertyBlock`. Pair with URP's Bloom post-processing volume for a physical glow. Works in both edit mode and play mode.

| Property | Default | Notes |
|---|---|---|
| Glow Color | White | HDR — values > 1 bloom |
| Glow Brightness | 2 | Multiplied into glow color |
| Outline Width | 1 texel | |
| Alpha Threshold | 0.01 | Sprite border detection |
| Draw Outside | false | Restrict outline to sprite silhouette |

**Material:** `Assets/Materials/SpriteOutline.mat`

---

## Project Structure

```
Assets/
├── UIFX/
│   ├── Demo/
│   │   ├── Scenes/         # UIShimmerDemo.unity, Vedaant_Demo.unity, UIFX_Catalog.unity
│   │   ├── Scripts/        # UIShimmerDemoController.cs
│   │   └── Videos/         # Feature showcase captures and per-feature README files
│   ├── Editor/              # UIFXMenuItems.cs, UIShimmerSetup.cs, UIShimmerDemoBuilder.cs, UIDynamicOrderDemoBuilder.cs
│   └── Runtime/
│       ├── Materials/       # UIShimmer.mat (auto-created)
│       └── Scripts/
│           ├── Core/        # UIEffectBase.cs
│           └── FX/          # UIShimmerEffect.cs, UIDynamicOrderSystem.cs, UIDynamicOrderItem.cs
├── Scripts/                 # UIPathProgress.cs, MinMaxSliderController.cs
├── SriramShineShader/       # Shine shader + demo scene
├── Aryan Particle System/   # Sparkle particle system + demo scene
├── VasudhaCoinCounter/      # Coin counter scripts + prefabs
├── Naveen/                  # Dynamic Order reference images
├── SpriteGlow/              # Sprite glow runtime + shader
├── Prefabs/Vedaant/         # LineBase.prefab, NodeBase.prefab
└── Settings/                # PC_RPAsset.asset, Mobile_RPAsset.asset
```

## Setup

1. Open the project in Unity Hub — select **Unity 6000.3.8f1**.
2. Open any demo via **UIFX > [Feature] > Open Demo Scene** and press Play.
3. The UIShimmer material is created automatically on first editor load; no manual step is needed.

**Debugging:** attach VS Code or Visual Studio to the running Unity Editor process using `.vscode/launch.json` (port 4024).

**Tests:** `Window > General > Test Runner` — framework is included (`com.unity.test-framework 1.6.0`) but no tests are written yet.
