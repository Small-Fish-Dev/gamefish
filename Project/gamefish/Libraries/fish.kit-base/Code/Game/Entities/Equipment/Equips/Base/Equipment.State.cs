using System.Text.Json.Serialization;

namespace GameFish;

partial class Equipment
{
	[Property, ReadOnly, JsonIgnore]
	[Feature( EQUIP ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	public bool IsDeployed => this.IsValid() && EquipState == EquipState.Deployed;

	[Title( "Equip State" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( EQUIP ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected EquipState DebugEquipState => EquipState;

	[Sync]
	public EquipState EquipState
	{
		get => _equipState;
		set
		{
			if ( _equipState == value )
				return;

			_equipState = value;

			if ( this.InGame() )
				OnEquipStateChanged( _equipState );
		}
	}

	protected EquipState _equipState;

	[Property, ReadOnly, JsonIgnore]
	[Feature( EQUIP ), Group( DEBUG )]
	public PawnEquipment Inventory => Pawn?.Equipment;

	protected virtual void OnEquipStateChanged( EquipState state )
	{
		if ( this.InEditor() || !GameObject.IsValid() )
			return;

		// Log.Info( $"DEBUG: {this}.EquipState = {state}" );

		switch ( EquipState )
		{
			case EquipState.Dropped:
				OnDrop();
				break;
			case EquipState.Deployed:
				OnDeploy();
				break;
			case EquipState.Holstered:
				OnHolster();
				break;
		}
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();

		switch ( EquipState )
		{
			case EquipState.Dropped:
				SetVisibility( viewModel: false, worldModel: true );
				break;
			case EquipState.Deployed:
				SetVisibility( viewModel: true, worldModel: false );
				break;
			case EquipState.Holstered:
				SetVisibility( viewModel: false, worldModel: false );
				break;
		}
	}

	protected void SetVisibility( bool viewModel, bool worldModel = false )
	{
		var r = ViewRenderer?.ModelRenderer;

		if ( r.IsValid() )
			r.Enabled = viewModel;

		if ( WorldRenderer.IsValid() )
			WorldRenderer.Enabled = worldModel;
	}

	public virtual bool CanDeploy( Equipment from = null )
		=> true;

	public virtual bool TryDeploy( Equipment from = null )
	{
		if ( !CanDeploy( from: from ) )
			return false;

		EquipState = EquipState.Deployed;
		return true;
	}

	public virtual bool CanHolster( Equipment to = null )
		=> true;

	public virtual bool TryHolster( Equipment to = null )
	{
		if ( !CanHolster( to: to ) )
			return false;

		EquipState = EquipState.Holstered;
		return true;
	}

	protected virtual void OnEquip( BasePawn owner )
	{
		if ( IsProxy )
			return;

		if ( Inventory.IsValid() )
			if ( !Inventory.ActiveEquip.IsValid() )
				Inventory.TryDeploy( this );

		OnModuleEvent( e => e.OnEquip( owner ) );
	}

	protected virtual void OnDrop()
	{
		if ( IsProxy )
			return;

		OnModuleEvent( e => e.OnDrop() );
	}

	protected virtual void OnDeploy()
	{
		if ( IsProxy )
			return;

		OnModuleEvent( e => e.OnDeploy() );
	}

	protected virtual void OnHolster()
	{
		if ( IsProxy )
			return;

		OnModuleEvent( e => e.OnHolster() );
	}
}
