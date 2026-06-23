# Anxiety In Lines Project Index

Last indexed: 2026-06-23

## Overview

This workspace contains a Unity project nested under:

- `Anxiety In Lines/`

The project is a Unity 6 URP 2D prototype for `Anxiety In Lines`, a queue-anxiety game based on waiting in line under uncertain timing and social pressure.

The first code version exists in `Assets/Scripts/`. It implements a runtime-generated queue scene with NPCs, a player, a timer, negotiation/cut-in behavior, win/loss logic, and a HUD. `SampleScene` now includes a `GameController` object that references `GameManager`, while the GreenGuy scene is being used for character movement and animation testing.

The project now also has a local showcase website under `website/`. The website presents the project concept, development timeline, current implementation state, imported visual assets, and a placeholder area for the final Unity WebGL build.

## Unity Version

- Unity Editor: `6000.3.6f1`
- Project name: `Anxiety In Lines`
- Bundle version: `1.0`
- Active input handler: new Input System

Source:

- `Anxiety In Lines/ProjectSettings/ProjectVersion.txt`
- `Anxiety In Lines/ProjectSettings/ProjectSettings.asset`

## High-Level Folder Map

| Path | Purpose | Keep in source control? |
| --- | --- | --- |
| `Anxiety In Lines/Assets/` | Authored Unity assets, scenes, render settings, input actions | Yes |
| `Anxiety In Lines/Packages/` | Unity package manifest and lockfile | Yes |
| `Anxiety In Lines/ProjectSettings/` | Unity project configuration | Yes |
| `website/` | Local project showcase website and future Unity WebGL host | Yes |
| `Anxiety In Lines/UserSettings/` | Local editor/user preferences | Usually no |
| `Anxiety In Lines/Library/` | Unity generated cache/import database | No |
| `Anxiety In Lines/Temp/` | Unity temporary files | No |
| `Anxiety In Lines/Logs/` | Unity editor/import logs | No |
| `Anxiety In Lines/*.csproj` | Unity-generated IDE project files | Usually no |
| `Anxiety In Lines/Anxiety In Lines.sln` | Unity-generated IDE solution | Usually no |

## Authored Asset Index

Current non-meta authored files in `Assets/`:

- `Assets/Scenes/SampleScene.unity`
- `Assets/Scenes/GreenGuy.unity`
- `Assets/Settings/Scenes/URP2DSceneTemplate.unity`
- `Assets/InputSystem_Actions.inputactions`
- `Assets/Scripts/GameManager.cs`
- `Assets/Scripts/GameUI.cs`
- `Assets/Scripts/GreenGuy_AnimationManager.cs`
- `Assets/Scripts/GreenGuy_Controller.cs`
- `Assets/Scripts/NPCController.cs`
- `Assets/Scripts/PlayerController.cs`
- `Assets/Animations/GreenGuy.controller`
- `Assets/Animations/GreenGuy_Idle.anim`
- `Assets/Animations/GreenGuy_WalkDownward.anim`
- `Assets/Animations/GreenGuy_Walking.anim`
- `Assets/Sprits/303-3035187_adv-chara-2d-character-sprite-png.png`
- `Assets/Sprits/360_F_114471205_0O1mMyKE100dWY4kqoDKBNYJDto53kkt.jpg`
- `Assets/Sprits/pngtree-character-sprite-sheet-with-walk-cycle-and-run-cycle-sequence-png-image_15099942.png`
- `Assets/DefaultVolumeProfile.asset`
- `Assets/UniversalRenderPipelineGlobalSettings.asset`
- `Assets/Settings/Renderer2D.asset`
- `Assets/Settings/UniversalRP.asset`
- `Assets/Settings/Lit2DSceneTemplate.scenetemplate`

The project has continued beyond the initial V1 script-only prototype. New local Unity work includes the `GreenGuy` scene, sprite assets, and animation assets.

## Script Index

### `Assets/Scripts/GameManager.cs`

Central runtime controller for the playable prototype.

Responsibilities:

- Stores core game settings: `totalNPCs`, `gameTime`, `slotSpacing`, `minRWT`, `maxRWT`, `serviceTime`, `cutSuccessChance`, and `cutFailTimePenalty`.
- Tracks runtime state: `noP`, `remainingTime`, `gameState`, and `negotiationActive`.
- Creates the camera setup, floor, ticket counter, queue slot markers, NPCs, player, and UI at runtime.
- Calculates NoP by counting NPCs ahead of the player.
- Decreases GT through `remainingTime -= Time.deltaTime`.
- Handles win/loss state:
  - Win when `noP <= 0` and `remainingTime > 0`.
  - Lose when `remainingTime <= 0`.
