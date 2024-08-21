namespace diep;
using Godot;
using System;

public partial class Target : RigidBody2D
{
	[Export]
	public int Health = 100;
	[Export]
	public int XPValue = 50;

	public void TakeDamage(int amount)
	{
		Health -= amount;
		if (Health <= 0)
		{
			EmitSignal(nameof(DestroyedEventHandler), XPValue);
			QueueFree();
		}
	}

	[Signal]
	public delegate void DestroyedEventHandler(int xp);
}
