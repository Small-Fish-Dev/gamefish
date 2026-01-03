namespace Playground;

/// <summary>
/// Places something as a child(shrimply all for now).
/// </summary>
public partial class DeviceTool : PrefabTool
{
	[Property]
	[ToolSetting]
	[Range( 0f, 360f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float Yaw { get; set; } = 0f;

	public override float Distance => 4096f;

	protected override void OnScroll( in float scroll )
	{
		Yaw = (Yaw + scroll).NormalizeDegrees();

		base.OnScroll( scroll );
	}

	public override bool TrySetTarget( in SceneTraceResult tr, Component target )
	{
		if ( !base.TrySetTarget( in tr, target ) )
			return false;

		if ( !tr.Collider.IsValid() || tr.Collider.Static )
		{
			ClearTarget();
			return false;
		}

		var targetPos = tr.StartPosition + (tr.Direction * Distance.Min( tr.Distance ));

		var flatDir = Vector3.VectorPlaneProject( Vector3.Forward, tr.Normal );
		var rTarget = Rotation.LookAt( flatDir, tr.Normal );

		rTarget *= Rotation.FromAxis( rTarget.Inverse * tr.Normal, Yaw );

		TargetTransform = new Transform( targetPos, rTarget );

		return true;
	}

	protected override bool TrySpawnAtTarget( out EditorObject e )
	{
		if ( !base.TrySpawnAtTarget( out e ) )
			return false;

		if ( TargetObject.IsValid() )
			e.GameObject.SetParent( TargetObject, keepWorldPosition: true );

		return true;
	}
}
