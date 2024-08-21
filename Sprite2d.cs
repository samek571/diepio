using Godot;
using System;

public partial class Sprite2d : Sprite2D
{
	// Movement speed in pixels per second
	public float Speed = 200;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetProcess(true);  // Enable processing if not already
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 velocity = new Vector2();

		// Check for input to determine the velocity
		if (Input.IsActionPressed("ui_right"))
			velocity.X += Speed * (float)delta;
		if (Input.IsActionPressed("ui_left"))
			velocity.X -= Speed * (float)delta;
		if (Input.IsActionPressed("ui_down"))
			velocity.Y += Speed * (float)delta;
		if (Input.IsActionPressed("ui_up"))
			velocity.Y -= Speed * (float)delta;

		// Apply the velocity to the sprite's position
		Position += velocity;
	}
}
