using System.ComponentModel;

namespace Playground;

public partial class GrabberTool : EditorTool
{
	[Property]
	[Title( "Hand" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile HandPrefab { get; set; }

	public GrabberHand Hand { get; set; }

	public float GrabDistance { get; set; }

	public override void FrameSimulate( in float deltaTime )
	{
		if ( Input.Pressed( "Attack1" ) )
			TryGrabTarget();

		if ( !Hand.IsValid() || !TryTrace( out var tr ) )
			return;

		if ( GrabDistance > 0f )
			Hand.WorldPosition = tr.StartPosition + tr.Direction * GrabDistance;
	}

	protected virtual bool TryGrabTarget()
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !TryTrace( out var tr ) || !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		if ( !CanGrab( Client.Local, tr.GameObject ) )
			return false;

		GrabDistance = tr.Distance;

		var hitPos = tr.HitPosition;
		var rAim = Rotation.LookAt( tr.Direction );

		if ( !Hand.IsValid() )
		{
			if ( !HandPrefab.TrySpawn( in hitPos, in rAim, out var handObj ) )
				return false;

			if ( !handObj.Components.TryGet<GrabberHand>( out var hand ) )
			{
				handObj.Destroy();
				return false;
			}

			Hand = hand;
		}

		if ( !Hand.IsValid() )
			return false;

		Hand.WorldPosition = hitPos;
		Hand.WorldRotation = rAim;
		Hand.Transform.ClearInterpolation();

		Hand.BodyObject = tr.GameObject;

		return true;
	}

	public virtual bool CanGrab( Client cl, GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		return true;
	}
}
