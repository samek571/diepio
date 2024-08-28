
# Game Documentation: Single-Player Diep.io Clone

## 1. Introduction

### Overview
This game is a single-player version of [diep.io](https://diep.io) made in Godot engine in C# language. The player controls a tank that shoots bullets to destroy enemies and derbis, earning experience points (XP) yielding to level ups, thus making possible statistics improvement. The game emphasizes strategic upgrades and survival against enemies.

### Key Features
- **Dynamic Shooting Mechanics**: Tank can shoot bullets that deal damage, however they degrade in speed over time. It is possible to toggle autoshoot by pressing "E" button on the keyboard.
- **Leveling and Upgrades**: Gain XP by defeating enemies and random lying junk, level up, and improve various statistics by pressing 1-8. Unlike in diep.io It is possible to reset the build-tree anytime by pressing "0". (numpad keyboard works aswell).
-  speed, health, and more.
- **Enemy**: Enemies chase and attack the player while in range, trasnforming this into a strategical combat game.

#### Dislcaimer
(All the constants are not tweaked to perfection, i.e spawnrate might be too high or relatively to the later progress in the game the starting statistics might be sometimes underperformingly initialized. Purpose was to make the game, not to make an enoyable experience, later if I find some time and inclination there might be updates)


## 2. File Descriptions

### 2.1 Player.cs
The `Player` class manages the player character's actions, including movement, shooting, collision handling, and XP management.

- **Movement**: Handled using WASD or arrow keys. The player's movement speed is defined by `MovementSpeed`.
- **Shooting**: Bullets are instantiated and shot towards the mouse pointer's position. Shooting can be toggled between manual and automatic modes using the "E" key.
- **Collision Handling**: Handles collisions with enemies and targets, applying damage and knockback effects.
- **XP and Leveling**: Manages gaining XP and leveling up through interaction with the `LevelManager`.
- **Health Management**: Connects to `HealthManager` to handle damage and healing over time.
  
### 2.2 Bullet.cs
The `Bullet` class defines the behavior of bullets shot by the player.

- **Properties**: Direction, speed, damage, and durability of bullets.
- **Lifecycle**: Bullets have a lifespan managed by a timer and decrease in speed exponentially over time.
- **Collision Detection**: Bullets can collide with players, enemies, and other targets, dealing damage and then being destroyed.

### 2.3 Enemy.cs
The `Enemy` class inherits from `Target` and manages the behavior of enemies in the game.

- **Chasing Mechanism**: Enemies detect player within a certain range and chase them.
- **Health and Damage**: Enemies have health and deal damage to the player upon collision.
- **XP Contribution**: Upon death, enemies grant XP to the player.

### 2.4 Target.cs
The `Target` class serves as a base for destructible objects - derbis, including enemies.

- **Randomized Properties**: Targets generate random shapes and colors, influencing their health and XP values (other deviation from the og diep.io).
- **Collision and Impulse**: Handles physical interactions with bullets and player collisions, including impulse-based knockbacks.

### 2.5 HealthManager.cs
Manages the player's health, damage intake, and healing over time.

- **Health System**: Tracks current and maximum health, manages damage intake, and healing mechanics.
- **Healing Mechanism**: Starts healing after a delay when not taking damage.
- **Signals**: Emits a signal when the player's health reaches zero.

### 2.6 LevelManager.cs
Manages the XP system, levels, and upgrade points.

- **XP Tracking**: Tracks current XP and checks for level-ups.
- **Leveling Logic**: Levels are determined by XP thresholds, which increase exponentially.
- **Upgrade Points**: Grants upgrade points upon leveling, which can be spent on various stats, each stat plays a "tts" upon triggering.

### 2.7 UpgradeManager.cs
Handles the upgrade system for various player stats.

- **Stat Management**: Manages stat levels and values, including health, bullet speed, and damage.
- **Upgrade System**: Allows players to spend points to improve stats. Provides feedback through sounds.
- **Reset Functionality**: Allows resetting of all upgrades, refunding points.

## 3. Gameplay Mechanics

### Player Actions
- **Movement**: Controlled via WASD or arrow keys.
- **Shooting**: Toggle auto-shoot with "E". Aim using the mouse.
- **Upgrading**: Use number keys (1-8) to upgrade stats.
    - 1 - Healing speed
    - 2 - Maximum Health
    - 3 - Body damage
    - 4 - Bullet speed
    - 5 - Bullet durability
    - 6 - Bullet damage
    - 7 - Reloading speed
    - 8 - Movement speed
    - 0 - stats reset

### Enemies
- **Behavior**: Enemies chase the player when in sight and deal damage upon collision.
- **XP Rewards**: Enemies grant XP upon death, contributing to the player’s level progression.

### Junk or Derbis
- **Behavior**: Static or moving obstacles that can be destroyed for XP.

### XP and Leveling
- **System**: XP is gained from defeating enemies and junk, contributing to leveling up which sometimes earns upgrade points. (First 15 levels each time, till lvl30 its every odd level and since then every third level, account for 45 being the top level).
- **Upgrades**: Stats can be improved by spending upgrade points on various attributes mentioned earlier.

## 4. Controls
- **Movement**: WASD / Arrow Keys
- **Shooting**: Left Mouse Button / Auto-Shoot (E)
- **Upgrading**: Numpad or Number Keys (1-8)
- **Other Actions**: Reset upgrades (0)

## 5. Technical Details

### Game Architecture
- The game follows an object-oriented design using the Godot engine’s scene and node-based system.
- **Interaction Between Components**: The `Player` interacts with `Enemy`, `Bullet`, `LevelManager`, `HealthManager`, and `UpgradeManager` to handle gameplay flow.
- **Performance Considerations**: Includes mechanisms to handle multiple enemies and bullets efficiently, though attention to collision handling and memory management is crucial.
