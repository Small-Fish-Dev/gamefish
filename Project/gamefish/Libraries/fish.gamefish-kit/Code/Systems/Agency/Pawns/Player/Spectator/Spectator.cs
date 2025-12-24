namespace GameFish;

[Icon( "control_camera" )]
[EditorHandle( Icon = "👻" )]
public partial class Spectator : Player
{
	/// <summary>
	/// The target that this is actively spectating.
	/// </summary>
	[Sync]
	[Property]
	[Feature( SPECTATOR ), Group( DEBUG )]
	public Pawn Spectating
	{
		get => _spectating;

		protected set
		{
			var prevSpec = _spectating;

			_spectating = value;

			if ( this.IsOwner() )
				OnSpectatingSet( prevSpec, value );
		}
	}

	protected Pawn _spectating;

	/// <summary>
	/// The button that spectates a target.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Spectate Target" )]
	[Feature( SPECTATOR ), Group( INPUT )]
	public virtual string SpectateTargetAction { get; set; } = "Use";
	public virtual bool AllowSpectateTarget => !string.IsNullOrWhiteSpace( SpectateTargetAction );

	/// <summary>
	/// The button that spectates a target.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Stop Spectating" )]
	[Feature( SPECTATOR ), Group( INPUT )]
	public virtual string StopSpectatingAction { get; set; } = "Reload";
	public virtual bool AllowStopSpectating => !string.IsNullOrWhiteSpace( StopSpectatingAction );

	/// <summary>
	/// The button that cycles forward to the next possible target.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Spectate Previous" )]
	[Feature( SPECTATOR ), Group( INPUT )]
	public virtual string SpectatePreviousAction { get; set; } = "Attack1";
	public virtual bool AllowSpectatePrevious => !string.IsNullOrWhiteSpace( SpectatePreviousAction );

	/// <summary>
	/// The button that cycles forward to the next possible target.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Spectate Next" )]
	[Feature( SPECTATOR ), Group( INPUT )]
	public virtual string SpectateNextAction { get; set; } = "Attack2";
	public virtual bool AllowSpectateNext => !string.IsNullOrWhiteSpace( SpectateNextAction );

	/// <summary> How fast the spectator is moving. </summary>
	[Property]
	[Feature( ENTITY ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public override Vector3 Velocity { get; set; }

	/// <summary> Spectators can never be spectated. </summary>
	public override bool AllowSpectators => false;

	/// <summary> Spectators can never be spectated. </summary>
	public override bool AllowSpectator( Pawn spec )
		=> false;

	protected override void OnEnabled()
	{
		Tags?.Add( TAG_SPECTATOR );

		base.OnEnabled();
	}

	protected override void OnTaken( Agent newAgent, Agent oldAgent = null )
	{
		base.OnTaken( newAgent, oldAgent );

		if ( !newAgent.IsOwner() )
			return;

		if ( Scene?.Camera?.WorldTransform is not Transform tView )
			return;

		if ( View is not PawnView view || !view.IsValid() )
			return;

		view.ViewPosition = tView.Position;
		view.ViewRotation = tView.Rotation;

		view.StartTransition( useWorldPosition: true );
		view.UpdateViewTransform();
	}

	public override void FrameSimulate( in float deltaTime )
	{
		DoAiming( in deltaTime );

		base.FrameSimulate( deltaTime );
	}

	protected override void Move( in float deltaTime, in bool isFixedUpdate )
	{
		if ( !Controller.IsValid() || !Spectating.IsValid() )
		{
			DoFlying( in deltaTime );
			return;
		}

		base.Move( in deltaTime, in isFixedUpdate );
	}

	protected override void UpdateInput( in float deltaTime )
	{
		if ( !IsPlayer )
			return;

		if ( Mouse.Active || IFocus.Action )
			return;

		if ( Spectating.IsValid() )
		{
			if ( AllowStopSpectating && Input.Pressed( StopSpectatingAction ) )
				StopSpectating();
		}
		else
		{
			if ( AllowSpectateTarget && Input.Pressed( SpectateTargetAction ) )
				SpectateTarget();
		}

		if ( AllowSpectatePrevious && Input.Pressed( SpectatePreviousAction ) )
			SpectatePrevious();

		if ( AllowSpectateNext && Input.Pressed( SpectateNextAction ) )
			SpectateNext();
	}

	/// <summary>
	/// Called whenever <see cref="Spectating"/> has been set.
	/// </summary>
	protected virtual void OnSpectatingSet( Pawn prev, Pawn next )
	{
		if ( !this.IsOwner() )
			return;

		var view = View;

		if ( !view.IsValid() )
			return;

		if ( next.IsValid() )
			view.StartTransition( useWorldPosition: true );
		else
			view.TryEnterFirstPerson();
	}

	public override bool CanSpectate( Pawn target )
	{
		if ( !this.IsValid() || !target.IsValid() )
			return false;

		if ( target == this )
			return false;

		return target.AllowSpectator( this );
	}

	public override bool TrySpectate( Pawn target )
	{
		if ( !this.IsOwner() )
			return false;

		if ( !CanSpectate( target ) )
			return false;

		Spectating = target;

		return true;
	}

	/// <summary>
	/// Called by the host to tell us to spectate a pawn. Ignores filters.
	/// </summary>
	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void ForceSpectate( Pawn target )
		=> Spectating = target;

	/// <summary>
	/// Kicks the spectator out of the fuggen thing, man.
	/// </summary>
	[Button]
	[Feature( SPECTATOR ), Group( DEBUG )]
	public void StopSpectating()
		=> Spectating = null;

	/// <summary>
	/// Try to spectate what we're looking at.
	/// </summary>
	public virtual void SpectateTarget()
	{
	}

	public virtual IEnumerable<Pawn> GetAllowedTargets()
		=> GetAllActive<Pawn>().Where( CanSpectate );

	public virtual void CycleSpectating( int dir )
	{
		var targets = GetAllowedTargets().ToList();

		if ( targets.Count <= 0 )
		{
			// this.Log( $"no targets to cycle:[{dir}]" );
			return;
		}

		var iTarget = targets.IndexOf( Spectating );
		var iNext = iTarget == -1 ? 0 : (iTarget + dir).UnsignedMod( targets.Count );

		var target = targets.ElementAtOrDefault( iNext );

		// TODO: Upon failure remove failed target and continue cycling until empty.
		if ( !TrySpectate( target ) )
			this.Warn( $"failed to spectate target:[{target}] index:[{iNext}]" );
	}

	public virtual void SpectateNext()
		=> CycleSpectating( 1 );

	public virtual void SpectatePrevious()
		=> CycleSpectating( -1 );
}
