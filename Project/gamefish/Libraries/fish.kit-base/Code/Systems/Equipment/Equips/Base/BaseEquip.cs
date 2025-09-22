namespace GameFish;

/// <summary>
/// An <see cref="PawnEquipment"/> can store, deploy and use this.
/// </summary>
public abstract partial class BaseEquip : PhysicsEntity, ISkinned
{
	public const string MELEE = "🗡 Melee";
	public const string WEAPON = "🔫 Weapon";

	public const string GROUP_SLOT = "Slot";
	public const string GROUP_METHOD = "Input Method";
	public const string GROUP_TIMING = "Timing";
	public const string GROUP_MODELS = "Models";
	public const string GROUP_RANGE = "Range";

	/// <summary>
	/// Identifies this class of equipment. <br />
	/// Used for spawning purposes(such as with commands).
	/// </summary>
	[Property]
	[Feature( EQUIP )]
	public string ClassId
	{
		get => string.IsNullOrWhiteSpace( _id ) ? _id = GameObject.Name : _id;
		set => _id = value;
	}

	protected string _id;

	/// <summary> The name of the equipment to display. </summary>
	[Property]
	[Feature( EQUIP )]
	public virtual string Name { get; set; }

	[Property]
	[Feature( EQUIP ), Group( GROUP_MODELS )]
	public Model ViewModel { get; set; }

	[Property]
	[Feature( EQUIP ), Group( GROUP_MODELS )]
	public Model WorldModel { get => WorldRenderer?.Model; set { if ( WorldRenderer.IsValid() ) WorldRenderer.Model = value; } }

	[Property]
	[Feature( EQUIP ), Group( GROUP_MODELS )]
	public SkinnedModelRenderer WorldRenderer
	{
		// Auto-cache the component.
		get => _wr.IsValid() ? _wr
			: _wr = Components?.Get<SkinnedModelRenderer>( FindMode.EverythingInDescendants );

		set { _wr = value; }
	}

	protected SkinnedModelRenderer _wr;

	public SkinnedModelRenderer SkinRenderer { get => WorldRenderer; set => _wr = value; }

	[Property]
	[Feature( EQUIP ), Group( DEBUG )]
	public BasePawn Owner
	{
		get
		{
			if ( !this.IsValid() || EquipState is EquipState.Dropped )
				return null;

			return _owner.IsValid() ? _owner
				: _owner = Components.Get<BasePawn>( FindMode.EnabledInSelf | FindMode.InAncestors );
		}

		set => _owner = value;
	}

	protected BasePawn _owner;

	[Property]
	[Feature( EQUIP ), Group( DEBUG )]
	public PawnEquipment Inventory
		=> Owner?.GetModule<PawnEquipment>();

	/// <summary>
	/// The slot this is meant to go in.
	/// </summary>
	[Property]
	[Title( "Default" )]
	[Feature( EQUIP ), Group( GROUP_SLOT )]
	public int DefaultSlot { get; set; }

	[Sync]
	[Property]
	[Title( "Current" )]
	[Feature( EQUIP ), Group( GROUP_SLOT )]
	[ShowIf( nameof( PlayingScene ), true )]
	public int Slot { get; set; }

	public bool IsDeployed => this.IsValid() && EquipState == EquipState.Deployed && Inventory?.ActiveEquip == this;

	[Property, Feature( INPUT ), Group( GROUP_METHOD )]
	public virtual bool PrimaryHeld { get; set; }
	public virtual bool PrimaryInput => PrimaryHeld ? Input.Down( PrimaryAction ) : Input.Pressed( PrimaryAction );

	[Property, InputAction]
	[Feature( INPUT ), Group( GROUP_METHOD )]
	public string PrimaryAction { get; set; } = "Attack1";

	[Property, Feature( INPUT ), Group( GROUP_METHOD )]
	public virtual bool SecondaryHeld { get; set; }
	public virtual bool SecondaryInput => SecondaryHeld ? Input.Down( SecondaryAction ) : Input.Pressed( SecondaryAction );

	[Property, InputAction]
	[Feature( INPUT ), Group( GROUP_METHOD )]
	public string SecondaryAction { get; set; } = "Attack2";

	[Property, Feature( INPUT ), Group( GROUP_TIMING )]
	public virtual float PrimaryCooldown { get; set; } = 0.5f;
	public virtual TimeSince? LastPrimary { get; set; }

	[Property, Feature( INPUT ), Group( GROUP_TIMING )]
	public virtual float SecondaryCooldown { get; set; } = 1.0f;
	public virtual TimeSince? LastSecondary { get; set; }

	public virtual Vector3 AimPosition => Owner?.EyePosition ?? WorldPosition;
	public virtual Vector3 AimDirection => Owner?.EyeForward ?? WorldRotation.Forward;
	public Transform AimTransform => new( AimPosition, Rotation.LookAt( AimDirection ) );

	public override string ToString()
		=> $"{ClassId}|{Name}";

	protected override void OnStart()
	{
		Tags?.Add( TAG_EQUIP );

		base.OnStart();
	}

	public virtual bool AllowInput()
	{
		if ( !IsDeployed )
			return false;

		var owner = Owner;

		if ( !owner.IsValid() || !owner.IsAlive )
			return false;

		return owner.AllowInput();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !AllowInput() )
			return;

		if ( PrimaryInput )
			TryPrimary();

		if ( SecondaryInput )
			TrySecondary();
	}

	/// <returns> If a pawn can equip this. </returns>
	public virtual bool AllowEquip( BasePawn pawn )
	{
		return pawn.IsValid() && pawn.HasModule<PawnEquipment>();
	}

	public virtual bool CanPrimary()
		=> !LastPrimary.HasValue || LastPrimary >= PrimaryCooldown;

	public virtual bool CanSecondary()
		=> !LastSecondary.HasValue || LastSecondary >= SecondaryCooldown;

	public virtual bool TryPrimary()
	{
		if ( CanPrimary() )
		{
			LastPrimary = 0;
			OnPrimary();

			return true;
		}

		return false;
	}

	/// <summary>
	/// Executes the primary function of this equipment. <br />
	/// You should call <see cref="TryPrimary"/> to call this while respecting cooldowns.
	/// </summary>
	public virtual void OnPrimary()
	{
	}

	public virtual bool TrySecondary()
	{
		if ( CanSecondary() )
		{
			LastSecondary = 0;
			OnSecondary();

			return true;
		}

		return false;
	}

	/// <summary>
	/// Executes the secondary function of this equipment. <br />
	/// You should call <see cref="TrySecondary"/> to call this while respecting cooldowns.
	/// </summary>
	public virtual void OnSecondary()
	{
	}
}
