using System.Collections;

namespace diep;
using Godot;

public partial class HealthManager : Node
{
    private float _currentHP;
    private float _maxHP;
    private float _healingSpeed;
    private bool _isHealing;
    private Timer _healingTimer;
    private float _timeSinceLastDamage;
    private float _timeToStartHealing;
    
    [Signal] public delegate void PlayerDiedEventHandler();

    public HealthManager(float maxHP, float healingSpeed, float timeToStartHealing)
    {
        _maxHP = maxHP;
        _currentHP = maxHP;
        _healingSpeed = healingSpeed;
        _timeToStartHealing = timeToStartHealing;
        _isHealing = false;

        _healingTimer = new Timer();
        _healingTimer.OneShot = true;
        _healingTimer.WaitTime = 0.1f;
        _healingTimer.Timeout += () => _isHealing = true;
        AddChild(_healingTimer);
    }

    public void TakeDamage(float damage)
    {
        _currentHP -= damage;
        _timeSinceLastDamage = 0f;
        _isHealing = false;
        _healingTimer.Stop();

        if (_currentHP <= 0)
        {
            EmitSignal(nameof(PlayerDied));
        }
        GD.Print($"Player took damage, has {_currentHP} HP, and was bounced away from the target!");
    }

    public void Heal(float delta)
    {
        if (_timeSinceLastDamage >= _timeToStartHealing && !_isHealing)
        {
            _isHealing = true;
            _healingTimer.Start();
        }

        if (_isHealing)
        {
            _currentHP += _healingSpeed * delta;
            _currentHP = Mathf.Min(_currentHP, _maxHP);
            //GD.Print($"HEALING, I have {_currentHP}");

            if (_currentHP >= _maxHP)
            {
                _isHealing = false;
                //GD.Print("Player healing finished!");
            }
        }
    }

    public void UpdateTimeSinceLastDamage(float delta)
    {
        _timeSinceLastDamage += delta;
    }

    public float GetCurrentHP() => _currentHP;
    public float GetMaxHP() => _maxHP;
}