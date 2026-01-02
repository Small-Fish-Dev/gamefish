namespace Playground;

public partial class UprightTool : JointTool
{
	[Property]
	[Feature( EDITOR ), Group( SOUNDS ), Order( SOUNDS_ORDER )]
	public virtual SoundEvent AttachingSound { get; set; }

	[Property, InlineEditor]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual UprightSettings UprightSettings { get; set; }

	protected override bool TryAddPoint( in DeviceAttachPoint point )
	{
		if ( !point.Object.IsValid() )
			return false;

		if ( point.Offset is not Offset offset )
			return false;

		return TryAttachUpright( point.Object, offset );
	}

	protected virtual bool TryAttachUpright( GameObject objTarget, Offset offset )
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !objTarget.IsValid() )
			return false;

		var tWorld = objTarget.WorldTransform.ToLocal( offset );

		if ( !TrySpawnObject( JointPrefab, tWorld: tWorld, out var e, withIsland: false ) )
			return false;

		var eObj = e.GameObject;
		eObj.NetworkInterpolation = false;

		if ( !eObj.Components.TryGet<UprightDevice>( out var Upright ) )
		{
			this.Warn( $"No {typeof( UprightDevice )} on obj:[{eObj}]!" );
			eObj.Destroy();
			return false;
		}

		Upright.Settings = UprightSettings;
		Upright.Offset = offset;

		if ( !Upright.TryAttachTo( objTarget, Upright.Offset ) )
		{
			this.Warn( $"Couldn't attach Upright:[{Upright}] to obj:{objTarget}!" );
			eObj.Destroy();
			return false;
		}

		return true;
	}

	protected virtual bool TryClearUprights( GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		var Uprights = obj.Components.GetAll<UprightDevice>( FindMode.EverythingInSelfAndDescendants );

		if ( !Uprights.Any( th => th.IsValid() ) )
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

		var Uprights = obj.Components.GetAll<UprightDevice>( findMode );

		if ( !Uprights.Any() )
			return;

		foreach ( var th in Uprights.ToArray() )
			th.Destroy();
	}

	public override bool TryClear( GameObject obj )
	{
		throw new System.NotImplementedException();
	}

	public override bool TryAttach( in DeviceAttachPoint point1, in DeviceAttachPoint point2 )
		=> false;
}
