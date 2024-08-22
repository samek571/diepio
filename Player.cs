namespace diep;
using Godot;

public partial class Player : CharacterBody2D
{
	/// <summary>
	/// abilities that can be upgraded over time as the tank destroys some target enemies
	/// </summary>
	[Export]
	public int Speed = 200;

	[Export]
	public int ShootRate = 10;

	[Export]
	public PackedScene TargetScene = (PackedScene)ResourceLoader.Load("res://Target.tscn");

	[Export]
	public int TargetSpawnRange = 300;

	private float _shootTimer;
	private PackedScene _bulletScene;
	private Timer _spawnTimer;
	private int _currentXP = 0;
	

	public override void _Ready()
	{
		_bulletScene = ResourceLoader.Load<PackedScene>("res://Bullet.tscn");
		_shootTimer = 0;

		_spawnTimer = new Timer();
		_spawnTimer.OneShot = true;
		AddChild(_spawnTimer);
		_spawnTimer.Timeout += SpawnSingleTarget;
		ScheduleNextSpawn();
	}

	public override void _Process(double delta)
	{
		Vector2 velocity = GetInput();
		Velocity = velocity;
		MoveAndSlide();

		_shootTimer += (float)delta;
		if (Input.IsActionPressed("shoot") && _shootTimer >= 1.0 / ShootRate)
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

		var target = TargetScene.Instantiate<Node2D>();
		target.GlobalPosition = targetPosition;

		GetParent().CallDeferred("add_child", target);
		ScheduleNextSpawn();
	}

}
