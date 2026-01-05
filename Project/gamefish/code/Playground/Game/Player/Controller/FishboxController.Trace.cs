using GameFish;
using ShrimpleCharacterController;
using SCC = ShrimpleCharacterController.ShrimpleCharacterController;

namespace Playground;

partial class FishboxController
{
	public SceneTraceResult GroundTrace { get; set; }
	public Surface GroundSurface { get; set; }
	public float SurfaceFriction { get; set; }

	public virtual void DoGroundTrace()
	{
		if ( !ShrimpleController.IsValid() || !_c.IsOnGround )
			return;

		var startPos = WorldPosition;
		var down = WorldRotation.Down;

		GroundTrace = Trace( startPos, startPos + down * 2f ).Run();
		GroundSurface = GroundTrace.Surface;
		SurfaceFriction = GroundTrace.Surface?.Friction ?? 1f;
	}

	public override SceneTrace Trace()
	{
		if ( !Scene.IsValid() || !ShrimpleController.IsValid() )
			return default;

		var scale = WorldScale;
		var height = (LocalEyePosition.z - SkinWidth).Max( 1f ) * scale.z;
		var radius = _c.TraceWidth.Max( 1f ) * scale.x.Abs();

		var tr = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( _c.IgnoreTags )
			.Cylinder( height, radius ); // squadala

		// if ( _c.RotateWithGameObject )
		tr = tr.Rotated( WorldRotation );

		return tr;
	}

	public override SceneTrace Trace( Vector3 from, Vector3 to )
	{
		var vCenter = Up * LocalEyePosition.z * 0.5f;

		from += vCenter;
		to += vCenter;

		return base.Trace( from, to );
	}
}
