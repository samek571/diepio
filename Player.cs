namespace diep;
using Godot;

public partial class Player : RigidBody2D
{
	//diep stats
	[Export] public float HealingSpeed = 0.2f;
	[Export] public float Health = 100f;
	[Export] public float BodyDamage = 50f;
	[Export] public float BulletSpeed = 400f;
	[Export] public float BulletDurability = 1.2f;
	[Export] public float BulletDamage = 40f;
	[Export] public float ReloadSpeed = 5f;
	[Export] public float MovementSpeed = 200f;
	
	//target
	[Export] public PackedScene TargetScene = (PackedScene)ResourceLoader.Load("res://Target.tscn");
	[Export] public int TargetSpawnRange = 350;
	private float targetSpawnTime = 2.2f;
	
	//enemy	
	[Export] public PackedScene EnemyScene = (PackedScene)ResourceLoader.Load("res://Enemy.tscn");
	private Timer _enemySpawnTimer;
	private RandomNumberGenerator _rng;
	[Export] public int EnemySpawnRange = 600;
	
	//shooting
	private bool _autoShootEnabled;
	private float _shootTimer;
	private PackedScene _bulletScene;
	private Timer _spawnTimer;
	
	//xp
	private int _level;
	private int _upgradePoints;
	
	//collisions
	private bool _collisionCooldown;
	private Timer _collisionCooldownTimer;
	
	//help files
	private LevelManager _levelManager;
	private UpgradeManager _upgradeManager;
	private HealthManager _healthManager;
	private PackedScene _enemyScene;
	
	//health + xpbar
	private ProgressBar _healthBar;
	private ProgressBar _xpBar;
	private float XPBarWidth = DisplayServer.WindowGetSize().X;
	private float XPBarHeight = DisplayServer.WindowGetSize().Y *0.03f; //scaling factor
	
	public override void _Ready()
	{
		GravityScale = 0;
		
		//scenes and files
		_bulletScene = ResourceLoader.Load<PackedScene>("res://Bullet.tscn");
		_healthManager = new HealthManager(Health, HealingSpeed, 3.5f);
		_healthManager.Connect(nameof(HealthManager.PlayerDied), new Callable(this, nameof(OnPlayerDied)));
		AddChild(_healthManager);
		
		//audio
		var upgradeSoundPlayer = new AudioStreamPlayer();
		AddChild(upgradeSoundPlayer);
		_levelManager = new LevelManager();
		_upgradeManager = new UpgradeManager(_levelManager, upgradeSoundPlayer);

		//enemy
		_enemyScene = ResourceLoader.Load<PackedScene>("res://Enemy.tscn");
		var enemySpawnTimer = new Timer();
		enemySpawnTimer.WaitTime = 12.0f;
		enemySpawnTimer.OneShot = false;
		enemySpawnTimer.Timeout += SpawnEnemiesInBulk;
		AddChild(enemySpawnTimer);
		enemySpawnTimer.Start();
		_rng = new RandomNumberGenerator();
		_rng.Randomize();
		
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
		
		//healthbar
		_healthManager._currentHP = Health;
		_healthBar = GetNode<ProgressBar>("ProgressBar");
		_healthBar.MaxValue = Health;
		_healthBar.Value = _healthManager._currentHP;
		
		//xpbar
		InitializeXPBar();
	}
	public override void _Process(double delta)
	{
		HandleMovement();
		HandleShooting((float)delta);
		_healthManager.UpdateTimeSinceLastDamage((float)delta);
		_healthManager.Heal((float)delta);
		_upgradeManager.HandleUpgradeInputs();
		_healthBar.Value = _healthManager._currentHP;
		
		//update xpbar size
		base._Process(delta);
		UpdateXPBarSize();
		
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
		
		if (target is Enemy enemy)
		{
			TakeDamage(enemy._enemyDamage);
			enemy.TakeDamage(BodyDamage);
		}
		else if (target is Target targetObject)
		{
			_healthManager.TakeDamage(targetObject._targetDamage);
			target.Call("TakeDamage", BodyDamage);
		}

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

		if (bullet is { } bulletScript)
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

		float randomInterval = rng.RandfRange(-1.0f, 1.0f);

		_spawnTimer.WaitTime = targetSpawnTime + randomInterval;
		_spawnTimer.Start();
	}

	private void SpawnSingleTarget()
	{
		RandomNumberGenerator rng = new RandomNumberGenerator();
		rng.Randomize();
		
		float minDistance = 75f;
		float maxDistance = TargetSpawnRange;
		float angle = rng.RandfRange(0, Mathf.Tau);
		float distance = rng.RandfRange(minDistance, maxDistance);

		Vector2 randomOffset = new Vector2(
			Mathf.Cos(angle) * distance,
			Mathf.Sin(angle) * distance
		);

		Vector2 targetPosition = GlobalPosition + randomOffset;
		
		var target = TargetScene.Instantiate<Target>();
		target.GlobalPosition = targetPosition;

		float initialImpulseX = rng.RandfRange(-50f, 50f);
		float initialImpulseY = rng.RandfRange(-50f, 50f);
		Vector2 initialImpulse = new Vector2(initialImpulseX, initialImpulseY);

		target.SetInitialImpulse(initialImpulse, 10f);

		GetParent().CallDeferred("add_child", target);
		ScheduleNextSpawn();
	}
	
