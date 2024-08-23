namespace diep;
using Godot;

public partial class Bullet : Area2D
{
	public Vector2 Direction { get; set; }
	public float Speed { get; set; }
	public float Damage { get; set; }
	
	public override void _Ready()
	{
		this.BodyEntered += OnBulletBodyEntered;
	}
	
	public override void _Process(double delta)
	{
		Position += Direction * (float)delta * Speed;
	}

	
	private void OnCollision(Node body)
	{
		if (body is Player player)
		{
			player.TakeDamage(Damage);
		}
		else if (body is Target target)
		{
			target.Call("TakeDamage", Damage);
		}

		QueueFree();
	}
	private void OnBulletBodyEntered(Node body)
	{
		if (body is Target target)
		{
			Vector2 impulse = Direction * Damage;
			target.ApplyCentralImpulse(impulse);

			target.Call("TakeDamage", Damage);
			QueueFree();
		}
	}
}
