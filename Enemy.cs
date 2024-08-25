namespace diep;
using Godot;

public partial class Enemy : Target
{
	private float _enemyHealth = 5f;
	private float _enemySpeed = 50f;
	public float _enemyDamage = 15f;
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
			LinearVelocity = directionToPlayer * _enemySpeed;
		}
		else
		{
			LinearVelocity = Vector2.Zero;
		}
	}
	public void TakeDamage(float damage)
	{
		_enemyHealth -= damage;
		if (_enemyHealth <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		QueueFree();
	}
}
