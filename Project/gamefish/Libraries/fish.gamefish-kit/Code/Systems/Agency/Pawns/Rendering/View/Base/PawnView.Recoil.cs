namespace GameFish;

partial class PawnView
{
	[Property]
	[Title( "Reset Speed" )]
	[Range( 1f, 50f, clamped: false )]
	[Feature( VIEW ), Group( RECOIL )]
	public virtual float RecoilResetSpeed { get; set; } = 15f;

	/// <summary>
	/// The current relative orientation. <br />
	/// Setting this automatically sets the transform.
	/// </summary>
	[Property]
	[Title( "Recoil (current)" )]
	[Feature( VIEW ), Group( RECOIL )]
	[ShowIf( nameof( InGame ), true )]
	protected Rotation InspectorRecoil
	{
		get => Recoil;
		set => Recoil = value;
	}

	[Sync( SyncFlags.Interpolate )]
	public Rotation Recoil
	{
		get => _recoil;
		set
		{
			if ( !ITransform.IsValid( value ) )
				return;

			_recoil = value;
			OnSetRecoil( in value );
		}
	}

	protected Rotation _recoil = Rotation.Identity;

	protected virtual void OnSetRecoil( in Rotation rRecoil ) { }

	protected virtual void UpdateRecoil( in float deltaTime )
	{
		if ( Recoil == Rotation.Identity )
			return;

		if ( Pawn.IsValid() )
			Pawn.EyeRotation *= Recoil * deltaTime;

		var speed = RecoilResetSpeed * deltaTime;

		Recoil *= Recoil.Inverse * speed;

		if ( Recoil.AlmostEqual( Rotation.Identity ) )
			Recoil = Rotation.Identity;
	}

	public virtual void AddRecoil( in Rotation rRecoil )
	{
		Recoil *= rRecoil;
	}

	public virtual void ResetRecoil()
	{
		Recoil = Rotation.Identity;
	}

	[Rpc.Owner( NetFlags.Reliable | NetFlags.SendImmediate | NetFlags.HostOnly )]
	public void RpcHostAddRecoil( Rotation rRecoil )
		=> AddRecoil( in rRecoil );

	[Rpc.Owner( NetFlags.Reliable | NetFlags.SendImmediate | NetFlags.HostOnly )]
	public void RpcHostResetRecoil()
		=> ResetRecoil();
}