	private void SpawnEnemiesInBulk()
	{
		RandomNumberGenerator rng = new RandomNumberGenerator();
		rng.Randomize();
		
		int enemyCount = rng.RandiRange(2, 4);
		float minDistance = 50f;  
		float maxDistance = TargetSpawnRange;
		for (int i = 0; i < enemyCount; i++)
		{
			
			float angle = rng.RandfRange(0, Mathf.Tau);
			float distance = rng.RandfRange(minDistance, maxDistance);

			Vector2 randomOffset = new Vector2(
				Mathf.Cos(angle) * distance,
				Mathf.Sin(angle) * distance
			);

			Vector2 spawnPosition = GlobalPosition + randomOffset;

			var enemy = _enemyScene.Instantiate<Enemy>();
			enemy.GlobalPosition = spawnPosition;

			enemy.Initialize(this);
			GetParent().CallDeferred("add_child", enemy);
		}
		ScheduleNextEnemySpawn();
	}

	private void ScheduleNextEnemySpawn()
	{
		if (_enemySpawnTimer != null)
		{
			RandomNumberGenerator rng = new RandomNumberGenerator();
			rng.Randomize();

			float randomInterval = rng.RandfRange(1.0f, 2.0f);
			_enemySpawnTimer.WaitTime = randomInterval;
			_enemySpawnTimer.Start();
		}
	}

	private void HealPlayer()
	{
		_healthManager.Heal(HealingSpeed);
	}
	
	public void TakeDamage(float damage)
	{
		_healthManager._currentHP -= damage;
		_healthBar.Value -= Mathf.Clamp(_healthManager._currentHP, 0, Health);
	}
	
	private void OnPlayerDied()
	{
		GD.Print("Player has died!");
		QueueFree();
	}
	private void InitializeXPBar()
	{
		_xpBar = new ProgressBar();
		_xpBar.Size = new Vector2(XPBarWidth, XPBarHeight);
		_xpBar.MaxValue = _levelManager.GetXPForNextLevel();
		_xpBar.Value = _levelManager._currentXP;
		_xpBar.Position = new Vector2(-XPBarWidth/2,300);
		AddChild(_xpBar);
	}
	public void UpdateXPBar()
	{
		int currentXPWithinLevel = _levelManager.GetCurrentXPWithinLevel();
		int xpRangeForCurrentLevel = _levelManager.GetXPRangeForCurrentLevel();

		_xpBar.MaxValue = xpRangeForCurrentLevel;
		_xpBar.Value = currentXPWithinLevel;
	}
	public void AddXP(int xp)
	{
		_levelManager.AddXP(xp);
		UpdateXPBar();
	}

	private void HandleUpgradeInputs()
	{
		_upgradeManager.HandleUpgradeInputs();
	}
	
	public Vector2 GetPlayerPosition()
	{
		return GlobalPosition;
	}
	
	public void UpdateHealthBar(float newMaxHealth)
	{
		_healthManager._maxHP = newMaxHealth;
		_healthBar.MaxValue = newMaxHealth;
		_healthBar.Value = Mathf.Min(_healthManager._currentHP, newMaxHealth);
		GD.Print("Health bar updated: Max Health = " + newMaxHealth);
	}
	
	public void ResetHealthBar()
	{
		_healthManager._maxHP = 100;
		_healthBar.MaxValue = 100;
		_healthBar.Value = Mathf.Min(_healthManager._currentHP, 100);
		GD.Print("Health bar reset to original settings.");
	}
	
	
	private void UpdateXPBarSize()
	{
		Vector2 screenSize = DisplayServer.WindowGetSize();
	
		float xpBarWidth = screenSize.X * 1f;
		float xpBarHeight = screenSize.Y * 0.03f;
		_xpBar.Size = new Vector2(xpBarWidth, xpBarHeight);
		
		Vector2 xpBarPosition = new Vector2(screenSize.X - 1.5f*xpBarWidth, xpBarHeight+screenSize.Y/2 - 2.4f*xpBarHeight);
		_xpBar.Position = xpBarPosition;
		
		//color - no function needed...		
		var theme = new Theme();
		var fgStyle = new StyleBoxFlat();
		fgStyle.BgColor = new Color(1.0f, 0.8f, 0.0f);
		var bgStyle = new StyleBoxFlat();
		bgStyle.BgColor = new Color(0.2f, 0.2f, 0.2f);

		theme.SetStylebox("fill", "ProgressBar", fgStyle);
		theme.SetStylebox("background", "ProgressBar", bgStyle);
		_xpBar.Theme = theme;
	}
}
