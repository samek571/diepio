namespace diep;
using Godot;

public partial class Target : RigidBody2D
{

	private int _currentHP;
	private int _xpValue;

	[Export]
	public float CollisionBorderScale = 1.2f;

	private Polygon2D _polygon;
	private CollisionPolygon2D _collisionPolygon;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();


	public override void _Ready()
	{
		_polygon = GetNode<Polygon2D>("Polygon2D");
		_collisionPolygon = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
		
		GenerateRandomPolygon();
		SetRandomColor();
		CalculateHPBasedOnColor();
		CalculateXPBasedOnVertices();
	}
	
	// i couldnt really think of anything better than doing circle and random value +- from the centre, works
	// surely i didnt want to do a bitmap as this is a simple task
	private void GenerateRandomPolygon()
	{
		_rng.Randomize();

		int vertexCount = _rng.RandiRange(3, 12);
		Vector2[] vertices = new Vector2[vertexCount];
		Vector2[] collisionVertices = new Vector2[vertexCount];
		
		for (int i = 0; i < vertexCount; i++)
		{
			float angle = Mathf.Tau * (i / (float)vertexCount);
			float radius = _rng.RandfRange(20, 50);

			vertices[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
			collisionVertices[i] = vertices[i] * CollisionBorderScale;
		}

		_polygon.Polygon = vertices;
		_collisionPolygon.Polygon = collisionVertices;
	}

	private void SetRandomColor()
	{
		_polygon.Color = new Color(_rng.Randf(), _rng.Randf(), _rng.Randf());
	}
	
	private void CalculateHPBasedOnColor()
	{
		Color color = _polygon.Color;

		// we should normalize the RGB values to 0-1 range but they alreadycome in that format
		// exponential decay 
		float k = 5.0f; 
		float contributionR = Mathf.Exp(-k * color.R);
		float contributionG = Mathf.Exp(-k * color.G);
		float contributionB = Mathf.Exp(-k * color.B);

		float logContribution = Mathf.Log(1 + contributionR + contributionG + contributionB);

		int baseHP = 100;
		int scaleFactor = 200;
		float threshold = 0.4f;
		int bonusHP = 0;
		if (color.R < threshold && color.G < threshold && color.B < threshold)
		{
			bonusHP = 50;
		}

		int maxHP = baseHP + (int)(scaleFactor * logContribution) + bonusHP;
		_currentHP = maxHP;
		//GD.Print($"Target Color: R: {color.R}, G: {color.G}, B: {color.B} => MaxHP: {maxHP}");
	}
	
	private void CalculateXPBasedOnVertices()
	{
		int vertexCount = _polygon.Polygon.Length;
		float randomRange = 10.0f;
		float randomVariation = randomRange * (GD.Randf() - 0.5f);
		float baseXP = 50f; 
		float scaleFactor = 20.0f;

		float logarithmicXP = baseXP + (scaleFactor * Mathf.Log(vertexCount + 1));

		_xpValue = (int)(logarithmicXP + randomVariation);

		if (vertexCount > 10)
		{
			_xpValue += 10;
		}

		//GD.Print($"Target with {vertexCount} vertices => XP Value: {_xpValue}");
	}
	private void CalculateMassBasedOnSize()
	{
		float area = CalculatePolygonArea(_polygon.Polygon);
		Mass = area * 0.1f;
	}
	
	public void TakeDamage(int damage)
	{
		_currentHP -= damage;

		if (_currentHP <= 0)
		{
			GiveXPToPlayer();
			Die();
		}
	}
	
	private void GiveXPToPlayer()
	{
		var player = GetTree().Root.GetNode<Player>("RigidBody2D");
		if (player != null)
		{
			player.AddXP(_xpValue);
		}
	}
	//little trick from linear algebra and pokrocile cvika at school
	private float CalculatePolygonArea(Vector2[] vertices)
	{
		float area = 0;
		int n = vertices.Length;

		for (int i = 0; i < n; i++)
		{
			Vector2 current = vertices[i];
			Vector2 next = vertices[(i + 1) % n];

			area += current.X * next.Y - next.X * current.Y;
		}

		area = Mathf.Abs(area) / 2.0f;
		return area;
	}

	private void Die()
	{
		QueueFree();
	}
	
	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		state.LinearVelocity = Vector2.Zero;
	}
}
