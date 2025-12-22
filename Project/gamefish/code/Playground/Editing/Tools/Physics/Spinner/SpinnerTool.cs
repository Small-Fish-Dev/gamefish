namespace Playground;

public partial class SpinnerTool : EditorTool
{
	[Property]
	[Title( "Spinner" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile SpinnerPrefab { get; set; }

	[Property]
	[Title( "Attach" )]
	[Feature( EDITOR ), Group( SOUNDS ), Order( SOUNDS_ORDER )]
	public virtual SoundEvent AttachingSound { get; set; }

	/// <summary>
	/// The key you press to activate the thruster you're placing.
	/// </summary>
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual SpinnerSettings SpinnerSettings { get; set; }

	public Rigidbody Target { get; set; }

	public Vector3 HitPosition { get; set; }
	public Vector3 HitNormal { get; set; }

	public override void OnExit()
	{
		base.OnExit();

		Target = null;
	}

	public override bool TryLeftClick()
	{
		if ( TryAttachSpinner( Target, HitPosition, HitNormal ) )
			if ( AttachingSound.IsValid() )
				Sound.Play( AttachingSound, HitPosition );

		return true;
	}

	public override void FrameSimulate( in float deltaTime )
	{
		Target = null;

		if ( !IsClientAllowed( Client.Local ) )
			return;

		if ( !TryTrace( out var tr ) )
			return;

		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return;

		// Clear Spinners
		if ( PressedReload )
			TryClearSpinners( tr.GameObject );

		const FindMode findMode = FindMode.EnabledInSelf | FindMode.InAncestors;

		if ( !tr.GameObject.Components.TryGet<Rigidbody>( out var rb, findMode ) )
			return;

		Target = rb;

		HitPosition = tr.HitPosition;
		HitNormal = tr.Normal;

		// Placement Preview
		if ( CanTarget( Client.Local, rb, HitPosition, HitNormal ) )
			DrawSpinnerGizmo( HitPosition, HitNormal );
	}

	protected virtual void DrawSpinnerGizmo( in Vector3 hitPos, in Vector3 hitNormal )
	{
		var c = Color.Cyan.WithAlpha( 0.3f );

		this.DrawArrow(
			from: hitPos + (hitNormal * 64f),
			to: hitPos,
			c: c, len: 8f, w: 3f,
			tWorld: global::Transform.Zero
		);
	}

	protected virtual bool TryAttachSpinner( Rigidbody rb, in Vector3 hitPos, in Vector3 hitNormal )
	{
		if ( !rb.IsValid() )
			return false;

		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !CanTarget( Client.Local, rb, in hitPos, in hitNormal ) )
			return false;

		var rAim = Rotation.LookAt( -hitNormal );
		var tWorld = new Transform( hitPos, rAim );

		if ( !SpinnerPrefab.TrySpawn( tWorld, out var thrusterObj ) )
			return false;

		thrusterObj.NetworkInterpolation = false;

		if ( !thrusterObj.Components.TryGet<Spinner>( out var thruster ) )
		{
			this.Warn( $"No {typeof( Spinner )} on obj:[{thrusterObj}]!" );
			thrusterObj.Destroy();
			return false;
		}

		thruster.Settings = SpinnerSettings;
		thruster.Offset = rb.WorldTransform.ToLocal( tWorld );

		thruster.TrySetNetworkOwner( Connection.Local, allowProxy: true );

		if ( !thruster.TryAttachTo( rb, thruster.Offset ) )
		{
			this.Warn( $"Couldn't attach thruster:[{thruster}] to rb:{rb}!" );
			thrusterObj.Destroy();
			return false;
		}

		return true;
	}

	protected virtual bool TryClearSpinners( GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		var thrusters = obj.Components.GetAll<Spinner>( FindMode.EverythingInSelfAndDescendants );

		if ( !thrusters.Any( th => th.IsValid() ) )
			return false;

		RpcRemoveSpinners( obj );
		return true;
	}

	public virtual bool CanTarget( Client cl, Rigidbody rb, in Vector3 hitPos, in Vector3 hitNormal )
	{
		if ( !rb.IsValid() )
			return false;

		if ( Pawn.TryGet( rb.GameObject, out _ ) )
			return false;

		return ITransform.IsValid( in hitPos ) && ITransform.IsValid( in hitNormal );
	}

	[Rpc.Host]
	protected void RpcRemoveSpinners( GameObject obj )
	{
		if ( !obj.IsValid() || !TryUse( Rpc.Caller, out _ ) )
			return;

		const FindMode findMode = FindMode.EverythingInSelf | FindMode.InDescendants;

		var thrusters = obj.Components.GetAll<Spinner>( findMode );

		if ( !thrusters.Any() )
			return;

		foreach ( var th in thrusters.ToArray() )
			th.Destroy();
	}
}
