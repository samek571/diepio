namespace diep;
using Godot;

public partial class Enemy : Target
{
	private Vector2 _playerPosition;
	private float _enemyHealth = 5f;
	public float _enemyDamage = 15f;
	private float _sightRange = 300f;
	private float _chaseSpeed = 50f;
	private int _xpValue = 20;
	private Player _player;
	public void Initialize(Player player)
	{
		_player = player;
	}
	public override void _PhysicsProcess(double delta)
	{
		if (_player == null) return;

		_playerPosition = _player.GlobalPosition;
		float distanceToPlayer = Mathf.Sqrt(Mathf.Pow(_playerPosition.X - GlobalPosition.X, 2) + Mathf.Pow(_playerPosition.Y - GlobalPosition.Y, 2));

		if (distanceToPlayer <= _sightRange)
		{
			MoveTowardsPlayer(delta);
		}
		else
		{
			StopMovement();
		}
	}

	private void MoveTowardsPlayer(double delta)
	{
		Vector2 direction = (_playerPosition - GlobalPosition).Normalized();
		Vector2 velocity = direction * _chaseSpeed * (float)delta;
		Position += velocity;
	}
	private void StopMovement()
	{
		LinearVelocity = Vector2.Zero;
	}
	// public override void _PhysicsProcess(double delta)
	// {
	// 	if (_player != null && IsInstanceValid(_player))
	// 	{
	// 		Vector2 playerPosition = _player.GetPlayerPosition();
	// 		Vector2 directionToPlayer = (playerPosition - GlobalPosition).Normalized();
	// 		LinearVelocity = directionToPlayer * _enemySpeed;
	// 	}
	// 	else
	// 	{
	// 		LinearVelocity = Vector2.Zero;
	// 	}
	// }
	public void TakeDamage(float damage)
	{
		_enemyHealth -= damage;
		if (_enemyHealth <= 0)
		{
			_player.AddXP(_xpValue);
			Die();
		}
	}

	private void Die()
	{
		QueueFree();
	}
}
