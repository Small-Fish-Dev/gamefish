using System;

namespace GameFish;

partial class Projectile
{
	/// <summary>
	/// The speed to go if not otherwise specified(like by an equipment).
	/// </summary>
	[Property]
	[Feature( PROJECTILE ), Group( MOVEMENT )]
	[Range( 0f, 10000f, clamped: false ), Step( 1 )]
	public float DefaultSpeed { get; set; } = 1000f;

	/// <summary>
	/// Should the object be rotated towards the direction it's moving?
	/// </summary>
	[Property]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( RotateTowardsVelocity ), Label = ROTATION )]
	public bool RotateTowardsVelocity { get; set; } = false;

	/// <summary>
	/// Offsets the final rotation from where we're moving to.
	/// </summary>
	[Property]
	[Title( "Offset" )]
	[Feature( PROJECTILE ), ToggleGroup( nameof( RotateTowardsVelocity ) )]
	public Rotation RotationOffset { get; set; } = Rotation.Identity;

	public override Vector3 Velocity
	{
		get => ProjectileVelocity;
		set => ProjectileVelocity = value;
	}

	[Sync]
	protected Vector3 ProjectileVelocity { get; set; }

	/// <summary>
	/// Overrides what speed the projectile should be moving.
	/// </summary>
	[Sync]
	public float? ProjectileTargetSpeed { get; set; }

	protected virtual void UpdateVelocity( in float deltaTime )
	{
		// Homing missiles.
		DoHoming( in deltaTime );
	}

	protected override void Move( in float deltaTime, in bool isFixedUpdate )
	{
		var startPos = WorldPosition;
		var move = Velocity * deltaTime;

		if ( TryCollide( startPos, startPos + move, out _ ) )
			return;

		if ( !GameObject.IsValid() || IsProxy )
			return;

		WorldPosition += move;

		// Look towards where we're moving to?
		if ( RotateTowardsVelocity )
		{
			var rVel = Rotation.LookAt( Velocity, WorldRotation.Up );

			if ( RotationOffset != default )
				rVel *= RotationOffset;

			WorldRotation = rVel;
		}
	}

	protected virtual bool TryCollide( Vector3 start, Vector3 end, out IEnumerable<SceneTraceResult> trAll )
	{
		try
		{
			trAll = TraceSettings.RunAll( GameObject, start, end );
		}
		catch ( Exception e )
		{
			trAll = null;

			if ( DebugLogging )
				this.Warn( e );
		}

		if ( trAll is not null )
		{
			foreach ( var tr in trAll )
			{
				if ( !IsCollision( in tr ) )
					continue;

				if ( TryCollide( new ImpactData( GameObject, tr ) ) )
					if ( IsFinished() )
						return true;
			}
		}

		return false;
	}
}
