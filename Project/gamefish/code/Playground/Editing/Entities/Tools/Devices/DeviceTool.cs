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

	protected override void OnObjectSpawned( EditorObject e, EditorIsland parent )
	{
		base.OnObjectSpawned( e, parent );

		if ( !parent.IsValid() )
			return;

		var toDestroy = parent.FindObjects()
			.Where( d => d is CenteringDevice && d != e );

		if ( !toDestroy.Any() )
			return;

		// TODO: Have the host do this so it always works.
		foreach ( var cd in toDestroy.ToArray() )
			cd.Destroy();
	}

	protected override bool TryGetPointer( in SceneTraceResult tr, out Transform tPointer )
	{
		if ( !base.TryGetPointer( tr, out tPointer ) )
			return false;

		tPointer.Rotation *= Rotation.FromYaw( Yaw );

		return true;
	}

	protected override void OnScroll( in float scroll )
	{
		Yaw = (Yaw + scroll).NormalizeDegrees();

		base.OnScroll( scroll );
	}

	protected override SceneTraceResult RunTrace( in Ray ray )
		=> RunRayTrace( in ray );
}
