# Programmer Documentation: Single-Player Diep.io

## 1. Introduction

This documentation provides you with detailed technical overview of the game's architecture, things that get covered are for instance class descriptions, methods, properties as well as their interconnections. Person understands the game's functionality far better than reading comments and code.

## 2. File Descriptions and Class Interconnections

### 2.1 Player.cs
The `Player` class manages the player character's actions, including movement, shooting, collision handling, and XP management.

- **Key Methods**:
  - `HandleMovement()`: Manages player movement using keyboard input.
  - `HandleShooting(double delta)`: Handles bullet shooting mechanics and auto-shoot feature.
  - `TakeDamage(float damage)`: Processes damage dealt to the player and also death.
  - `AddXP(int xp)`: Adds experience points to the player and checks for leveling up.
  - `HandleUpgradeInputs()`: Manages the input for upgrades.
  - Many more that are crucial, but the mentioned methods use them as an helper.

### 2.2 Bullet.cs
The `Bullet` class defines the behavior of bullets shot by the player.

- **Key Methods**:
  - `_Ready()`: Initializes the bullet, sets its speed, and starts the lifespan timer.
  - `_Process(double delta)`: Updates bullet position and adjusts speed - slowing down exponentionally over time, something as a "wear" is implelemnted.
  - `OnBulletBodyEntered(Node body)`: Handles collision with various objects.

### 2.3 Enemy.cs
The `Enemy` class manages enemy behavior and inherits from `Target`.

- **Key Methods**:
  - `Initialize(Player player)`: Sets up enemy behavior with a reference to the player.
  - `_PhysicsProcess(double delta)`: Handles enemy chasing logic.
  - `TakeDamage(float damage)`: Reduces enemy health and checks for death.

### 2.4 Target.cs
The `Target` class serves as a base for destructible objects, including enemies.

- **Key Methods**:
  - `_Ready()`: Initializes target properties like shape, color, and collision polygons.
  - `TakeDamage(int damage)`: Processes damage to the target and checks for destruction.

### 2.5 HealthManager.cs
Manages the player's health, damage intake, and healing over time.

- **Key Methods**:
  - `TakeDamage(float damage)`: Reduces health and stops healing for some time.
  - `Heal(float delta)`: Handles automatic healing over time.
  - `PlayerDied`: Signal emitted when health reaches zero.

### 2.6 LevelManager.cs
Manages the XP system, levels, and upgrade points.

- **Key Methods**:
  - `AddXP(int xp)`: Adds experience points and checks for level-up.
  - `CheckForLevelUp()`: Determines if the player levels up based on XP.
  - `GrantUpgradePoints()`: Awards upgrade points based on the level.

### 2.7 UpgradeManager.cs
Handles the upgrade system for various player stats.

- **Key Methods**:
  - `HandleUpgradeInputs()`: Manages input handling for all upgrades.
  - `SpendUpgradePoint(string stat)`: Processes the spending of upgrade points.
  - `ResetAllUpgrades()`: Resets all upgrades to default values.

## 3. Class Interconnections

- **Player.cs**: Interacts with `Bullet`, `Enemy`, `Target`, `LevelManager`, `HealthManager`, and `UpgradeManager`.
- **Bullet.cs**: Interacts with `Player`, `Enemy`, and `Target` for collision detection.
- **Enemy.cs**: Relies on `Player` for chasing logic and damage interactions.
- **HealthManager.cs**: Used by `Player` to manage health and healing.
- **LevelManager.cs**: Manages XP and levels, linked to `Player` and `UpgradeManager`.
- **UpgradeManager.cs**: Handles upgrades, interacts with `LevelManager` and `Player`.

## 4. Technical Details

### Game Architecture
- OOP design made in Godot 4.3, breathes on engine’s scene and node-based system, made in C# as it might be crystal clear already.
- **Performance Considerations**: None.