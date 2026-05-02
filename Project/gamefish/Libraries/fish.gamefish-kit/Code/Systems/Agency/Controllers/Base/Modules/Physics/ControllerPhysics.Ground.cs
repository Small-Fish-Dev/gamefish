namespace GameFish;

partial class ControllerPhysics
{
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
}
