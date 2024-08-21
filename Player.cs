namespace diep;
using Godot;
using System;

public partial class Player : CharacterBody2D
{
	/// <summary>
	/// abilities that will can be upgraded over time
	/// </summary>
	[Export]
	public int Speed = 200;

	[Export]
	public int ShootRate = 1;

	private float _shootTimer;
	private PackedScene _bulletScene;

	public override void _Ready()
	{
		_bulletScene = ResourceLoader.Load<PackedScene>("res://Bullet.tscn");
		_shootTimer = 0;
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
		
		// handles wasd as well as arrows
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

        var bullet = _bulletScene.Instantiate<Area2D>();
        bullet.Position = GlobalPosition;
        bullet.Rotation = direction.Angle();

        if (bullet is Bullet bulletScript)
        {
            bulletScript.Direction = direction;
        }

        GetParent().AddChild(bullet);
    }
}
