namespace diep;
using Godot;
using System;
using System.Collections.Generic;

public partial class Player : RigidBody2D
{
	//diep stats
	[Export] public float HealingAmount = 0.1f;
	[Export] public float Health = 100f;
	[Export] public float BodyDamage = 5f;
	[Export] public float BulletSpeed = 400f;
	[Export] public float BulletDamage = 20f;
	[Export] public float ShootRate = 10f;
	[Export] public float Speed = 200f;

	
	[Export] public PackedScene TargetScene = (PackedScene)ResourceLoader.Load("res://Target.tscn");
	[Export] public int TargetSpawnRange = 300;
	
	//shooting
	private bool _autoShootEnabled;
	private float _shootTimer;
	private PackedScene _bulletScene;
	private Timer _spawnTimer;
	
	//xp
	private int _currentXP;
	private int _level;
	private int _upgradePoints;
	private int Maxlevel = 45;
	private List<int> _xpToLevels;
	[Export] public int BaseXP = 200;
	[Export] public float ExponentialFactor = 1.51f;
	[Export] public int AdditionalXP = 50;
	
	//collisions
	private bool _collisionCooldown;
	private Timer _collisionCooldownTimer;
	
	//healing
	private float _currentHP = 100;
	float TimeToStartHealing = 3.0f;
	private float _timeSinceLastDamage;
	private bool _isHealing;
	private Timer _healingTimer;


	public override void _Ready()
	{
		GravityScale = 0;
		
		_bulletScene = ResourceLoader.Load<PackedScene>("res://Bullet.tscn");
		
		//shooting
		_shootTimer = 0;
		_spawnTimer = new Timer();
		_spawnTimer.OneShot = true;
		AddChild(_spawnTimer);
		_spawnTimer.Timeout += SpawnSingleTarget;
		ScheduleNextSpawn();
		
		//healing
		_healingTimer = new Timer();
		_healingTimer.WaitTime = 0.1f;
		_healingTimer.OneShot = true;
		AddChild(_healingTimer);
		_healingTimer.Timeout += () => _isHealing = true;
		
		//collisions
		ContactMonitor = true;
		MaxContactsReported = 5;
		_collisionCooldownTimer = new Timer();
		_collisionCooldownTimer.WaitTime = 0.2f;
		_collisionCooldownTimer.OneShot = true;
		AddChild(_collisionCooldownTimer);
		_collisionCooldownTimer.Timeout += () => _collisionCooldown = false;
		
		//xp
		_xpToLevels = CalculateXPRequirements(Maxlevel, BaseXP, ExponentialFactor, AdditionalXP);
	}
	public override void _Process(double delta)
	{
		if (_timeSinceLastDamage < TimeToStartHealing)
		{
			_timeSinceLastDamage += (float)delta;
		}
		else
		{
			_isHealing = true;
		}

		if (_isHealing && _healingTimer.IsStopped())
		{
			HealPlayer();
		}

		HandleMovement();
		HandleShooting((float)delta);
		HandleUpgradeInputs();
	}
	public override void _PhysicsProcess(double delta)
	{
		AngularVelocity = 0;
		Rotation = 0;
		RotationDegrees = 0;
		
		foreach (var body in GetCollidingBodies())
		{
			if (body is Target target)
			{
				HandlePlayerTargetCollision(target);
			}
		}
		
	}

	private void HandleMovement()
	{
		Vector2 inputVelocity = GetInput();
		LinearVelocity = inputVelocity;
	}
	
	private void HandlePlayerTargetCollision(RigidBody2D target)
	{
		if (_collisionCooldown)
			return;
		
		TakeDamage(BodyDamage);
		target.Call("TakeDamage", BodyDamage);

		Vector2 collisionDirection = GlobalPosition.DirectionTo(target.GlobalPosition).Normalized();
		float playerBounceStrength = 300f;
		float targetBounceStrength = 5000f;

		ApplyCentralImpulse(collisionDirection * playerBounceStrength);
		target.ApplyCentralImpulse(collisionDirection * targetBounceStrength);
		Rotation = 0;
		AngularVelocity = 0;

		_collisionCooldown = true;
		_collisionCooldownTimer.Start();
		GD.Print($"Player took damage, has {_currentHP} HP, and was bounced away from the target!");
	}
	
	private void HandleShooting(double delta)
	{
		if (Input.IsActionJustPressed("toggle_auto_shoot")) //prees E keyboard
		{
			_autoShootEnabled = !_autoShootEnabled;
		}

		_shootTimer += (float)delta;

		if (_autoShootEnabled && _shootTimer >= 1.0 / ShootRate)
		{
			Shoot();
			_shootTimer = 0;
		}

		else if (!_autoShootEnabled && Input.IsActionPressed("shoot") && _shootTimer >= 1.0 / ShootRate)
		{
			Shoot();
			_shootTimer = 0;
		}
	}
	private Vector2 GetInput()
	{
		Vector2 inputVector = Vector2.Zero;

		// handles WASD input as well as arrows
		if (Input.IsActionPressed("move_right"))
			inputVector.X += 1;
		if (Input.IsActionPressed("move_left"))
			inputVector.X -= 1;
		if (Input.IsActionPressed("move_down"))
			inputVector.Y += 1;
		if (Input.IsActionPressed("move_up"))
			inputVector.Y -= 1;

		return inputVector.Normalized() * Speed;
	}

