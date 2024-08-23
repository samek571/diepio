namespace diep;
using Godot;

public partial class Bullet : Area2D
{
	public Vector2 Direction { get; set; }
	public float BulletSpeed { get; set; }
	public float BulletDamage { get; set; }
	public float BulletDurability { get; set; }
	
	private Timer _lifespanTimer;
	private float _initialSpeed;
	private float _elapsedTime;
	
	public override void _Ready()
	{
		_initialSpeed = BulletSpeed;
		_elapsedTime = 0f;
		
		_lifespanTimer = new Timer();
		_lifespanTimer.WaitTime = BulletDurability;
		_lifespanTimer.OneShot = true;
		_lifespanTimer.Timeout += () => QueueFree();
		AddChild(_lifespanTimer);
		_lifespanTimer.Start();
		
		this.BodyEntered += OnBulletBodyEntered;
	}
	
	public override void _Process(double delta)
	{
		_elapsedTime += (float)delta;

		//float remainingFraction = Mathf.Max(0, 1 - (_elapsedTime / BulletDurability));
		//BulletSpeed = _initialSpeed * remainingFraction; //linear
		
		BulletSpeed = _initialSpeed * Mathf.Pow(0.3f, _elapsedTime); //exponential
		
		Position += Direction * (float)delta * BulletSpeed;
	}
	
	private void OnCollision(Node body)
	{
		if (body is Player player)
		{
			player.TakeDamage(BulletDamage);
		}
		else if (body is Target target)
		{
			target.Call("TakeDamage", BulletDamage);
		}
		QueueFree();
	}

	private void OnBulletBodyEntered(Node body)
	{
		if (body is Target target)
		{
			Vector2 impulse = Direction * BulletDamage;
			target.ApplyCentralImpulse(impulse);

			target.Call("TakeDamage", BulletDamage);
			QueueFree();
		}
	}
}
