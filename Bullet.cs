namespace diep;
using Godot;
using System;

public partial class Bullet : Area2D
{
	[Export]
	public float Speed = 400f;

	[Export]
	public int Strength = 20;

	public Vector2 Direction;
	
	public override void _Ready()
	{
		this.BodyEntered += OnBulletBodyEntered;
	}
	public override void _Process(double delta)
	{
		Position += Direction * (float)delta * Speed;
	}

	private void OnBulletBodyEntered(Node body)
	{
		if (body is Target target)
		{
			Vector2 impulse = Direction * Strength;
			target.ApplyCentralImpulse(impulse);

			target.TakeDamage(Strength);
			QueueFree();
		}
	}
}
