namespace GameFish;

/// <summary>
/// A trigger with the "ladder" tag and appropriate default collider. <br />
/// You need a pawn controller of some kind to utilize this. <br />
/// Capable of creating, updating and previewing its collision.
/// <code> func_ladder </code>
/// </summary>
[Icon( "stairs" )]
[EditorHandle( Icon = "🧗‍" )]
public partial class LadderTrigger : Trigger
{
	protected override ColliderType DefaultColliderType => ColliderType.Box;
	protected override BBox DefaultBoxSize => new( new Vector3( 0, -16, 0f ), new Vector3( 12f, 16f, 256f ) );

	public override TagSet DefaultTags { get; } = [TAG_TRIGGER, TAG_LADDER];
	public override Color DefaultGizmoColor { get; } = Color.Orange.Desaturate( 0.3f ).Darken( 0.1f );
}
