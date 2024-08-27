namespace diep;
using Godot;
using System.Collections.Generic;

public class UpgradeManager
{
    private const int MaxUpgradeLevel = 7;
    private readonly Dictionary<string, (float value, int level)> _stats;
    
    private readonly LevelManager _levelManager;

    private readonly Dictionary<string, AudioStream> _upgradeSounds;
    private AudioStreamPlayer _audioPlayer;
    public UpgradeManager(LevelManager levelManager, AudioStreamPlayer audioPlayer)
    {
        _levelManager = levelManager;
        _audioPlayer = audioPlayer;
        _stats = new Dictionary<string, (float value, int level)>
        {
            {"HealingSpeed", (0.15f, 0)},
            {"Health", (100f, 0)},
            {"BodyDamage", (5f, 0)},
            {"BulletSpeed", (400f, 0)},
            {"BulletDurability", (1.0f, 0)},
            {"BulletDamage", (20f, 0)},
            {"ReloadSpeed", (10f, 0)},
            {"MovementSpeed", (200f, 0)}
        };
        
        _upgradeSounds = new Dictionary<string, AudioStream>
        {
            {"HealingSpeed", (AudioStream)ResourceLoader.Load("res://sounds/1.wav")},
            {"Health", (AudioStream)ResourceLoader.Load("res://sounds/2.wav")},
            {"BodyDamage", (AudioStream)ResourceLoader.Load("res://sounds/3.wav")},
            {"BulletSpeed", (AudioStream)ResourceLoader.Load("res://sounds/4.wav")},
            {"BulletDurability", (AudioStream)ResourceLoader.Load("res://sounds/5.wav")},
            {"BulletDamage", (AudioStream)ResourceLoader.Load("res://sounds/6.wav")},
            {"ReloadSpeed", (AudioStream)ResourceLoader.Load("res://sounds/7.wav")},
            {"MovementSpeed", (AudioStream)ResourceLoader.Load("res://sounds/8.wav")},
            {"upgrade_reset", (AudioStream)ResourceLoader.Load("res://sounds/upgrade_reset.wav")}
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
        else if (Input.IsActionJustPressed("upgrade_reset"))
        {
            ResetAllUpgrades();
        }
    }

    public void SpendUpgradePoint(string stat)
    {
        if (_levelManager.GetUpgradePoints() > 0)
        {
            var statData = _stats[stat];
            if (statData.level >= MaxUpgradeLevel)
            {
                GD.Print($"{stat} is already at the maximum level of {MaxUpgradeLevel}.");
                return;
            }
            
            _levelManager.SpendUpgradePoint();
            switch (stat)
            {
                case "HealingSpeed":
                    statData.value += 0.05f;
                    break;
                case "Health":
                    statData.value += 20f;
                    break;
                case "BodyDamage":
                    statData.value += 5f;
                    break;
                case "BulletSpeed":
                    statData.value += 50f;
                    break;
                case "BulletDurability":
                    statData.value += 0.35f;
                    break;
                case "BulletDamage":
                    statData.value += 10f;
                    break;
                case "ReloadSpeed":
                    statData.value += 2f;
                    break;
                case "MovementSpeed":
                    statData.value += 20f;
                    break;
                default:
                    GD.Print("Invalid stat selected for upgrade.");
                    break;
            }
            statData.level++;
            _stats[stat] = statData;
            GD.Print($"{stat} upgraded! Current {stat}: {statData.value}, Level: {statData.level}");
            PlayUpgradeSound(stat);
        }
        else
        {
            GD.Print("No Upgrade Points available!");
        }
    }
    private void ResetAllUpgrades()
    {
        _stats["HealingSpeed"] = (0.15f, 0);
        _stats["Health"] = (100f, 0);
        _stats["BodyDamage"] = (5f, 0);
        _stats["BulletSpeed"] = (400f, 0);
        _stats["BulletDurability"] = (1.0f, 0);
        _stats["BulletDamage"] = (20f, 0);
        _stats["ReloadSpeed"] = (10f, 0);
        _stats["MovementSpeed"] = (200f, 0);

        _levelManager._upgradePoints += _levelManager._spentPoints;
        _levelManager._spentPoints = 0;

        GD.Print("All upgrades have been reset, and points have been refunded!");
        PlayUpgradeSound("upgrade_reset");
    }
    
    private void PlayUpgradeSound(string stat)
    {
        if (_upgradeSounds.ContainsKey(stat) || ((stat == "upgrade_reset") && (_levelManager._spentPoints > 0)))
        {
            _audioPlayer.Stream = _upgradeSounds[stat];
            _audioPlayer.Play();
        }
    }
    
    public float GetHealth() => _stats["Health"].value;
    public float GetHealingSpeed() => _stats["HealingSpeed"].value;
    public float GetBodyDamage() => _stats["BodyDamage"].value;
    public float GetBulletSpeed() => _stats["BulletSpeed"].value;
    public float GetBulletDurability() => _stats["BulletDurability"].value;
    public float GetBulletDamage() => _stats["BulletDamage"].value;
    public float GetReloadSpeed() => _stats["ReloadSpeed"].value;
    public float GetMovementSpeed() => _stats["MovementSpeed"].value;
}