- Handles negotiation/cut-in:
  - `Y` attempts to cut in.
  - Successful cut swaps the player with the NPC ahead.
  - Failed cut subtracts a time penalty.
- Restarts the scene through `SceneManager.LoadScene`.

### `Assets/Scripts/PlayerController.cs`

Player queue-position controller.

Responsibilities:

- Reads keyboard input through Unity Input System.
- Uses `UpArrow` to move forward when the next slot is free.
- Starts negotiation when the next slot is occupied.
- Uses `Y` / `N` while negotiation is active.
- Applies a short movement cooldown.
- Visually pulses the player during negotiation.

Current limitation:

- The default input action asset exists, but this script reads `Keyboard.current` directly rather than using the generated input action map.

### `Assets/Scripts/NPCController.cs`

NPC queue behavior controller.

Responsibilities:

- Tracks each NPC's current queue slot.
- Waits a random amount of time before moving when the slot ahead is free.
- Uses `Random.Range(gm.minRWT, gm.maxRWT)` as RWT.
- Moves toward the counter one slot at a time.
- Starts service when reaching slot `0`.
- Calls `GameManager.OnNPCServed` after service time, then disappears.
- Uses scale and alpha changes as simple visual feedback while waiting or being served.

### `Assets/Scripts/GameUI.cs`

Runtime HUD and end-screen controller.

Responsibilities:

- Creates a screen-space overlay canvas at runtime.
- Displays:
  - people ahead / NoP
  - remaining time / GT
  - controls hint
  - negotiation prompt
  - cut-in success/failure result
  - win/loss end screen
- Allows restart with `R` after the game ends.

Current risk:

- Restart input reads `Keyboard.current.rKey` without a null check. This is usually fine on desktop, but it can throw an error on a platform/session without an active keyboard device.

### `Assets/Scripts/GreenGuy_Controller.cs`

Character movement test controller.

Responsibilities:

- Gets a `Rigidbody2D` from the attached object.
- Reads horizontal input through `Input.GetAxis("Horizontal")`.
- Applies horizontal velocity to the character.

Current risks:

- The script logs velocity every frame, which can become noisy during play.
- Movement accumulates `rb.linearVelocity.x + input * speed`, so the character may keep accelerating rather than moving at a capped speed.

### `Assets/Scripts/GreenGuy_AnimationManager.cs`

Animation state test controller.

Responsibilities:

- Uses horizontal input to set the `WalkSideway` animator boolean.
- Contains commented-out vertical walking logic for possible future use.

Current risks:

- `animator` is public and not automatically assigned because `GetComponent<Animator>()` is commented out. The scene must assign this reference in the Inspector.
- Debug logging occurs during horizontal input.

## Scene Index

### `Assets/Scenes/SampleScene.unity`

Detected key scene objects:

- `Main Camera`
- `Global Light 2D`
- `GameController`

Current integration note:

- `SampleScene.unity` now references `GameManager` through a `GameController` object. This addresses the earlier scene-integration gap found in the first V1 check.
- `PlayerController`, `NPCController`, and `GameUI` are still created or used through runtime flow rather than pre-authored scene objects.

### `Assets/Scenes/GreenGuy.unity`

Character movement and animation test scene.

Detected key objects:

- background/sprite reference object
- `Player`
- `Player (1)`
- `GreenGuy`
- `Main Camera`
- `Global Light 2D`
- `CinemachineCamera_2D`

This scene appears to test a GreenGuy character sprite, animator controller, Rigidbody2D movement, and Cinemachine camera setup.

### `Assets/Settings/Scenes/URP2DSceneTemplate.unity`

Unity-provided URP 2D scene template.

## Input Index

### `Assets/InputSystem_Actions.inputactions`

Action map:

- `Player`

Detected actions:

- `Move`
- `Look`
- `Attack`
- `Interact`
- `Crouch`
- `Jump`
- `Previous`
- `Next`
- `Sprint`

This is still the default Unity Input System action set and does not yet appear tailored to this project. The current V1 gameplay scripts read keyboard keys directly:

- `UpArrow`: move forward / begin negotiation if blocked
- `Y`: attempt to cut in during negotiation
- `N`: decline cut-in during negotiation
- `R`: restart after win/loss

## V1 Gameplay Model

The first implemented version translates the concept diagram into a simple playable queue system:

- The player starts behind several NPCs.
- NPCs move forward only after a random waiting time.
- The player can move forward when the next slot is empty.
- If someone is directly ahead, the player can negotiate/cut in.
- Cutting in has a configurable success chance.
- Failure costs time, strengthening the anxiety/time-pressure loop.
- The player wins by reaching the front before time expires.
- The player loses if time reaches zero while people remain ahead.

Core variables from the concept are present:

