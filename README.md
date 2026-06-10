![Unity](https://img.shields.io/badge/Unity-6000.0.62f1_LTS-black?style=for-the-badge&logo=unity)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![URP](https://img.shields.io/badge/URP_2D-Render_Pipeline-blue?style=for-the-badge&logo=unity)
![Status](https://img.shields.io/badge/Status-Completed_TFG-brightgreen?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)

# ⚔️ Dungeon Legacy

> *Death is not the end — it's an inheritance.*

A 2D action-exploration game built in Unity 6000 that hybridizes **Metroidvania** and **Roguelike** mechanics around a core differentiator: **generational succession**. When your character dies, their heir carries forward a fraction of their stats — turning every death into narrative progression rather than pure punishment.

---

<!-- SCREENSHOT PLACEHOLDER -->
> 📸 **[ADD HERE: Gameplay GIF or screenshot — ideally the dungeon scene with 2D lighting active, showing the player character and at least one enemy. A 600×340px GIF of a combat sequence works best on GitHub.]**

---

## ✨ Features

- 🧬 **Generational Inheritance System** — On death, each stat has a 60% chance of transferring to the heir, at a random 5%–20% of the ancestor's value. At least one stat is always guaranteed to carry over.

- ⚔️ **Knight Class** — Melee-focused combat with circular hitbox detection. Fast energy regeneration (20 u/s). **6 unique skins** that randomize between generations, visually reinforcing the passage of time.

- 🧙 **Mage Class** — Ranged projectiles and AoE spells. Slow mana regeneration (2 u/s) forces deliberate resource management and positional play.

- 🏚️ **Procedural Room Generation** — Dungeons are assembled from prefab rooms with no-repeat logic, enemy count and type scaling per generation number.

- 🃏 **Blessing System** — On entering the base, three blessing cards are offered (Bronze / Silver / Gold tiers), each boosting a random stat for the current run only.

- 📜 **Epitaph Screen** — A dedicated UI screen appears 1.5s after the death animation, displaying the ancestor's generation number, floor reached, and final stats — alongside a summary of what the heir will inherit.

- 🏗️ **SOLID Architecture** — Full separation of concerns across `Core`, `Player`, `Enemies`, `GenerationSystem`, `Managers`, `Persistence`, and `Progression` modules. Zero circular dependencies.

---

## 🛠️ Tech Stack

| Technology | Version / Detail |
|---|---|
| Engine | Unity 6000.0.62f1 (LTS) |
| Language | C# |
| Render Pipeline | URP 2D (Universal Render Pipeline) |
| Physics | Unity 2D Physics (Rigidbody2D, CompositeCollider2D) |
| Version Control | Git / GitHub |
| IDE | Visual Studio Community |
| Art Style | Pixel Art — 32 PPU, Point Filter, no compression |

---

## 🏛️ Architecture

The entire codebase lives under `Assets/_Proyect/`, structured so that every directory owns exactly **one domain**. Adding a new system never requires touching existing ones.

```
Assets/_Proyect/
├── Core/
│   ├── EventBus/           # Generic Observer — decoupled system communication
│   ├── Interfaces/         # IDamageable, IHealable, IInteractable
│   ├── ServiceLocator/     # Centralized service registry (no rigid coupling)
│   └── StateMachine/       # IPlayerState base interface
│
├── Player/
│   ├── Combat/             # Attack detection, hitbox logic
│   ├── Controller/
│   │   └── States/         # IdleState, RunState, JumpState, FallState,
│   │                       # AttackState, HurtState, DashState, DeadState
│   └── Stats/              # HealthComponent, EnergySystem, ManaSystem
│
├── Enemies/
│   ├── Base/               # EnemyBase (abstract — Template Method pattern)
│   └── Types/              # EnemyMelee (Orc), GoblinArcher + EnemyProjectile
│
├── GenerationSystem/
│   ├── Dungeon/            # RoomManager, RoomLoader, RoomSpawner
│   └── Inheritance/        # AncestorRecord, InheritanceResolver
│
├── Managers/               # GenerationManager, GameBootstrap, SceneTransitionManager
├── Persistence/            # LegacyData (permanent cross-run history)
├── Progression/            # RunData (volatile per-run state)
│
├── UI/                     # EpitaphScreen, HUDController, BlessingSelectionUI,
│                           # BlessingCard, InteractionTextUI
├── ScriptableObjects/
│   ├── Generation/         # GenerationConfig
│   └── Stats/              # StatBlock
│
└── Scenes/                 # MainMenuScene [0], BaseScene [1], DungeonScene [2]
```

**Scene build order is meaningful:** index 0 = main menu, 1 = inter-generation base camp, 2 = dungeon room (loaded/unloaded per room cycle).

---

## 🧩 Design Patterns

### Singleton + DontDestroyOnLoad
Reserved **exclusively** for managers that need cross-scene persistence. Attaching a persistent manager as a child of a scene Canvas silently breaks persistence — all persistent managers live on their own root GameObjects.

| Manager | Registered in ServiceLocator |
|---|---|
| `GenerationManager` | ✅ |
| `GameBootstrap` | ❌ |
| `RoomManager` | ✅ |
| `SceneTransitionManager` | ✅ |
| `HUDCanvas` (HUDPersistence) | ❌ |
| `PauseCanvas` (PauseManager) | ❌ |
| `BlessingCanvas` (BlessingSelectionUI) | ❌ |
| `EpitaphCanvas` (EpitaphScreen) | ❌ |

### EventBus (Observer)
`HealthComponent` publishes `OnDeath`. `GenerationManager` and `HUDController` react independently — neither knows the other exists. Every subscriber follows the mandatory unsubscribe-before-subscribe protocol to prevent memory leaks:

```csharp
// Mandatory protocol in every script that handles events
_playerHealth.OnDeath -= HandlePlayerDeath;
_playerHealth.OnDeath += HandlePlayerDeath;
```

### ServiceLocator
Provides controlled access to global services without creating a dependency graph. Allows swapping service implementations without touching consumer code — critical for testability and long-term scalability.

### FSM (State Pattern)
The player controller delegates all per-frame logic to the active state. Each state is an isolated C# class implementing `IPlayerState` (`Enter()`, `Update()`, `Exit()`). Adding a new state requires zero changes to existing ones.

```
Idle ──► Run ──► Jump ──► Fall
 │        │               │
 └──► Attack ◄────────────┘
 │
 └──► Dash
 │
 └──► Hurt ──► (Idle / Fall)
 │
 └──► Dead ──► [GenerationManager takes over]
```

A `guard clause` at the top of `HandleTransitions()` ensures no state transition can fire after the player enters `Dead` — preventing the generation system from initializing the heir while the FSM is still processing a Hurt frame.

### Template Method (EnemyBase)
`EnemyBase` defines the patrol → detect → chase → attack loop as a sealed sequence. Subclasses (`EnemyMelee`, `GoblinArcher`) only override `Attack()`, which in the archer's case fires a parabolic projectile calculated with kinematic equations in `EnemyProjectile.Initialize()`.

---

## 🧬 Generational System — Deep Dive

The core mechanic is built on a strict architectural separation between two data classes:

| Class | Scope | Reset on death |
|---|---|---|
| `RunData` | Current run (volatile) | ✅ Partially — stats are modified by `InheritanceResolver` *before* reset |
| `LegacyData` | Full family history (persistent) | ❌ Never — grows across the entire session |

### Death → Inheritance Flow

```
HealthComponent.TakeDamage()
  │
  ├─► PlayerController.OnDead()       // FSM enters Dead state FIRST
  │                                   // stops all input processing
  │
  └─► OnDeath?.Invoke()               // GenerationManager reacts
        │
        ├─► AncestorRecord(snapshot)  // captures all stats at death
        │
        ├─► ShowEpitaphAfterDelay()   // waits 1.5s for death animation
        │     └─► EpitaphScreen.Show(record, summary, callback)
        │
        └─► StartNextGeneration()
              ├─► ResetRun(generation + 1)
              ├─► InheritanceResolver.ApplyInheritance(Legacy, CurrentRun)
              ├─► ApplyRunDataToPlayer()
              └─► CargarEscena("BaseScene")
```

> ⚠️ **Order matters:** `OnDead()` must fire before `OnDeath?.Invoke()`. An inverted order caused the GenerationManager to initialize the heir while the FSM was still in `Hurt` state — resulting in a paralyzed player at generation start. This was a real bug, caught and documented.

### Inheritance Limits

`InheritanceResolver` is a **pure static C# class** (no `MonoBehaviour`). This is a deliberate design decision: inheritance calculation is pure math with no need for Unity's lifecycle. It can be unit-tested in isolation and carries zero memory overhead.

| Stat | Inheritance Probability | Range | Hard Cap |
|---|---|---|---|
| Max HP | 60% | 5% – 20% of ancestor's value | 250 HP |
| Move Speed | 60% | 5% – 20% | 12 u/s |
| Jump Force | 60% | 5% – 20% | 20 |
| Attack Damage | 60% | 5% – 20% | 80 pts |
| Max Energy | 60% | 5% – 20% | 200 units |
| Max Mana | 60% | 5% – 20% | 200 units |

Hard caps are enforced via `Mathf.Min()`. Without them, multi-generation stacking would make the player invincible — breaking the roguelike tension. Caps were calibrated through playtesting sessions.

**Verification results (first 3 generations):**

| Generation | Stats at run start | Inheritance applied |
|---|---|---|
| Gen 1 | HP: 100 · Dmg: 20 · Spd: 5 · Energy: 100 · Mana: 100 | First generation — no inheritance |
| Gen 2 | HP: 108.3 · Spd: 5.43 · Energy: 107 · Mana: 112 | HP +8.3 (8%) · Spd +0.43 (9%) · Energy +7 (7%) · Mana +12 (12%) |
| Gen 3 | HP: 121 · Dmg: 22.4 · Spd: 5.61 · Jump: 12.9 · Energy: 114 | HP +12.7 (12%) · Dmg +2.4 (11%) · Spd +0.18 · Jump +0.9 · Energy +7 |

---

## 🚀 Getting Started

### Prerequisites

- Unity **6000.0.62f1 (LTS)** — download via [Unity Hub](https://unity.com/download)
- Git

### Clone & Open

```bash
git clone https://github.com/YOUR_USERNAME/dungeon-legacy.git
```

1. Open **Unity Hub** → **Add project from disk** → select the cloned folder.
2. Unity will resolve packages automatically on first open (this may take a few minutes).
3. Open `Assets/_Proyect/Scenes/MainMenuScene` and press **Play**.

> 💡 The project uses **URP 2D**. If sprites appear pink/magenta, go to `Edit → Render Pipeline → Universal Render Pipeline → Upgrade Project Materials`.

### Build Settings Scene Order

Verify the following order in `File → Build Settings`:

| Index | Scene |
|---|---|
| 0 | `MainMenuScene` |
| 1 | `BaseScene` |
| 2 | `DungeonScene` |

---

## 🎮 Controls

| Action | Input |
|---|---|
| Move | A / D or ←/→ |
| Jump | Space |
| Attack | Left Mouse Button |
| Dash | Left Shift |
| Pause | Escape |
| Interact | E |

---

## 📚 What I Learned

These are the architectural lessons that came from real bugs caught during development — not theory:

- **Double-subscription bug pattern** — Always unsubscribe before subscribing to Unity events, especially in `Initialize()` methods called across scene loads. A single missed unsubscription caused cascading generation logic to fire twice, corrupting the inheritance calculation silently.

- **DontDestroyOnLoad silently breaks with child GameObjects** — Attaching a persistent manager as a child of a scene Canvas prevents `DontDestroyOnLoad` from working as expected. Persistent managers must live on independent root GameObjects. This took a full debugging session to diagnose.

- **FSM state order in death sequences is critical** — The FSM must enter `Dead` state *before* external systems (like `GenerationManager`) react to `OnDeath`. Inverting this order caused the player controller to process input from `Hurt` while the generation system was already initializing the heir. The fix was a single line reorder — the investigation was not.

- **Pure C# classes for logic, MonoBehaviour only for Unity lifecycle** — `InheritanceResolver` and `AncestorRecord` have no `MonoBehaviour` inheritance. Pure C# classes are lighter, testable in isolation, and cannot generate memory leaks through uncleaned event subscriptions. This distinction became a project-wide architectural rule.

---

## 📁 Project Structure at a Glance

```
dungeon-legacy/
├── Assets/
│   └── _Proyect/          # All custom code and assets
├── Packages/              # Unity package manifest
├── ProjectSettings/       # Unity project settings
└── README.md
```

> `Library/`, `Temp/`, `Obj/` and `Build/` are excluded via `.gitignore` — Unity regenerates them automatically.

---

## 🗺️ Roadmap / Future Work

- [ ] Boss fight implementation (final dungeon floor)
- [ ] Persistent save system (serialize `LegacyData` to disk)
- [ ] Additional enemy types and attack patterns  
- [ ] Additional playable classes
- [ ] itch.io / Steam release

---

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

Asset packs used:
- *Tiny RPG Character Pack* — [itch.io](https://itch.io/game-assets)
- *Environment & Decoration Sprite Pack* by [Rhino] — [itch.io](https://itch.io/game-assets)
- *Alagard* font — SDF generated via Unity Font Asset Creator

---

## 👤 Author

**Gabriel Ignacio Castaño Irala**  
2º DAM — Desarrollo de Aplicaciones Multiplataforma · 2024/2025

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://linkedin.com/in/YOUR_LINKEDIN)
[![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/YOUR_USERNAME)

---

<p align="center">
  <em>Built as a Final Degree Project (TFG) — but designed as a real game.</em>
</p>![Unity](https://img.shields.io/badge/Unity-6000.0.62f1_LTS-black?style=for-the-badge&logo=unity)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![URP](https://img.shields.io/badge/URP_2D-Render_Pipeline-blue?style=for-the-badge&logo=unity)
![Status](https://img.shields.io/badge/Status-Completed_TFG-brightgreen?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)

# ⚔️ Dungeon Legacy

> *Death is not the end — it's an inheritance.*

A 2D action-exploration game built in Unity 6000 that hybridizes **Metroidvania** and **Roguelike** mechanics around a core differentiator: **generational succession**. When your character dies, their heir carries forward a fraction of their stats — turning every death into narrative progression rather than pure punishment.

---

<!-- SCREENSHOT PLACEHOLDER -->
> 📸 **[ADD HERE: Gameplay GIF or screenshot — ideally the dungeon scene with 2D lighting active, showing the player character and at least one enemy. A 600×340px GIF of a combat sequence works best on GitHub.]**

---

## ✨ Features

- 🧬 **Generational Inheritance System** — On death, each stat has a 60% chance of transferring to the heir, at a random 5%–20% of the ancestor's value. At least one stat is always guaranteed to carry over.

- ⚔️ **Knight Class** — Melee-focused combat with circular hitbox detection. Fast energy regeneration (20 u/s). **6 unique skins** that randomize between generations, visually reinforcing the passage of time.

- 🧙 **Mage Class** — Ranged projectiles and AoE spells. Slow mana regeneration (2 u/s) forces deliberate resource management and positional play.

- 🏚️ **Procedural Room Generation** — Dungeons are assembled from prefab rooms with no-repeat logic, enemy count and type scaling per generation number.

- 🃏 **Blessing System** — On entering the base, three blessing cards are offered (Bronze / Silver / Gold tiers), each boosting a random stat for the current run only.

- 📜 **Epitaph Screen** — A dedicated UI screen appears 1.5s after the death animation, displaying the ancestor's generation number, floor reached, and final stats — alongside a summary of what the heir will inherit.

- 🏗️ **SOLID Architecture** — Full separation of concerns across `Core`, `Player`, `Enemies`, `GenerationSystem`, `Managers`, `Persistence`, and `Progression` modules. Zero circular dependencies.

---

## 🛠️ Tech Stack

| Technology | Version / Detail |
|---|---|
| Engine | Unity 6000.0.62f1 (LTS) |
| Language | C# |
| Render Pipeline | URP 2D (Universal Render Pipeline) |
| Physics | Unity 2D Physics (Rigidbody2D, CompositeCollider2D) |
| Version Control | Git / GitHub |
| IDE | Visual Studio Community |
| Art Style | Pixel Art — 32 PPU, Point Filter, no compression |

---

## 🏛️ Architecture

The entire codebase lives under `Assets/_Proyect/`, structured so that every directory owns exactly **one domain**. Adding a new system never requires touching existing ones.

```
Assets/_Proyect/
├── Core/
│   ├── EventBus/           # Generic Observer — decoupled system communication
│   ├── Interfaces/         # IDamageable, IHealable, IInteractable
│   ├── ServiceLocator/     # Centralized service registry (no rigid coupling)
│   └── StateMachine/       # IPlayerState base interface
│
├── Player/
│   ├── Combat/             # Attack detection, hitbox logic
│   ├── Controller/
│   │   └── States/         # IdleState, RunState, JumpState, FallState,
│   │                       # AttackState, HurtState, DashState, DeadState
│   └── Stats/              # HealthComponent, EnergySystem, ManaSystem
│
├── Enemies/
│   ├── Base/               # EnemyBase (abstract — Template Method pattern)
│   └── Types/              # EnemyMelee (Orc), GoblinArcher + EnemyProjectile
│
├── GenerationSystem/
│   ├── Dungeon/            # RoomManager, RoomLoader, RoomSpawner
│   └── Inheritance/        # AncestorRecord, InheritanceResolver
│
├── Managers/               # GenerationManager, GameBootstrap, SceneTransitionManager
├── Persistence/            # LegacyData (permanent cross-run history)
├── Progression/            # RunData (volatile per-run state)
│
├── UI/                     # EpitaphScreen, HUDController, BlessingSelectionUI,
│                           # BlessingCard, InteractionTextUI
├── ScriptableObjects/
│   ├── Generation/         # GenerationConfig
│   └── Stats/              # StatBlock
│
└── Scenes/                 # MainMenuScene [0], BaseScene [1], DungeonScene [2]
```

**Scene build order is meaningful:** index 0 = main menu, 1 = inter-generation base camp, 2 = dungeon room (loaded/unloaded per room cycle).

---

## 🧩 Design Patterns

### Singleton + DontDestroyOnLoad
Reserved **exclusively** for managers that need cross-scene persistence. Attaching a persistent manager as a child of a scene Canvas silently breaks persistence — all persistent managers live on their own root GameObjects.

| Manager | Registered in ServiceLocator |
|---|---|
| `GenerationManager` | ✅ |
| `GameBootstrap` | ❌ |
| `RoomManager` | ✅ |
| `SceneTransitionManager` | ✅ |
| `HUDCanvas` (HUDPersistence) | ❌ |
| `PauseCanvas` (PauseManager) | ❌ |
| `BlessingCanvas` (BlessingSelectionUI) | ❌ |
| `EpitaphCanvas` (EpitaphScreen) | ❌ |

### EventBus (Observer)
`HealthComponent` publishes `OnDeath`. `GenerationManager` and `HUDController` react independently — neither knows the other exists. Every subscriber follows the mandatory unsubscribe-before-subscribe protocol to prevent memory leaks:

```csharp
// Mandatory protocol in every script that handles events
_playerHealth.OnDeath -= HandlePlayerDeath;
_playerHealth.OnDeath += HandlePlayerDeath;
```

### ServiceLocator
Provides controlled access to global services without creating a dependency graph. Allows swapping service implementations without touching consumer code — critical for testability and long-term scalability.

### FSM (State Pattern)
The player controller delegates all per-frame logic to the active state. Each state is an isolated C# class implementing `IPlayerState` (`Enter()`, `Update()`, `Exit()`). Adding a new state requires zero changes to existing ones.

```
Idle ──► Run ──► Jump ──► Fall
 │        │               │
 └──► Attack ◄────────────┘
 │
 └──► Dash
 │
 └──► Hurt ──► (Idle / Fall)
 │
 └──► Dead ──► [GenerationManager takes over]
```

A `guard clause` at the top of `HandleTransitions()` ensures no state transition can fire after the player enters `Dead` — preventing the generation system from initializing the heir while the FSM is still processing a Hurt frame.

### Template Method (EnemyBase)
`EnemyBase` defines the patrol → detect → chase → attack loop as a sealed sequence. Subclasses (`EnemyMelee`, `GoblinArcher`) only override `Attack()`, which in the archer's case fires a parabolic projectile calculated with kinematic equations in `EnemyProjectile.Initialize()`.

---

## 🧬 Generational System — Deep Dive

The core mechanic is built on a strict architectural separation between two data classes:

| Class | Scope | Reset on death |
|---|---|---|
| `RunData` | Current run (volatile) | ✅ Partially — stats are modified by `InheritanceResolver` *before* reset |
| `LegacyData` | Full family history (persistent) | ❌ Never — grows across the entire session |

### Death → Inheritance Flow

```
HealthComponent.TakeDamage()
  │
  ├─► PlayerController.OnDead()       // FSM enters Dead state FIRST
  │                                   // stops all input processing
  │
  └─► OnDeath?.Invoke()               // GenerationManager reacts
        │
        ├─► AncestorRecord(snapshot)  // captures all stats at death
        │
        ├─► ShowEpitaphAfterDelay()   // waits 1.5s for death animation
        │     └─► EpitaphScreen.Show(record, summary, callback)
        │
        └─► StartNextGeneration()
              ├─► ResetRun(generation + 1)
              ├─► InheritanceResolver.ApplyInheritance(Legacy, CurrentRun)
              ├─► ApplyRunDataToPlayer()
              └─► CargarEscena("BaseScene")
```

> ⚠️ **Order matters:** `OnDead()` must fire before `OnDeath?.Invoke()`. An inverted order caused the GenerationManager to initialize the heir while the FSM was still in `Hurt` state — resulting in a paralyzed player at generation start. This was a real bug, caught and documented.

### Inheritance Limits

`InheritanceResolver` is a **pure static C# class** (no `MonoBehaviour`). This is a deliberate design decision: inheritance calculation is pure math with no need for Unity's lifecycle. It can be unit-tested in isolation and carries zero memory overhead.

| Stat | Inheritance Probability | Range | Hard Cap |
|---|---|---|---|
| Max HP | 60% | 5% – 20% of ancestor's value | 250 HP |
| Move Speed | 60% | 5% – 20% | 12 u/s |
| Jump Force | 60% | 5% – 20% | 20 |
| Attack Damage | 60% | 5% – 20% | 80 pts |
| Max Energy | 60% | 5% – 20% | 200 units |
| Max Mana | 60% | 5% – 20% | 200 units |

Hard caps are enforced via `Mathf.Min()`. Without them, multi-generation stacking would make the player invincible — breaking the roguelike tension. Caps were calibrated through playtesting sessions.

**Verification results (first 3 generations):**

| Generation | Stats at run start | Inheritance applied |
|---|---|---|
| Gen 1 | HP: 100 · Dmg: 20 · Spd: 5 · Energy: 100 · Mana: 100 | First generation — no inheritance |
| Gen 2 | HP: 108.3 · Spd: 5.43 · Energy: 107 · Mana: 112 | HP +8.3 (8%) · Spd +0.43 (9%) · Energy +7 (7%) · Mana +12 (12%) |
| Gen 3 | HP: 121 · Dmg: 22.4 · Spd: 5.61 · Jump: 12.9 · Energy: 114 | HP +12.7 (12%) · Dmg +2.4 (11%) · Spd +0.18 · Jump +0.9 · Energy +7 |

---

## 🚀 Getting Started

### Prerequisites

- Unity **6000.0.62f1 (LTS)** — download via [Unity Hub](https://unity.com/download)
- Git

### Clone & Open

```bash
git clone https://github.com/YOUR_USERNAME/dungeon-legacy.git
```

1. Open **Unity Hub** → **Add project from disk** → select the cloned folder.
2. Unity will resolve packages automatically on first open (this may take a few minutes).
3. Open `Assets/_Proyect/Scenes/MainMenuScene` and press **Play**.

> 💡 The project uses **URP 2D**. If sprites appear pink/magenta, go to `Edit → Render Pipeline → Universal Render Pipeline → Upgrade Project Materials`.

### Build Settings Scene Order

Verify the following order in `File → Build Settings`:

| Index | Scene |
|---|---|
| 0 | `MainMenuScene` |
| 1 | `BaseScene` |
| 2 | `DungeonScene` |

---

## 🎮 Controls

| Action | Input |
|---|---|
| Move | A / D or ←/→ |
| Jump | Space |
| Attack | Left Mouse Button |
| Dash | Left Shift |
| Pause | Escape |
| Interact | E |

---

## 📚 What I Learned

These are the architectural lessons that came from real bugs caught during development — not theory:

- **Double-subscription bug pattern** — Always unsubscribe before subscribing to Unity events, especially in `Initialize()` methods called across scene loads. A single missed unsubscription caused cascading generation logic to fire twice, corrupting the inheritance calculation silently.

- **DontDestroyOnLoad silently breaks with child GameObjects** — Attaching a persistent manager as a child of a scene Canvas prevents `DontDestroyOnLoad` from working as expected. Persistent managers must live on independent root GameObjects. This took a full debugging session to diagnose.

- **FSM state order in death sequences is critical** — The FSM must enter `Dead` state *before* external systems (like `GenerationManager`) react to `OnDeath`. Inverting this order caused the player controller to process input from `Hurt` while the generation system was already initializing the heir. The fix was a single line reorder — the investigation was not.

- **Pure C# classes for logic, MonoBehaviour only for Unity lifecycle** — `InheritanceResolver` and `AncestorRecord` have no `MonoBehaviour` inheritance. Pure C# classes are lighter, testable in isolation, and cannot generate memory leaks through uncleaned event subscriptions. This distinction became a project-wide architectural rule.

---

## 📁 Project Structure at a Glance

```
dungeon-legacy/
├── Assets/
│   └── _Proyect/          # All custom code and assets
├── Packages/              # Unity package manifest
├── ProjectSettings/       # Unity project settings
└── README.md
```

> `Library/`, `Temp/`, `Obj/` and `Build/` are excluded via `.gitignore` — Unity regenerates them automatically.

---

## 🗺️ Roadmap / Future Work

- [ ] Boss fight implementation (final dungeon floor)
- [ ] Persistent save system (serialize `LegacyData` to disk)
- [ ] Additional enemy types and attack patterns  
- [ ] Additional playable classes
- [ ] itch.io / Steam release

---

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

Asset packs used:
- *Tiny RPG Character Pack* — [itch.io](https://itch.io/game-assets)
- *Environment & Decoration Sprite Pack* by [Rhino] — [itch.io](https://itch.io/game-assets)
- *Alagard* font — SDF generated via Unity Font Asset Creator

---

## 👤 Author

**Gabriel Ignacio Castaño Irala**  
2º DAM — Desarrollo de Aplicaciones Multiplataforma · 2024/2025

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/gabriel-ignacio-casta%C3%B1o-irala-inform%C3%A1tica/)
[![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/ItsGabrii)

---

<p align="center">
  <em>Built as a Final Degree Project (TFG) — but designed as a real game.</em>
</p>
