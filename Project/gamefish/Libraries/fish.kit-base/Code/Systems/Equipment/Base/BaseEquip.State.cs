namespace GameFish;

partial class BaseEquip
{
	public EquipState EquipState
	{
		get => _state;
		set
		{
			if ( _state == value )
				return;

			_state = value;

			if ( Scene.IsValid() && !Scene.IsEditor )
				OnEquipStateChanged( _state );
		}
	}
	private EquipState _state;

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
		var vm = ViewComponent;

		if ( vm.IsValid() )
			vm.Enabled = viewModel;

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

	public virtual void OnEquip( PawnEquipment inv )
	{
	}

	public virtual void OnDrop( PawnEquipment inv )
	{
	}

	public virtual void OnDeploy( PawnEquipment inv )
	{
	}

	public virtual void OnHolster( PawnEquipment inv )
	{
	}
}
