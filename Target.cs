namespace diep;
using Godot;

public partial class Target : StaticBody2D
{
	[Export]
	public int Health = 100;
	[Export]
	public int XPValue = 50;

	private Polygon2D _polygon;
	private CollisionPolygon2D _collisionPolygon;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();


	public override void _Ready()
	{
		_polygon = GetNode<Polygon2D>("Polygon2D");
		_collisionPolygon = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
		
		GenerateRandomPolygon();
		SetRandomColor();
	}
	
	// i couldnt really think of anything better than doing circle and random value +- from the centre, works
	// surely i didnt want to do a bitmap as this is a simple task
	private void GenerateRandomPolygon()
	{
		_rng.Randomize();

		int vertexCount = _rng.RandiRange(3, 12);
		Vector2[] vertices = new Vector2[vertexCount];
		
		for (int i = 0; i < vertexCount; i++)
		{
			float angle = Mathf.Tau * (i / (float)vertexCount);
			float radius = _rng.RandfRange(20, 50);

			vertices[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		}

		_polygon.Polygon = vertices;
		_collisionPolygon.Polygon = vertices;
	}

	private void SetRandomColor()
	{
		_polygon.Color = new Color(_rng.Randf(), _rng.Randf(), _rng.Randf());
	}
	
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
