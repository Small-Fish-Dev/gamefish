namespace GameFish;

partial class BaseEquip
{
	public EquipState EquipState
	{
		get => _equipState;
		set
		{
			if ( _equipState == value )
				return;

			_equipState = value;

			if ( Scene.IsValid() && !Scene.IsEditor )
				OnEquipStateChanged( _equipState );
		}
	}

	protected EquipState _equipState;

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

	public void SetVisibility( bool viewModel, bool worldModel = false )
	{
		var r = ViewRenderer?.ModelRenderer;

		if ( r.IsValid() )
			r.Enabled = viewModel;

		if ( WorldRenderer.IsValid() )
			WorldRenderer.Enabled = worldModel;
	}

	protected virtual void OnEquipStateChanged( EquipState state )
	{
		// Log.Info( $"DEBUG: {this}.EquipState = {state}" );

		switch ( EquipState )
		{
			case EquipState.Dropped:
				break;
			case EquipState.Deployed:
				break;
			case EquipState.Holstered:
				break;
		}
	}

	public virtual void OnEquip( BasePawn owner, PawnEquipment inv )
	{
		UpdateNetworking( inv?.Pawn?.Agent?.Connection );
	}

	public virtual void OnDrop( BasePawn owner, PawnEquipment inv )
	{
	}

	public virtual void OnDeploy( BasePawn owner, PawnEquipment inv )
	{
	}

	public virtual void OnHolster( BasePawn owner, PawnEquipment inv )
	{
	}
}
