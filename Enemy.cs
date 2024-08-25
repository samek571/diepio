namespace diep;
using Godot;

public partial class Enemy : Target
{
	private float _health = 20f;
	private float _speed = 150f;
	private Player _player;
	public void Initialize(Player player)
	{
		_player = player;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (_player != null && IsInstanceValid(_player))
		{
			Vector2 playerPosition = _player.GetPlayerPosition();
			Vector2 directionToPlayer = (playerPosition - GlobalPosition).Normalized();
			LinearVelocity = directionToPlayer * _speed;
		}
		else
		{
			LinearVelocity = Vector2.Zero;
		}
	}
	public void TakeDamage(float damage)
	{
		_health -= damage;
		if (_health <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		QueueFree();
	}
}
