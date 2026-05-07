namespace GameFish;

partial class FirstPersonController
{
	/// <summary>
	/// Should this be able to crouch?
	/// </summary>
	[Property]
	[Feature( PAWN ), Order( DUCKING_ORDER )]
	[ToggleGroup( nameof( DuckingEnabled ), Label = DUCKING )]
	public bool DuckingEnabled { get; set; } = true;

	[Property]
	[InputAction]
	[Title( "Input" )]
	[Feature( PAWN ), Order( DUCKING_ORDER )]
	[ToggleGroup( nameof( DuckingEnabled ) )]
	public string DuckInput { get; set; } = "Duck";

	[Property]
	[Title( "Move Speed" )]
	[Range( 0f, 1000f, clamped: false )]
	[Feature( PAWN ), Order( DUCKING_ORDER )]
	[ToggleGroup( nameof( DuckingEnabled ) )]
	public virtual float MoveSpeedDucked { get; set; } = 120f;

	[Sync]
	public bool IsDucking
	{
		get => _isDucking;
		set
		{
			if ( _isDucking == value )
				return;

			_isDucking = value;
			OnSetIsDucking( value );
		}
	}

	protected bool _isDucking = false;

	protected virtual void OnSetIsDucking( in bool isDucking )
	{
	}

	/// <returns> If ducking is currently intended. </returns>
	protected virtual bool IsWishingDuck()
		=> Input.Down( DuckInput );

	/// <returns> If ducking should be active. </returns>
	protected virtual bool ShouldDuck()
	{
		if ( !DuckingEnabled )
			return false;

		return IsWishingDuck();
	}
}
