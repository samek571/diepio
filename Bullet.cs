namespace diep;
using Godot;
using System;

public partial class Bullet : Area2D
{
	[Export]
	public int Speed = 400;

	public Vector2 Direction { get; set; }

	public override void _PhysicsProcess(double delta)
	{
		Position += Direction * Speed * (float)delta;
	}

	public void _on_Bullet_body_entered(Node body)
	{
		QueueFree();
	}
}
