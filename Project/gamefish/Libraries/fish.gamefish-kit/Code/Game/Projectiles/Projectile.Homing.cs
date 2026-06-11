using System;

namespace GameFish;

partial class Projectile
{
	[Flags]
	public enum HomingMode
	{
		[Hide]
		None = 0,

		/// <summary>
		/// Influences the direction of velocity without affecting its speed.
		/// </summary>
		[Icon( "♻" )]
		Redirect = 1 << 0,

		/// <summary>
		/// Adds velocity towards the direction of the target.
		/// </summary>
		[Icon( "🌎" )]
		Gravitate = 1 << 1,
	}

	[Property]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( IsHoming ), Label = HOMING )]
	public bool IsHoming { get; set; } = false;

	/// <summary>
	/// Should velocity and such be factored for the target's position?
	/// </summary>
	[Property]
	[Title( "Prediction" )]
	[ToggleGroup( nameof( IsHoming ) )]
	[Feature( PROJECTILE ), Order( PROJECTILE_ORDER )]
	public bool HomingPrediction { get; set; } = true;

	/// <summary>
	/// The types of homing currently enabled.
	/// </summary>
	[Property]
	[Title( "Modes" )]
	[ToggleGroup( nameof( IsHoming ) )]
	[Feature( PROJECTILE ), Order( PROJECTILE_ORDER )]
	public HomingMode HomingModes { get; set; }

	public virtual bool IsHomingRedirect => HomingModes.HasFlag( HomingMode.Redirect );
	public virtual bool IsHomingGravitate => HomingModes.HasFlag( HomingMode.Gravitate );

	/// <summary>
	/// Angles the velocity towards the target.
	/// <br /> <br />
	/// <b> TIP: </b> Basically steers it. Not very realistic.
	/// <br /> <br />
	/// <c>x</c> = distance <br />
	/// <c>y</c> = speed
	/// </summary>
	[Property]
	[Title( "Redirection" )]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( IsHoming ) )]
	[ShowIf( nameof( IsHomingRedirect ), true )]
	public Curve HomingRedirection { get; set; } = new Curve( new( 0f, 1f ), new( 1f, 0f ) )
	{
		TimeRange = new( 0f, 2000f ),
		ValueRange = new( 0f, 1000f )
	};

	/// <summary>
	/// Pulls the projectile towards the target.
	/// <br /> <br />
	/// <b> TIP: </b> Works kind of like a boomerang.
	/// <br /> <br />
	/// <c>x</c> = distance <br />
	/// <c>y</c> = speed
	/// </summary>
	[Property]
	[Title( "Gravitation" )]
	[ShowIf( nameof( IsHomingGravitate ), true )]
	[Feature( PROJECTILE ), ToggleGroup( nameof( IsHoming ) )]
	public Curve HomingGravitation { get; set; } = new Curve( new( 0f, 1f ), new( 1f, 0f ) )
	{
		TimeRange = new( 0f, 2000f ),
		ValueRange = new( 0f, 1000f )
	};

	protected virtual void DoHoming( in float deltaTime )
	{
		if ( !IsHoming )
			return;

		var maxDist = 0f;

		var isRedirect = IsHomingRedirect;
		var isGravitate = IsHomingGravitate;

		if ( isRedirect )
			maxDist = HomingRedirection.TimeRange.y;

		if ( isGravitate )
			maxDist = HomingGravitation.TimeRange.y;

		if ( maxDist <= 0 )
			return;

		// Find the nearest enemy.
		var projPos = Center;

		var (target, dist) = FindEnemiesWithin( projPos, maxDist )
			.Where( enemy => enemy.IsValid() && enemy.Active )
			.Select( enemy => (enemy, projPos.Distance( enemy.Center )) )
			.OrderBy( tuple => tuple.Item2 )
			.FirstOrDefault();

		if ( !target.IsValid() || dist > maxDist )
			return;

		var targetPos = target.Center;

		if ( HomingPrediction )
			targetPos += target.Velocity * deltaTime;

		var dir = Center.Direction( targetPos );

		var vel = Velocity;
		var speed = vel.Length;
		var speedLimit = ProjectileTargetSpeed ?? DefaultSpeed;

		if ( speed.AlmostEqual( speedLimit ) )
			speed = speedLimit;

		// Redirection
		if ( isRedirect )
		{
			var redirectSpeed = HomingRedirection.Evaluate( dist );
			var redirectVel = dir * redirectSpeed * deltaTime;

			vel = (vel + redirectVel).Normal * speed;
		}

		// Gravitation
		if ( isGravitate )
		{
			var gravitateSpeed = HomingGravitation.Evaluate( dist );
			var gravitateVel = dir * gravitateSpeed * deltaTime;

			vel += gravitateVel;
		}

		Velocity = vel.ClampLength( speedLimit );
	}

	protected virtual IEnumerable<Pawn> FindEnemiesWithin( in Vector3 origin, in float radius )
	{
		if ( !Scene.IsValid() || !Team.IsValid() )
			return [];

		var trSphere = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.Sphere( radius, origin, origin ).RunAll();

		var enemies = trSphere
			.Select( tr => Entity.TryGet<Pawn>( tr.GameObject, out var pawn ) ? pawn : null )
			.Where( pawn => pawn.IsValid() && pawn.IsAlive && Team.IsEnemy( pawn.Team ) )
			.Distinct();

		return enemies;
	}
}
