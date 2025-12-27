namespace Playground;

/// <summary>
/// Places something as a child(shrimply all for now).
/// </summary>
public partial class DeviceTool : PrefabTool
{
	public GameObject TargetObject { get; set; }

	protected override bool TrySetTarget( in SceneTraceResult tr )
	{
		TargetObject = tr.GameObject;

		HasTarget = TargetObject.IsValid()
			&& tr.Collider.IsValid() && !tr.Collider.Static
			&& tr.Distance <= Distance;

		var targetPos = tr.StartPosition + (tr.Direction * Distance.Min( tr.Distance ));
		TargetTransform = new Transform( targetPos, GetPrefabRotation() );

		return true;
	}

	protected override bool TryPlaceAtTarget( out GameObject obj )
	{
		if ( !base.TryPlaceAtTarget( out obj ) )
			return false;

		if ( TargetObject.IsValid() )
			obj.SetParent( TargetObject, keepWorldPosition: true );

		return true;
	}
}
