namespace diep;
using Godot;

public partial class Bullet : Area2D
{
	[Export]
	public int Speed = 400;

	public Vector2 Direction { get; set; } = Vector2.Zero;

	public override void _PhysicsProcess(double delta)
	{
		Position += Direction * Speed * (float)delta;
	}

	public void _on_Bullet_body_entered(Node body)
	{
		QueueFree();
	}
}
