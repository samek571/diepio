namespace diep;
using Godot;
using System;
using System.Collections.Generic;

public class LevelManager
{
    private int _currentXP;
    private int _level;
    private int _upgradePoints;
    private const int MaxLevel = 45;
    private List<int> _xpToLevels;

    [Export] public int BaseXP = 200;
    [Export] public float ExponentialFactor = 1.51f;
    [Export] public int AdditionalXP = 50;

    public LevelManager()
    {
        _currentXP = 0;
        _level = 0;
        _upgradePoints = 0;
        _xpToLevels = CalculateXPRequirements(MaxLevel, BaseXP, ExponentialFactor, AdditionalXP);
    }

    public void AddXP(int xp)
    {
        _currentXP += xp;
        CheckForLevelUp();
    }

    private void CheckForLevelUp()
    {
        while (_level < _xpToLevels.Count && _currentXP >= _xpToLevels[_level])
        {
            _level++;
            GD.Print($"Level Up! New Level: {_level}");
            GrantUpgradePoints();
        }
    }

    private List<int> CalculateXPRequirements(int maxLevels, int q, float k, int p)
    {
        List<int> xpRequirements = new List<int>();

        for (int x = 0; x < maxLevels; x++)
        {
            int xpRequired = (int)Math.Ceiling(q * Math.Pow(x, k) + p);
            xpRequirements.Add(xpRequired);
        }

        return xpRequirements;
    }

    private void GrantUpgradePoints()
    {
        if (_level <= 15)
        {
            _upgradePoints++;
        }
        else if (_level <= 30)
        {
            if (_level % 2 != 0)
            {
                _upgradePoints++;
            }
        }
        else if (_level <= MaxLevel)
        {
            if (_level % 3 == 0)
            {
                _upgradePoints++;
            }
        }

        GD.Print($"Upgrade Points Earned: {_upgradePoints}");
    }

    public int GetUpgradePoints() => _upgradePoints;

    public void SpendUpgradePoint()
    {
        if (_upgradePoints > 0)
        {
            _upgradePoints--;
        }
        else
        {
            GD.Print("No Upgrade Points available!");
        }
    }
}
