namespace diep;
using Godot;
using System.Collections.Generic;

public class UpgradeManager
{
    private readonly Dictionary<string, float> _stats;
    private readonly LevelManager _levelManager;

    public UpgradeManager(LevelManager levelManager)
    {
        _levelManager = levelManager;
        _stats = new Dictionary<string, float>
        {
            {"HealingSpeed", 0.1f},
            {"Health", 100f},
            {"BodyDamage", 5f},
            {"BulletSpeed", 400f},
            {"BulletDurability", 1.0f},
            {"BulletDamage", 20f},
            {"ReloadSpeed", 10f},
            {"MovementSpeed", 200f}
        };
    }

    public void HandleUpgradeInputs()
    {
        if (Input.IsActionJustPressed("upgrade_1"))
        {
            SpendUpgradePoint("HealingSpeed");
        }
        else if (Input.IsActionJustPressed("upgrade_2"))
        {
            SpendUpgradePoint("Health");
        }
        else if (Input.IsActionJustPressed("upgrade_3"))
        {
            SpendUpgradePoint("BodyDamage");
        }
        else if (Input.IsActionJustPressed("upgrade_4"))
        {
            SpendUpgradePoint("BulletSpeed");
        }
        else if (Input.IsActionJustPressed("upgrade_5"))
        {
            SpendUpgradePoint("BulletDurability");
        }
        else if (Input.IsActionJustPressed("upgrade_6"))
        {
            SpendUpgradePoint("BulletDamage");
        }
        else if (Input.IsActionJustPressed("upgrade_7"))
        {
            SpendUpgradePoint("ReloadSpeed");
        }
        else if (Input.IsActionJustPressed("upgrade_8"))
        {
            SpendUpgradePoint("MovementSpeed");
        }
    }

    public void SpendUpgradePoint(string stat)
    {
        if (_levelManager.GetUpgradePoints() > 0)
        {
            _levelManager.SpendUpgradePoint();
            switch (stat)
            {
                case "HealingSpeed":
                    _stats["HealingSpeed"] += 0.05f;
                    break;
                case "Health":
                    _stats["Health"] += 20f;
                    break;
                case "BodyDamage":
                    _stats["BodyDamage"] += 5f;
                    break;
                case "BulletSpeed":
                    _stats["BulletSpeed"] += 50f;
                    break;
                case "BulletDurability":
                    _stats["BulletDurability"] += 0.35f;
                    break;
                case "BulletDamage":
                    _stats["BulletDamage"] += 10f;
                    break;
                case "ReloadSpeed":
                    _stats["ReloadSpeed"] += 2f;
                    break;
                case "MovementSpeed":
                    _stats["MovementSpeed"] += 20f;
                    break;
                default:
                    GD.Print("Invalid stat selected for upgrade.");
                    break;
            }

            GD.Print($"{stat} upgraded! Current {stat}: {GetStatValue(stat)}");
        }
        else
        {
            GD.Print("No Upgrade Points available!");
        }
    }

    public float GetStatValue(string stat)
    {
        return _stats.ContainsKey(stat) ? _stats[stat] : 0f;
    }

    public float GetHealth() => _stats["Health"];
    public float GetHealingSpeed() => _stats["HealingSpeed"];
    public float GetBodyDamage() => _stats["BodyDamage"];
    public float GetBulletSpeed() => _stats["BulletSpeed"];
    public float GetBulletDurability() => _stats["BulletDurability"];
    public float GetBulletDamage() => _stats["BulletDamage"];
    public float GetReloadSpeed() => _stats["ReloadSpeed"];
    public float GetMovementSpeed() => _stats["MovementSpeed"];
}
