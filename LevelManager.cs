namespace diep;
using Godot;
using System;
using System.Collections.Generic;

public class LevelManager
{
	public int _currentXP;
	public int _upgradePoints;
	private int _level;
	private const int MaxLevel = 45;
	public List<int> _xpToLevels;
	public int _spentPoints;

	[Export] public int BaseXP = 200;
	[Export] public float ExponentialFactor = 1.21f;
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

	public List<int> CalculateXPRequirements(int maxLevels, int q, float k, int p)
	{
		List<int> xpRequirements = new List<int>();

		for (int x = 0; x < maxLevels; x++)
		{
			int xpRequired = (int)Math.Ceiling(q * Math.Pow(x, k) + p);
			xpRequirements.Add(xpRequired);
		}

		return xpRequirements;
	}
	public int GetXPForNextLevel()
	{
		if (_level < _xpToLevels.Count)
			return _xpToLevels[_level];
		else
			return int.MaxValue;
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
			_spentPoints += 1;
		}
		else
		{
			GD.Print("No Upgrade Points available!");
		}
	}
	
	public int GetCurrentLevelXP()
	{
		int currentLevelIndex = Math.Max(0, _level - 1);
		return _xpToLevels[currentLevelIndex];
	}

	public int GetNextLevelXP()
	{
		if (_level < _xpToLevels.Count)
			return _xpToLevels[_level];
		return int.MaxValue;
	}

	public int GetCurrentXPWithinLevel()
	{
		int currentLevelXP = GetCurrentLevelXP();
		return _currentXP - currentLevelXP;
	}

	public int GetXPRangeForCurrentLevel()
	{
		int currentLevelXP = GetCurrentLevelXP();
		int nextLevelXP = GetNextLevelXP();
		return nextLevelXP - currentLevelXP;
	}
	
}
