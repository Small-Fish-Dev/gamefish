namespace GameFish;

/// <summary>
/// Simplified physics data about one thing hitting another.
/// <br /> <br />
/// <b> NOTE: </b> Useful for objects with custom movement.
/// </summary>
[Icon( "subdirectory_arrow_right" )]
public partial struct ImpactData : IValid
{
	/// <summary>
	/// Returns true if this data has a hit object and hit normal defined.
	/// </summary>
	public readonly bool IsValid => GameObject.IsValid() && HitNormal != default;

	/// <summary> The object causing the impact. </summary>
	public GameObject Mover { get; set; }

	/// <summary> The impacted object. </summary>
	public GameObject GameObject { get; set; }
	/// <summary> The impacted collider. </summary>
	public Collider Collider { get; set; }
	/// <summary> The impacted hitbox. </summary>
	public Hitbox Hitbox { get; set; }

	public Vector3 HitPosition { get; set; }
	public Vector3 HitNormal { get; set; }

	/// <summary>
	/// The position the mover would end up at. <br />
	/// Not necessary to define if it's already there.
	/// </summary>
	public Vector3? EndPosition { get; set; }

	public static implicit operator ImpactData( in Collision c )
		=> new( in c );

	public ImpactData() { }

	/// <summary>
	/// Creates impact data manually.
	/// </summary>
	public ImpactData( GameObject mover, GameObject hitObj, in Vector3 hitPos, in Vector3 hitNormal, Collider cHit = null, Hitbox hitbox = null, in Vector3? endPos = null )
	{
		Mover = mover;

		GameObject = hitObj;
		Collider = cHit;
		Hitbox = hitbox;

		HitPosition = hitPos;
		HitNormal = hitNormal;

		EndPosition = endPos;
	}

	/// <param name="c"> The engine's collision result. </param>
	public ImpactData( in Collision c )
	{
		Mover = c.Self.GameObject;

		GameObject = c.Other.GameObject;
		Collider = c.Other.Collider;
		Hitbox = null;

		HitPosition = c.Contact.Point;
		HitNormal = c.Contact.Normal;

		EndPosition = null;
	}

	/// <param name="mover"> The object causing the collision. </param>
	/// <param name="tr"> The trace used to test for collision. </param>
	public ImpactData( GameObject mover, in SceneTraceResult tr )
	{
		Mover = mover;

		GameObject = tr.GameObject;
		Collider = tr.Collider;
		Hitbox = tr.Hitbox;

		HitPosition = tr.HitPosition;
		HitNormal = tr.Normal;

		EndPosition = tr.EndPosition;
	}
}
