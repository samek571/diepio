namespace diep;
using Godot;
using System;
using System.Collections.Generic;

public partial class Player : RigidBody2D
{
	//diep stats
	[Export] public float HealingSpeed = 0.2f;
	[Export] public float Health = 100f;
	[Export] public float BodyDamage = 5f;
	[Export] public float BulletSpeed = 400f;
	[Export] public float BulletDurability = 1.0f;
	[Export] public float BulletDamage = 20f;
	[Export] public float ReloadSpeed = 10f;
	[Export] public float MovementSpeed = 200f;

	[Export] public PackedScene TargetScene = (PackedScene)ResourceLoader.Load("res://Target.tscn");
	[Export] public int TargetSpawnRange = 400;
	
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
	
	//help files
	private LevelManager _levelManager;
	private UpgradeManager _upgradeManager;
	private HealthManager _healthManager;

	public override void _Ready()
	{
		GravityScale = 0;
		
		//scenes and files
		_bulletScene = ResourceLoader.Load<PackedScene>("res://Bullet.tscn");
		_levelManager = new LevelManager();
		_upgradeManager = new UpgradeManager(_levelManager);

		_healthManager = new HealthManager(Health, HealingSpeed, 3.0f);
		_healthManager.Connect(nameof(HealthManager.PlayerDied), new Callable(this, nameof(OnPlayerDied)));

		AddChild(_healthManager);
		
		
		//shooting
		_shootTimer = 0;
		_spawnTimer = new Timer();
		_spawnTimer.OneShot = true;
		AddChild(_spawnTimer);
		_spawnTimer.Timeout += SpawnSingleTarget;
		ScheduleNextSpawn();
		
		//collisions
		ContactMonitor = true;
		MaxContactsReported = 5;
		_collisionCooldownTimer = new Timer();
		_collisionCooldownTimer.WaitTime = 0.2f;
		_collisionCooldownTimer.OneShot = true;
		AddChild(_collisionCooldownTimer);
		_collisionCooldownTimer.Timeout += () => _collisionCooldown = false;
		
	}
	public override void _Process(double delta)
	{
		HandleMovement();
		HandleShooting((float)delta);
		_healthManager.UpdateTimeSinceLastDamage((float)delta);
		_healthManager.Heal((float)delta);
		_upgradeManager.HandleUpgradeInputs();
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
		
		_healthManager.TakeDamage(BodyDamage);
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
	}
	
	private void HandleShooting(double delta)
	{
		if (Input.IsActionJustPressed("toggle_auto_shoot")) //prees E keyboard
		{
			_autoShootEnabled = !_autoShootEnabled;
		}

		_shootTimer += (float)delta;

		if (_autoShootEnabled && _shootTimer >= 1.0 / ReloadSpeed)
		{
			Shoot();
			_shootTimer = 0;
		}

		else if (!_autoShootEnabled && Input.IsActionPressed("shoot") && _shootTimer >= 1.0 / ReloadSpeed)
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

		return inputVector.Normalized() * MovementSpeed;
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
			bulletScript.BulletSpeed = BulletSpeed;
			bulletScript.BulletDamage = BulletDamage;
			bulletScript.BulletDurability = BulletDurability;
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
		_healthManager.Heal(HealingSpeed);
	}
	
	public void TakeDamage(float damage)
	{
		_healthManager.TakeDamage(damage);
	}
	
	private void OnPlayerDied()
	{
		GD.Print("Player has died!");
		QueueFree(); // Handle game-over logic, like freeing the player node or showing a game-over screen
		// Optionally trigger a scene change or restart
	}
	
	public void AddXP(int xp)
	{
		_levelManager.AddXP(xp);
	}

	private void HandleUpgradeInputs()
	{
		_upgradeManager.HandleUpgradeInputs();
	}
}