- `NoP`: implemented as `noP`, calculated from NPCs ahead of the player.
- `GT`: implemented as `remainingTime`, reduced by `Time.deltaTime`.
- `RWT`: implemented as per-NPC random wait time using `Random.Range(minRWT, maxRWT)`.

## Website Index

The local showcase website is located at:

- `website/index.html`
- `website/design.html`
- `website/game-world.html`
- `website/development.html`
- `website/styles.css`
- `website/script.js`
- `website/README.md`
- `website/game/README.md`
- `.github/workflows/deploy-website.yml`

Website purpose:

- Use `Game` as the homepage and future WebGL host.
- Present the game concept and everyday-life source.
- Show the NoP / GT / RWT data model.
- Embed the game framework graph on the Design page.
- Present the game world, imported visual assets, and GreenGuy test.
- Display imported visual assets from the Unity project.
- Load the chronological development timeline from `agent-development-log.md`.
- Reserve a playable area for the final Unity WebGL export.
- Deploy the website to GitHub Pages through GitHub Actions.

Run locally from the repository root:

```sh
python3 -m http.server 8000
```

Then open:

```text
http://localhost:8000/website/
```

Future WebGL export location:

```text
website/game/index.html
```

Until a Unity WebGL build exists at that path, the website displays a "Web build not exported yet" state.

## Rendering Index

The project is configured for Universal Render Pipeline 2D:

- `Assets/Settings/UniversalRP.asset`
- `Assets/Settings/Renderer2D.asset`
- `Assets/UniversalRenderPipelineGlobalSettings.asset`
- `Assets/DefaultVolumeProfile.asset`

`ProjectSettings/GraphicsSettings.asset` maps URP global settings, but the custom render pipeline field itself is empty. Unity may still resolve URP through quality or global settings; verify inside the Editor before relying on this for builds.

## Package Index

Important package dependencies:

- `com.unity.render-pipelines.universal` `17.3.0`
- `com.unity.inputsystem` `1.18.0`
- `com.unity.2d.animation` `13.0.2`
- `com.unity.2d.aseprite` `3.0.1`
- `com.unity.2d.psdimporter` `12.0.1`
- `com.unity.2d.spriteshape` `13.0.0`
- `com.unity.2d.tilemap.extras` `6.0.1`
- `com.unity.timeline` `1.8.10`
- `com.unity.ugui` `2.0.0`
- `com.unity.visualscripting` `1.9.9`
- `com.unity.test-framework` `1.6.0`

Package files:

- `Anxiety In Lines/Packages/manifest.json`
- `Anxiety In Lines/Packages/packages-lock.json`

## Current Development State

The repository is initialized as Git from the workspace root and is connected to GitHub:

- `https://github.com/xingchen-ian/AnxietyInLines`

The project now contains a first code prototype, a GreenGuy movement/animation test, and a local showcase website:

- 6 custom C# scripts found in `Assets/Scripts/`
- Runtime-created visual objects are used instead of authored sprites or prefabs
- Imported sprites now exist under `Assets/Sprits/`
- GreenGuy animation assets now exist under `Assets/Animations/`
- No test folders found in `Assets/`
- `PROJECT_INDEX.md`, `agent-development-log.md`, and `website/` exist at the workspace root

## V1 Check Findings

Items that look implemented:

- Queue slots and positional movement
- NPC random waiting behavior
- NPC service/removal at counter
- NoP calculation
- Time countdown
- Win/loss state
- Negotiation/cut-in flow
- Cut-in success/failure feedback
- Runtime HUD
- Restart after end state

Items that need verification or follow-up:

- Confirm the project compiles in Unity Editor after opening the scene.
- Confirm the player can actually trigger the full loop from start to win/loss.
- Consider replacing direct keyboard reads with the existing Input System action asset if controller/remapping support matters.
- Add stronger emotional feedback for anxiety, such as crowd pressure, visible impatience, queue uncertainty, or sound/visual tension.
- Add authored visuals later if the prototype needs to communicate more than abstract colored blocks.
- Export a Unity WebGL build into `website/game/` when ready.

## Size Notes

Approximate folder sizes at index time:

- Full Unity project folder: `2.1G`
- `Assets/`: `4.0M`
- `Library/`: `2.0G`
- `Packages/`: `20K`
- `ProjectSettings/`: `140K`

Most disk usage is Unity-generated cache data in `Library/`.

## Recommended Next Cleanup

If this project will be versioned, add a Unity-focused `.gitignore` before the first commit. At minimum, ignore:

- `Library/`
- `Temp/`
- `Logs/`
- `UserSettings/`
- generated `*.csproj`
- generated `*.sln`

Then commit only:

- `Assets/`
- `Packages/`
- `ProjectSettings/`
- project documentation such as this index
