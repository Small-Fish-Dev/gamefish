using System.Text.Json.Serialization;

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

	[Property]
	[JsonIgnore, ReadOnly]
	[Title( "Is Grounded" )]
	[Feature( PAWN ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected bool InspectorIsGrounded => IsGrounded;

	[Property]
	[JsonIgnore, ReadOnly]
	[Title( "Ground Normal" )]
	[Feature( PAWN ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected Vector3 InspectorGroundNormal => GroundNormal;

	[Property]
	[JsonIgnore, ReadOnly]
	[Title( "Ground Object" )]
	[Feature( PAWN ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected GameObject InspectorGroundObject => GroundObject;

	[Property]
	[JsonIgnore, ReadOnly]
	[Title( "Ground Collider" )]
	[Feature( PAWN ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected Collider InspectorGroundCollider => GroundCollider;

	[Property]
	[JsonIgnore, ReadOnly]
	[Title( "Ground Body" )]
	[Feature( PAWN ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected Rigidbody InspectorGroundBody => GroundBody;

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
	public Rigidbody GroundBody
	{
		get => _groundBody;
		set
		{
			_groundBody = value;
			OnSetGroundBody( value );
		}
	}

	protected Rigidbody _groundBody;

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

	protected virtual void OnSetGroundObject( GameObject obj )
	{
		Controller?.OnSetGroundObject( obj );
	}

	protected virtual void OnSetGroundCollider( Collider c )
	{
		Controller?.OnSetGroundCollider( c );
	}

	protected virtual void OnSetGroundBody( Rigidbody rb )
	{
		Controller?.OnSetGroundBody( rb );
	}

	public virtual bool IsGround( in SceneTraceResult trGround )
	{
		if ( !trGround.Hit )
			return false;

		return IsGround( in trGround.Normal );
	}

	public virtual bool IsGround( in Vector3 normal )
	{
		if ( !GroundingEnabled )
			return false;

		var gravDir = Gravity.Normal;

		if ( normal.Angle( -gravDir ) > GroundAngle )
			return false;

		var upVel = Velocity.Forward( Up );
		var upSpeed = upVel.Dot( normal );

		return upSpeed < 300f;
	}

	protected virtual void ClearGround( ProjectedMovement move )
	{
		move.IsGrounded = false;
		move.GroundNormal = Up;

		move.GroundObject = null;
		move.GroundCollider = null;
		move.GroundBody = null;
	}
}
