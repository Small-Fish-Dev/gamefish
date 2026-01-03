
namespace Playground;

public partial class BoxTool : ShapeTool
{
	[Property]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public Vector3 BoxSize { get; set; } = 50f;

	public override int PointLimit => 2;

	protected override bool TryGetShapeTransform( out Transform tWorld )
	{
		if ( !base.TryGetShapeTransform( out tWorld ) )
			return false;

		tWorld.Scale /= BoxSize;

		return ITransform.IsValid( tWorld );
	}
}
