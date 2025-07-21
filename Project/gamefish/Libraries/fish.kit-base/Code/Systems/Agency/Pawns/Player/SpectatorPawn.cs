using System;

namespace GameFish;

/// <summary>
/// A basic spectator pawn with three modes to choose from (<see cref="SpectatorModes"/>).
/// </summary>
public partial class SpectatorPawn : ControllerPawn
{
	protected const string FEATURE_SPECTATOR = "👻 Spectator";

	public enum SpectatorModes
	{
		/// <summary>
		/// Free flight/Noclip mode
		/// </summary>
		[Icon( "flight" )] FreeRoam = 0,
		/// <summary>
		/// First person view from the target's perspective, with optional view model visibility
		/// </summary>
		[Icon( "face" )] FirstPerson,
		/// <summary>
		/// Third person view, centered around the target
		/// </summary>
		[Icon( "videocam" )] ThirdPerson
	}

	[Sync]
	[Property]
	[Feature( FEATURE_SPECTATOR )]
	public PlayerPawn Target { get; set; }

	[Sync]
	[Property]
	[Feature( FEATURE_SPECTATOR )]
	public SpectatorModes Mode { get; set; }

	/// <summary>
	/// Requirements for a pawn to be observable.
	/// For example, you can let the players spectate only their teammates.
	/// </summary>
	[Property]
	[Feature( FEATURE_SPECTATOR )]
	public Func<BasePlayer, bool> Filter { get; set; }

	private bool IsAnythingEnabled => HasFirstPersonMode || HasThirdPersonMode || HasFreeRoamMode;

	private bool IsModeEnabled( SpectatorModes mode ) => mode switch
	{
		SpectatorModes.FirstPerson => HasFirstPersonMode,
		SpectatorModes.ThirdPerson => HasThirdPersonMode,
		SpectatorModes.FreeRoam => HasFreeRoamMode,
		_ => throw new NotImplementedException( $"Cannot find a flag for mode {mode}" )
	};

	public List<SpectatorModes> GetAllowedModes()
		=> Enum.GetValues<SpectatorModes>()
			.Where( IsModeEnabled )
			.ToList();

	private bool InEditor => Scene?.IsEditor ?? true;

	public void CycleMode( int dir )
	{
		var modes = GetAllowedModes();

		if ( modes.Count <= 0 )
			return;

		var iMode = modes.IndexOf( Mode );
		var iNext = (iMode + dir) % modes.Count;

		Mode = modes.ElementAtOrDefault( iNext );
	}

	/// <summary>
	/// Choose the next mode available.
	/// </summary>
	[Button( "Next Mode" )]
	[Feature( FEATURE_SPECTATOR ), Group( "Debug" )]
	public void NextMode() => CycleMode( 1 );

	/// <summary>
	/// Choose the previous mode available.
	/// </summary>
	[Button( "Previous Mode" )]
	[Feature( FEATURE_SPECTATOR ), Group( "Debug" )]
	public void PreviousMode() => CycleMode( -1 );

	/// <summary>
	/// Choose the next mode available.
	/// </summary>
	[Button( "Next Target" )]
	[Feature( FEATURE_SPECTATOR ), Group( "Debug" )]
	[HideIf( nameof( InEditor ), true )]
	public void NextTarget()
	{
		this.Warn( "Targeting is not yet implemented." );
	}

	/// <summary>
	/// Choose the previous mode available.
	/// </summary>
	[Button( "Previous Target" )]
	[Feature( FEATURE_SPECTATOR ), Group( "Debug" )]
	[HideIf( nameof( InEditor ), true )]
	public void PreviousTarget()
	{
		this.Warn( "Targeting is not yet implemented." );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		switch ( Mode )
		{
			case SpectatorModes.FirstPerson:
				FirstPersonUpdate();
				break;
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		switch ( Mode )
		{
			case SpectatorModes.FreeRoam:
			case SpectatorModes.FirstPerson:
			case SpectatorModes.ThirdPerson:
				// rndtrash: There is no need for a FixedUpdate here... not yet
				break;
		}
	}
}
