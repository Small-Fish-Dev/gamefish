namespace GameFish;

/// <summary>
/// An <see cref="PawnEquipment"/> can store, deploy and use this.
/// </summary>
public abstract partial class BaseEquip : PhysicsEntity, ISkinned
{
	public const string TAG = "equip";

	public const string EQUIP = "🏹 Equip";

	public const string FEATURE_INPUT = "🕹 Input";
	public const string FEATURE_WEAPON = "🔫 Weapon";
	public const string FEATURE_VISUALS = "🎨 Visuals";

	public const string GROUP_SLOT = "Slot";
	public const string GROUP_METHOD = "Input Method";
	public const string GROUP_TIMING = "Timing";
	public const string GROUP_MODELS = "Models";
	public const string GROUP_RANGE = "Range";

	/// <summary> Identifies this class of equipment. </summary>
	[Property]
	[Feature( EQUIP )]
	public string ID
	{
		get => string.IsNullOrWhiteSpace( _id ) ? _id = GameObject.Name : _id;
		set => _id = value;
	}

	protected string _id;

	/// <summary> The name of the equipment to display. </summary>
	[Property]
	[Feature( EQUIP )]
	public virtual string Name { get; }

	[Property]
	[Feature( FEATURE_VISUALS ), Group( GROUP_MODELS )]
	public Model ViewModel { get; set; }

	[Property, ReadOnly]
	[Feature( FEATURE_VISUALS ), Group( GROUP_MODELS )]
	public ViewModel ViewComponent => Owner?.ViewModel;

	[Property]
	[Feature( FEATURE_VISUALS ), Group( GROUP_MODELS )]
	public Model WorldModel { get => WorldRenderer?.Model; set { if ( WorldRenderer.IsValid() ) WorldRenderer.Model = value; } }

	[Property]
	[Feature( FEATURE_VISUALS ), Group( GROUP_MODELS )]
	public SkinnedModelRenderer WorldRenderer
	{
		// Auto-cache the component.
		get => _wr.IsValid() ? _wr
			: _wr = Components?.Get<SkinnedModelRenderer>( FindMode.EverythingInDescendants );

		set { _wr = value; }
	}

	protected SkinnedModelRenderer _wr;
	public SkinnedModelRenderer SkinRenderer { get => WorldRenderer; set => WorldRenderer = value; }

	[Property]
	[Feature( EQUIP ), Group( DEBUG )]
	public BasePawn Owner
	{
		get
		{
			if ( EquipState is EquipState.Dropped )
				return null;

			if ( !_owner.IsValid() && this.IsValid() )
				_owner = Components.Get<BasePawn>( FindMode.EnabledInSelf | FindMode.InAncestors );

			return _owner;
		}

		set => _owner = value;
	}

	protected BasePawn _owner;

	[Property]
	[Feature( EQUIP ), Group( DEBUG )]
	public PawnEquipment Inventory
	{
		get => _inv.IsValid() ? _inv
			: _inv = Owner?.Modules.GetModule<PawnEquipment>();

		set => _inv = value;
	}

	protected PawnEquipment _inv;

	/// <summary>
	/// The slot this is goes in(if enforced).
	/// </summary>
	[Property]
	[Title( "Default" )]
	[Feature( EQUIP ), Group( GROUP_SLOT )]
	public int DefaultSlot { get; set; }

	[Sync]
	[Property, ReadOnly]
	[Title( "Current" )]
	[Feature( EQUIP ), Group( GROUP_SLOT )]
	public int Slot { get; set; }

	public bool IsDeployed => this.IsValid() && EquipState == EquipState.Deployed && Inventory?.ActiveEquip == this;

	[Property, Feature( FEATURE_INPUT ), Group( GROUP_METHOD )]
	public virtual bool PrimaryHeld { get; set; }
	public virtual bool PrimaryInput => PrimaryHeld ? Input.Down( PrimaryAction ) : Input.Pressed( PrimaryAction );

	[Property, InputAction]
	[Feature( FEATURE_INPUT ), Group( GROUP_METHOD )]
	public string PrimaryAction { get; set; } = "Attack1";

	[Property, Feature( FEATURE_INPUT ), Group( GROUP_METHOD )]
	public virtual bool SecondaryHeld { get; set; }
	public virtual bool SecondaryInput => SecondaryHeld ? Input.Down( SecondaryAction ) : Input.Pressed( SecondaryAction );

	[Property, InputAction]
	[Feature( FEATURE_INPUT ), Group( GROUP_METHOD )]
	public string SecondaryAction { get; set; } = "Attack2";

	[Property, Feature( FEATURE_INPUT ), Group( GROUP_TIMING )]
	public virtual float PrimaryCooldown { get; set; } = 0.5f;
	public virtual TimeSince? LastPrimary { get; set; }

	[Property, Feature( FEATURE_INPUT ), Group( GROUP_TIMING )]
	public virtual float SecondaryCooldown { get; set; } = 1.0f;
	public virtual TimeSince? LastSecondary { get; set; }

	public virtual Vector3 AimPosition => Owner?.EyePosition ?? WorldPosition;
	public virtual Vector3 AimDirection => Owner?.EyeForward ?? WorldRotation.Forward;
	public Transform AimTransform => new( AimPosition, Rotation.LookAt( AimDirection ) );

	public override string ToString()
		=> Name ?? base.ToString();

	protected override void OnStart()
	{
		base.OnStart();

		Tags?.Add( TAG );
	}

	public virtual void UpdateNetworking()
	{
		if ( !Networking.IsHost )
			return;

		var cn = Owner?.Agent?.Connection;
		GameObject.NetworkSetup( cn, NetworkOrphaned.ClearOwner, OwnerTransfer.Fixed, NetworkMode.Object, ignoreProxy: false );
	}

	public virtual bool AllowInput()
	{
		if ( EquipState is not EquipState.Deployed )
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

		if ( !IsDeployed )
			return;

		if ( PrimaryInput && CanPrimary() )
			TryPrimary();

		if ( SecondaryInput && CanSecondary() )
			TrySecondary();
	}

	/// <returns> If a pawn can equip this. </returns>
	public virtual bool AllowEquip( BasePawn pawn )
	{
		return pawn.IsValid() && pawn.Modules.HasModule<PawnEquipment>();
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
