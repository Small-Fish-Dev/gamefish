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
		get => _equipState ?? EquipState.Initializing;
		set
		{
			// "Initializing"
			if ( value is EquipState.Initializing )
				return;

			if ( _equipState.HasValue )
				if ( _equipState.Value == value )
					return;

			_equipState = value;
			OnEquipStateChanged( in value );
		}
	}

	protected EquipState? _equipState;

	[Property, ReadOnly, JsonIgnore]
	[Feature( EQUIP ), Group( DEBUG )]
	public EquipInventory Inventory => Pawn?.Equipment;

	protected virtual void OnEquipStateChanged( in EquipState state )
	{
		if ( !InGame )
			return;

		// this.Log( $"DEBUG: {this}.EquipState = {state}" );

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

		UpdateVisibility( in state );
	}

	protected virtual void UpdateVisibility( in EquipState state )
	{
		switch ( state )
		{
			case EquipState.Dropped:
				SetViewRendererVisibility( false );
				SetWorldRendererVisibility( true );
				break;

			case EquipState.Deployed:
				var isOwner = this.IsOwner();
				SetViewRendererVisibility( isOwner );
				SetWorldRendererVisibility( !isOwner );
				break;

			case EquipState.Holstered:
				SetViewRendererVisibility( false );
				SetWorldRendererVisibility( false );
				break;
		}
	}

	protected virtual void SetViewRendererVisibility( in bool isVisible )
	{
		var r = ViewRenderer?.ModelRenderer;

		if ( r.IsValid() )
			r.Enabled = isVisible;
	}

	protected virtual void SetWorldRendererVisibility( in bool isVisible )
	{
		if ( WorldRenderer.IsValid() )
			WorldRenderer.Enabled = isVisible;
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
