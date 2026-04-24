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
	public EquipState EquipState { get; set; }
	private EquipState currentEquipState;

	[Property, ReadOnly, JsonIgnore]
	[Feature( EQUIP ), Group( DEBUG )]
	public EquipInventory Inventory => Pawn?.Equipment;

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

	protected virtual void UpdateState()
	{
		if ( currentEquipState != EquipState )
		{
			currentEquipState = EquipState;
			OnEquipStateChanged( currentEquipState );
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

	protected virtual void OnEquip( Pawn owner )
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
		DropEffects();	

		if ( IsProxy )
			return;


		OnModuleEvent( e => e.OnDrop() );
	}

	protected virtual void DropEffects()
	{
		SetVisibility( false, true );
	}

	protected virtual void OnDeploy()
	{
		DeployEffects();
		if ( IsProxy )
		{
			return;
		}

		OnModuleEvent( e => e.OnDeploy() );
	}

	protected virtual void DeployEffects()
	{
		if ( IsProxy )
		{
			SetVisibility( false, true );
		}
		else
		{
			SetVisibility( true, false );
		}
	}

	protected virtual void OnHolster()
	{
		HolsterEffects();

		if ( IsProxy )
			return;

		OnModuleEvent( e => e.OnHolster() );
	}

	protected virtual void HolsterEffects()
	{
		SetVisibility( false, false );
	}
}
