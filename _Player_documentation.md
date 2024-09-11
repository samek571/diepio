 
# Player Documentation: Single-Player Diep.io Clone

## 1. Introduction

This game is a single-player version of [diep.io](https://diep.io) made in the Godot engine using C# language. The player controls a tank that shoots bullets to destroy enemies and debris, earning experience points (XP) to level up and improve stats of a tank. The game emphasizes strategic upgrades and survival against enemies. It was my second actual project, I got forced into something like this by the university.

## 2. Gameplay Mechanics

### Player Actions
- **Movement**: Controlled via WASD or Arrow keys.
- **Shooting**: Toggle auto-shoot with "E". Aim using the mouse. Be aware that bullets speed decrease over time.
- **Upgrading**: Use number keys (1-8) or numpad to upgrade stats (a sound should be played upon pressing):
    - 1 - Healing speed
    - 2 - Maximum Health
    - 3 - Body damage
    - 4 - Bullet speed
    - 5 - Bullet durability
    - 6 - Bullet damage
    - 7 - Reload speed
    - 8 - Movement speed
    - 0 - Reset all stats

### Enemies
- **Behavior**: Enemies chase the player when in sight and deal damage upon collision. No shooting is implemented.
- **XP Rewards**: Enemies grant XP upon death, contributing to the player’s level progression.

### Junk aka Debris
- **Behavior**: Static obstacles (2D colorful polygon) with small initial movement vector can be destroyed for XP..The amount of HP and XP are determined by the color and amount of vertices respectively.

### XP and Leveling
- **System**: XP is gained from defeating enemies and debris, contributing to leveling up, which sometimes earns upgrade points. (Every level up to 15, every odd level from 16-30, and every third level from 31-45)
- **Upgrades**: Stats can be improved by spending upgrade points on various attributes.

## 3. Controls
- **Movement**: WASD / Arrow Keys
- **Shooting**: Left Mouse Button / Auto-Shoot (E)
- **Upgrading**: Number Keys or Numpad (1-8)
- **Reset Upgrades**: Press "0" to reset upgrades.

### Tips for Players
- use auto-shoot as it is helpful only, leaving you to worry only on movement and dodging.
- Prioritize upgrades based on your play style; more aggressive players may favor bullet damage and speed, while defensive players may prioritize health and healing speed.
- Watch the enemy spawn rate and plan your movements accordingly.