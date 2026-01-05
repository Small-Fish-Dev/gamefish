using GameFish;
using ShrimpleCharacterController;
using SCC = ShrimpleCharacterController.ShrimpleCharacterController;

namespace Playground;

partial class FishboxController
{
	public SceneTraceResult GroundTrace { get; set; }
	public Surface GroundSurface { get; set; }
	public float SurfaceFriction { get; set; }

	public virtual Vector3 TraceOffset => GetCenterOffset();

	public virtual void DoGroundTrace()
	{
		if ( !ShrimpleController.IsValid() || !_c.IsOnGround )
			return;

		var startPos = WorldPosition;
		var down = WorldRotation.Down;

		GroundTrace = Trace( startPos, startPos + down * 2f, TraceOffset ).Run();
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

		tr = tr.Rotated( WorldRotation );

		return tr;
	}

	protected virtual Vector3 GetCenterOffset()
	{
		return Up * LocalEyePosition.z * 0.5f;
	}

	public virtual bool TryUnstuck( out Vector3 result )
	{
		if ( !ShrimpleController.IsValid() )
		{
			result = default;
			return false;
		}

		return _c.TryUnstuck( WorldPosition + GetCenterOffset(), out result );
	}
}
