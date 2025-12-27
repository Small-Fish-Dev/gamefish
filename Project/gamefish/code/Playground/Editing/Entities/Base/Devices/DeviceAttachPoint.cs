namespace Playground;

public partial struct DeviceAttachPoint : IValid
{
	public readonly bool IsValid => Object.IsValid()
		&& Position.HasValue && HitNormal.HasValue
		&& Offset.HasValue;

	public GameObject Object { get; set; }
	public Collider Collider { get; set; }

	public Vector3? Position { get; set; }
	public Vector3? HitNormal { get; set; }

	public Offset? Offset { get; set; }

	public DeviceAttachPoint() { }

	public DeviceAttachPoint( in SceneTraceResult tr, in Vector3? worldPos = null, in bool withNormal = true )
	{
		Object = tr.GameObject;
		Collider = tr.Collider;

		Position = worldPos ?? tr.EndPosition;
		HitNormal = tr.Normal;

		if ( Object.IsValid() )
		{
			var tWorld = Object.WorldTransform;

			var pos = tWorld.PointToLocal( worldPos ?? tr.EndPosition );

			var r = withNormal
				? tWorld.RotationToLocal( Rotation.LookAt( tr.Normal ) )
				: Rotation.Identity;

			Offset = new( pos, r );
		}
	}

	public DeviceAttachPoint( in SceneTraceResult tr, in Offset offset )
	{
		Object = tr.GameObject;
		Collider = tr.Collider;

		Offset = offset;
		HitNormal = tr.Normal;

		if ( Object.IsValid() )
		{
			var tWorld = Object.WorldTransform;
			Position = tWorld.WithOffset( offset ).Position;
		}
		else
		{
			Position = tr.EndPosition;
		}
	}
}
