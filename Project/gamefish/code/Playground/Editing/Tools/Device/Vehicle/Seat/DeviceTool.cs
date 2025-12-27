namespace Playground;

/// <summary>
/// Places something as a child(shrimply all for now).
/// </summary>
public partial class DeviceTool : PrefabTool
{
	public GameObject TargetObject { get; set; }

	[Property]
	[ToolOption]
	[Range( 0f, 360f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float Yaw { get; set; } = 0f;

	public override float Distance => 4096f;

	protected override void OnScroll( in float scroll )
	{
		Yaw = (Yaw + scroll).NormalizeDegrees();

		base.OnScroll( scroll );
	}

	protected override bool TrySetTarget( in SceneTraceResult tr )
	{
		TargetObject = tr.GameObject;

		HasTarget = TargetObject.IsValid()
			&& tr.Collider.IsValid() && !tr.Collider.Static;

		var targetPos = tr.StartPosition + (tr.Direction * Distance.Min( tr.Distance ));

		var flatDir = Vector3.VectorPlaneProject( Vector3.Forward, tr.Normal );
		var rTarget = Rotation.LookAt( flatDir, tr.Normal );

		rTarget *= Rotation.FromAxis( rTarget.Inverse * tr.Normal, Yaw );

		TargetTransform = new Transform( targetPos, rTarget );

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