	private void Shoot()
	{
		Vector2 mousePosition = GetGlobalMousePosition();
		Vector2 direction = (mousePosition - GlobalPosition).Normalized();

		var bullet = _bulletScene.Instantiate<Bullet>();
		bullet.GlobalPosition = GlobalPosition;
		bullet.Rotation = direction.Angle();

		if (bullet is Bullet bulletScript)
		{
			bulletScript.Direction = direction;
			bulletScript.Speed = BulletSpeed;
			bulletScript.Damage = BulletDamage;
		}

		GetParent().AddChild(bullet);
	}
	
	private void ScheduleNextSpawn()
	{
		RandomNumberGenerator rng = new RandomNumberGenerator();
		rng.Randomize();

		float randomInterval = rng.RandfRange(1.0f, 2.0f); //tweak this --------------------------------

		_spawnTimer.WaitTime = randomInterval;
		_spawnTimer.Start();
	}

	private void SpawnSingleTarget()
	{
		RandomNumberGenerator rng = new RandomNumberGenerator();
		rng.Randomize();

		Vector2 randomOffset = new Vector2(
			rng.RandfRange(-TargetSpawnRange, TargetSpawnRange),
			rng.RandfRange(-TargetSpawnRange, TargetSpawnRange)
		);

		Vector2 targetPosition = GlobalPosition + randomOffset;
		
		var target = TargetScene.Instantiate<Target>();
		target.GlobalPosition = targetPosition;

		float initialImpulseX = rng.RandfRange(-50f, 50f);
		float initialImpulseY = rng.RandfRange(-50f, 50f);
		Vector2 initialImpulse = new Vector2(initialImpulseX, initialImpulseY);
		//GD.Print($"Generated Initial Impulse: {initialImpulse}");

		target.SetInitialImpulse(initialImpulse, 10f);

		GetParent().CallDeferred("add_child", target);
		ScheduleNextSpawn();
	}
	private void HealPlayer()
	{
		_currentHP += HealingAmount;
		_currentHP = Mathf.Min(_currentHP, Health);


		if (_currentHP >= Health)
		{
			_isHealing = false;
		}
		else
		{
			//GD.Print($"Healing... Current HP: {_currentHP}");
			_isHealing = false;
			_healingTimer.Start();
		}
	}
	
	public void TakeDamage(float damage)
	{
		_currentHP -= damage;
		_timeSinceLastDamage = 0f;
		_isHealing = false;
		_healingTimer.Stop();

		if (_currentHP <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		GD.Print("Player has died!");
		QueueFree(); //----------------------------------------------------trigger a game-over screen
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
		else if (_level <= Maxlevel)
		{
			if (_level % 3 == 0)
			{
				_upgradePoints++;
			}
		}

		GD.Print($"Upgrade Points Earned: {_upgradePoints}");
	}
	private void HandleUpgradeInputs()
	{
		if (Input.IsActionJustPressed("upgrade_1"))
		{
			SpendUpgradePoint("HealingAmount");
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
			SpendUpgradePoint("BulletDamage");
		}
		else if (Input.IsActionJustPressed("upgrade_6"))
		{
			SpendUpgradePoint("ShootRate");
		}
		else if (Input.IsActionJustPressed("upgrade_7"))
		{
			SpendUpgradePoint("Speed");
		}
	}
	
	public void SpendUpgradePoint(string stat)
	{
		if (_upgradePoints > 0)
		{
			_upgradePoints--;
			
			switch (stat)
			{
				case "HealingAmount":
					HealingAmount += 0.05f;
					break;
				case "Health":
					Health += 20f;
					break;
				case "BodyDamage":
					BodyDamage += 5f;
					break;
				case "BulletSpeed":
					BulletSpeed += 50f;
					break;
				case "BulletDamage":
					BulletDamage += 10f;
					break;
				case "ShootRate":
					ShootRate += 2f;
					break;
				case "Speed":
					Speed += 20f;
					break;
				default:
					GD.Print("Invalid stat selected for upgrade.");
					_upgradePoints++;
					break;
			}

			GD.Print($"{stat} upgraded! Current {stat}: {GetStatValue(stat)}");
		}
		else
		{
			GD.Print("No Upgrade Points available!");
		}
	}
	
	private float GetStatValue(string stat)
	{
		return stat switch
		{
			"HealingAmount" => HealingAmount,
			"Health" => Health,
			"BodyDamage" => BodyDamage,
			"BulletSpeed" => BulletSpeed,
			"BulletDamage" => BulletDamage,
			"ShootRate" => ShootRate,
			"Speed" => Speed,
			_ => 0f,
		};
	}
}
