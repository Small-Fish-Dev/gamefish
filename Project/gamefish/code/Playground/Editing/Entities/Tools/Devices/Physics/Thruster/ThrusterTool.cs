namespace Fishbox;

public partial class ThrusterTool : JointTool
{
	[Property]
	[Title( "Thruster" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile ThrusterPrefab { get; set; }

	[Property]
	[Title( "Place Thruster" )]
	[Feature( EDITOR ), Group( SOUNDS ), Order( SOUNDS_ORDER )]
	public virtual SoundEvent AttachingSound { get; set; }

	[Property, InlineEditor]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual ThrusterSettings ThrusterSettings { get; set; }

	public override bool ValidTarget( Client cl, in SceneTraceResult tr )
	{
		return base.ValidTarget( cl, tr );
	}

	protected override bool TryAddPoint( in DeviceAttachPoint point )
	{
		if ( !point.Object.IsValid() )
			return false;

		if ( point.Offset is not Offset offset )
			return false;

		return TryAttachThruster( point.Object, offset );
	}

	protected override bool TryGetAttachPoint( GameObject obj, Component target, in SceneTraceResult tr, out DeviceAttachPoint point )
	{
		if ( !base.TryGetAttachPoint( obj, target, tr, out point ) )
			return false;

		if ( point.Offset is not Offset offset )
			return false;

		point.Offset = offset with { Rotation = offset.Rotation * Rotation.FromYaw( 180f ) };

		return true;
	}

	protected virtual bool TryAttachThruster( GameObject objTarget, Offset offset )
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !objTarget.IsValid() )
			return false;

		var tWorld = objTarget.WorldTransform.ToLocal( offset );

		if ( !TrySpawnObject( ThrusterPrefab, tWorld: tWorld, out var e, withIsland: false ) )
			return false;

		var eObj = e.GameObject;
		eObj.NetworkInterpolation = false;

		if ( !eObj.Components.TryGet<Thruster>( out var thruster ) )
		{
			this.Warn( $"No {typeof( Thruster )} on obj:[{eObj}]!" );
			eObj.Destroy();
			return false;
		}

		thruster.Settings = ThrusterSettings;
		thruster.Offset = offset;

		if ( !thruster.TryAttachTo( objTarget, thruster.Offset ) )
		{
			this.Warn( $"Couldn't attach thruster:[{thruster}] to obj:{objTarget}!" );
			eObj.Destroy();
			return false;
		}

		return true;
	}

	protected virtual bool TryClearThrusters( GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		var thrusters = obj.Components.GetAll<Thruster>( FindMode.EverythingInSelfAndDescendants );

		if ( !thrusters.Any( th => th.IsValid() ) )
			return false;

		RpcRemoveJoints( obj );
		return true;
	}

	[Rpc.Host]
	protected override void RpcRemoveJoints( GameObject obj )
	{
		if ( !obj.IsValid() || !TryUse( Rpc.Caller, out _ ) )
			return;

		const FindMode findMode = FindMode.EverythingInSelf | FindMode.InDescendants;

		var thrusters = obj.Components.GetAll<Thruster>( findMode );

		if ( !thrusters.Any() )
			return;

		foreach ( var th in thrusters.ToArray() )
			th.Destroy();
	}

	public override bool TryClear( GameObject obj )
	{
		throw new System.NotImplementedException();
	}

	public override bool TryAttach( in DeviceAttachPoint point1, in DeviceAttachPoint point2 )
		=> false;
}
