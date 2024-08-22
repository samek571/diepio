namespace diep;
using Godot;

public partial class Player : RigidBody2D
{
	//diep stats
	[Export] public float HealingAmount = 0.1f;
	[Export] public float Health = 100f;
	[Export] public int BodyDamage = 5;
	[Export] public float BulletSpeed = 400f;
	[Export] public int BulletDamage = 20;
	[Export] public int ShootRate = 10;
	[Export] public int Speed = 200;

	
	[Export] public PackedScene TargetScene = (PackedScene)ResourceLoader.Load("res://Target.tscn");
	[Export] public int TargetSpawnRange = 300;

	private bool _autoShootEnabled;
	private float _shootTimer;
	private PackedScene _bulletScene;
	private Timer _spawnTimer;
	private int _currentXP;
	
	//collisions
	private bool _collisionCooldown = false;
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
		AddChild(_collisionCooldownTimer);
		_collisionCooldownTimer.WaitTime = 0.2f;
		_collisionCooldownTimer.OneShot = true;
		_collisionCooldownTimer.Timeout += () => _collisionCooldown = false;
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
		
		int damage = 5;
		TakeDamage(damage);
		target.Call("TakeDamage", damage);

		Vector2 collisionDirection = GlobalPosition.DirectionTo(target.GlobalPosition);
		float playerBounceStrength = 300f;
		float targetBounceStrength = 5000f;

		ApplyCentralImpulse(collisionDirection * playerBounceStrength);
		target.ApplyCentralImpulse(collisionDirection * targetBounceStrength);
		Rotation = 0;
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
	public void AddXP(int xp)
	{
		_currentXP += xp;
		GD.Print($"XP Added: {xp}. Total XP: {_currentXP}");
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
		GD.Print($"Generated Initial Impulse: {initialImpulse}");

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
			GD.Print($"Healing... Current HP: {_currentHP}");
			_isHealing = false;
			_healingTimer.Start();
		}
	}
	
	public void TakeDamage(int damage)
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
		QueueFree(); //trigger a game-over screen
	}
}
