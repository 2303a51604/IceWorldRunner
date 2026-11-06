# ❄ Frost Runner: Glacial Leap
### An Adrenaline-Packed Ice World Platformer Runner for Unity

Created for Unity 6 (6000.x) and compatible with modern Unity versions.

---

## 🎮 Game Overview

**Frost Runner: Glacial Leap** is an endless 2D/2.5D platformer runner where you control an Arctic explorer dashing through treacherous glaciers, collapsing ice caverns, and howling blizzards. 

The game is built with **zero external asset dependencies**:
- **Procedural Graphics Engine**: Crisp pixel/stylized sprites (player, ice blocks, cracked ice, icicles, crystals, mountain parallax) generated automatically in memory.
- **Procedural Synthesizer Audio**: Crystal chords, ice crack snaps, slide skate friction, jump sweeps, and howling blizzard wind generated at runtime via mathematical wave synthesis.
- **Endless Procedural Level Generation**: Seamless chunk generator pooling and recycling balanced challenge segments ahead of the camera.

---

## 🕹️ Controls

| Action | Primary Key | Alternate Keys |
| :--- | :--- | :--- |
| **Jump / Double Jump** | `Space` | `W`, `Up Arrow` |
| **Slide / Crouch** | `S` | `Down Arrow`, `Left Ctrl` |
| **Steer / Adjust Pace** | `A` / `D` | `Left` / `Right Arrow` |
| **Quick Restart** | `R` | — |
| **Pause / Resume** | `Escape` | `P` |

---

## 🧊 Core Gameplay & Mechanics

### 1. Slippery Ice Physics
* **Zero-Grip Sliding**: Running on ice drops ground friction to `0.02`. Momentum carries you forward; timing your jumps and slides around corners is crucial.
* **Low-Profile Slide**: Sliding shrinks your collider by 50%, letting you slip beneath low hanging icicle arches while giving an icy speed boost.

### 2. Environmental Hazards & Traps
* **Cracking Fragile Ice**: Step on thin ice blocks and they shiver with an audible crack before breaking 0.75 seconds later!
* **Falling Icicles**: Overhead stalactites detect your approach, vibrate with warning particles, and plummet down.
* **Crevasse Spikes**: Deep glacial chasms lined with sharp ice spikes.
* **Bouncy Snow Geysers**: Trampoline-like snowdrifts that launch you high into secret crystal tiers.
* **Blizzard Headwinds**: Periodic storms that kick up blinding snow and push back against your runner.

### 3. Power-Ups & Collectibles
* 💎 **Frost Shards / Crystals**: Boost your score and crystal counter with sparkling chime chords.
* 🔥 **Warmth Shield**: Surrounds you in an ember aura; absorbs 1 lethal hazard hit and melts falling icicles on contact.
* 🥾 **Spiked Crampons**: Eliminates ice slippage for 8 seconds, giving 100% responsive traction.
* 🧲 **Crystal Magnet**: Pulls all nearby crystals straight into your path.

---

## 🚀 How to Open and Play in Unity

1. Open **Unity Hub**.
2. Click **Add** -> **Add project from disk**.
3. Select this folder:
   ```
   C:\Users\golis\.gemini\antigravity\scratch\IceWorldRunner
   ```
4. Choose **Unity 6 (6000.6.7f1)** (or your installed Unity editor).
5. Open the project.
6. Open the scene located at:
   ```
   Assets/Scenes/IceWorldRunnerScene.unity
   ```
7. Press the **Play** button (`Ctrl + P`) in the Unity Editor to play immediately!

*(Optional)* You can also use the top menu bar in Unity:
- **`Tools` -> `Ice World Runner` -> `Build Complete Playable Scene`** to reconstruct or refresh all scene objects and prefabs.
- **`Tools` -> `Ice World Runner` -> `Reset High Scores`** to clear saved records.

---

## 📁 Architecture & File Structure

```
IceWorldRunner/
├── Assets/
│   ├── Scenes/
│   │   └── IceWorldRunnerScene.unity    # Ready-to-play preconfigured scene
│   └── Scripts/
│       ├── Bootstrap/
│       │   └── GameBootstrap.cs         # Self-initializing runtime bootstrapper
│       ├── Collectibles/
│       │   ├── CrystalCollectible.cs    # Crystal shard bobbing & magnetic attraction
│       │   └── PowerUpItem.cs           # Warmth Shield, Crampons, Magnet power-ups
│       ├── Core/
│       │   ├── CameraController.cs      # Lookahead camera + screen shake
│       │   ├── GameManager.cs           # State machine, speed curve, high score
│       │   └── SoundManager.cs          # Real-time procedural audio wave synthesizer
│       ├── Editor/
│       │   └── IceWorldBuilder.cs       # Unity Editor menu setup utilities
│       ├── Environment/
│       │   ├── BlizzardController.cs    # Snowstorm particle system & wind gusts
│       │   ├── FallingIcicle.cs         # Proximity-triggered drop & shatter
│       │   ├── FragileIcePlatform.cs    # Collapsing thin ice platforms
│       │   ├── IcePlatform.cs           # Physics materials & bouncy snowdrifts
│       │   └── ParallaxBackground.cs    # Infinite multi-layer scrolling mountains
│       ├── Player/
│       │   ├── PlayerController.cs      # Ice friction, double jump, coyote time, slide
│       │   └── PlayerEffects.cs         # Snow spray, dust puffs, death shatter
│       ├── Spawners/
│       │   └── ChunkManager.cs          # Procedural infinite runner level generator
│       └── UI/
│           ├── ProceduralTextureGenerator.cs # Code-generated pixel & vector sprites
│           └── UIManager.cs             # Frosted HUD, stats, powerup timers, game over
├── Packages/
│   └── manifest.json                    # Package dependencies for 2D & physics
└── ProjectSettings/
    ├── EditorBuildSettings.asset        # Registers IceWorldRunnerScene in build
    ├── InputManager.asset               # Standard Jump, Horizontal, Vertical axes
    ├── ProjectSettings.asset            # Game configuration & window sizing
    ├── ProjectVersion.txt               # Editor version targeting
    └── TagManager.asset                 # Ground, Ice, Obstacle, Collectible tags
```
