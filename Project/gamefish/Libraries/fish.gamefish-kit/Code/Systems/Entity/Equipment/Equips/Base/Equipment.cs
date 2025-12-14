namespace GameFish;

/// <summary>
/// An equipment that <see cref="PawnEquipment"/> can store, deploy and use.
/// </summary>
[Icon( "plumbing" )]
[EditorHandle( Icon = "🏹" )]
public partial class Equipment : DynamicEntity
{
	protected const int EQUIP_ORDER = DEFAULT_ORDER - 71144;

	protected const string SLOT = "Slot";

	[Property]
	[Order( EQUIP_ORDER )]
	[Feature( EQUIP ), Group( SLOT )]
	[FilePath( Extension = FileExtensions.IMAGE )]
	public string Icon { get; set; }

	/// <summary>
	/// The slot this is meant to go in.
	/// </summary>
	[Property]
	[Order( EQUIP_ORDER )]
	[Title( "Default" )]
	[Feature( EQUIP ), Group( SLOT )]
	public EquipSlot DefaultSlot { get; set; }

	[Property]
	[Title( "Current" )]
	[Order( EQUIP_ORDER )]
	[Sync( SyncFlags.FromHost )]
	[Feature( EQUIP ), Group( SLOT )]
	[ShowIf( nameof( InGame ), true )]
	public int Slot { get; set; }

	public virtual Vector3 AimPosition => Pawn?.EyePosition ?? WorldPosition;
	public virtual Rotation AimRotation => Pawn?.EyeRotation ?? Rotation.Identity;
	public virtual Vector3 AimScale => Pawn?.WorldScale ?? Vector3.One;

	public Transform AimTransform => new( AimPosition, AimRotation, AimScale );
	public Vector3 AimDirection => AimRotation.Forward;

	public override string ToString()
		=> ClassId.IsBlank() || ClassName.IsBlank() ? base.ToString() : $"{ClassId}|{ClassName}";

	protected override void OnStart()
	{
		base.OnStart();

		Tags?.Add( TAG_EQUIP );
	}

	/// <returns> If a pawn can equip this. </returns>
	public virtual bool AllowEquip( Pawn pawn )
		=> pawn.IsValid() && pawn.HasModule<PawnEquipment>();

	/// <returns> If the owning pawn can press buttons to use this. </returns>
	public virtual bool AllowInput()
	{
		// Must be deployed and the explicit network owner.
		if ( !IsDeployed || !this.IsOwner() )
			return false;

		// Prevent simulating while dead or not actually held.
		if ( !Pawn.IsValid() || !Pawn.IsAlive )
			return false;

		// Pawn must belong to the local client.
		return Pawn.AllowInput();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !this.InGame() )
			return;

		if ( AllowInput() )
			Simulate( Time.Delta );
	}

	protected virtual void Simulate( in float deltaTime )
	{
		SimulateModules( in deltaTime );
	}

	/// <returns> The current angles(in degrees) of spread applied to bullets and such. </returns>
	public virtual Vector2 GetCurrentSpread( in Vector2 baseSpread, EquipFunction func )
		=> baseSpread;
}
