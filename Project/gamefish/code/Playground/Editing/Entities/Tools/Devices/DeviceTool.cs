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
		=> RunDefaultTrace( in ray );
}
