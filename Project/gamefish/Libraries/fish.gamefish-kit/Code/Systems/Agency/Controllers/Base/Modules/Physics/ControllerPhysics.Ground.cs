namespace GameFish;

partial class ControllerPhysics
{
	/// <summary> Stick to floors? Also prevents slipping down them. </summary>
	[Property]
	[Range( 0.01f, 5f, clamped: false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public virtual bool GroundingEnabled { get; set; } = true;

	/// <summary> The angle in which a surface is considered ground. </summary>
	[Property]
	[Range( 0f, 90f, clamped: false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public virtual float GroundAngle { get; set; } = 45f;

	/// <summary> The maximum distance to stick to ground. </summary>
	[Property]
	[Range( 0f, 32f, clamped: false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public virtual float GroundDistance { get; set; } = 16f;

	[Sync]
	public bool IsGrounded
	{
		get => _isGrounded;
		set
		{
			if ( _isGrounded == value )
				return;

			_isGrounded = value;
			OnSetIsGrounded( in value );
		}
	}

	protected bool _isGrounded;

	[Sync]
	public Vector3 GroundNormal
	{
		get => _groundNormal;
		set
		{
			_groundNormal = value;
			OnSetGroundNormal( in value );
		}
	}

	protected Vector3 _groundNormal = Vector3.Up;

	[Sync]
	public Collider GroundCollider
	{
		get => _groundCollider;
		set
		{
			_groundCollider = value;
			OnSetGroundCollider( value );
		}
	}

	protected Collider _groundCollider;

	[Sync]
	public GameObject GroundObject
	{
		get => _groundObject;
		set
		{
			_groundObject = value;
			OnSetGroundObject( value );
		}
	}

	protected GameObject _groundObject;

	/// <summary>
	/// Called when <see cref="IsGrounded"/> is toggled.
	/// </summary>
	protected virtual void OnSetIsGrounded( in bool isGrounded )
	{
		Controller?.OnSetIsGrounded( in isGrounded );

		if ( !isGrounded )
		{
			GroundCollider = null;
			GroundObject = null;
		}
	}

	protected virtual void OnSetGroundNormal( in Vector3 vNormal )
	{
		Controller?.OnSetGroundNormal( in vNormal );
	}

	protected virtual void OnSetGroundCollider( Collider c )
	{
		Controller?.OnSetGroundCollider( c );
	}

	protected virtual void OnSetGroundObject( GameObject obj )
	{
		Controller?.OnSetGroundObject( obj );
	}

	public bool IsGround( in Vector3 normal )
	{
		if ( !GroundingEnabled )
			return false;

		if ( Up.Angle( normal ) > GroundAngle )
			return false;

		var upVel = Velocity.Forward( Up );
		var upSpeed = upVel.Dot( normal );

		return upSpeed < 30f;
	}
}
