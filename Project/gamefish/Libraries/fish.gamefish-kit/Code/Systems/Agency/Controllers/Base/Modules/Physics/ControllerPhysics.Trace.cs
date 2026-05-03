using System;

namespace GameFish;

partial class ControllerPhysics
{
	/// <summary>
	/// Movement/collision logic tries to stay this far away
	/// from surfaces to prevent getting stuck in them.
	/// </summary>
	[Property]
	[Range( 0.01f, 5f, clamped: false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public float SkinWidth { get; set; } = 0.5f;

	public virtual ITagSet TraceTags => Tags;

	public Transform Origin => Pawn?.WorldTransform ?? WorldTransform;

	/// <summary>
	/// This is where solid object filters and such go.
	/// </summary>
	/// <returns> The basis of every collison trace. </returns>
	public virtual SceneTrace Trace()
	{
		if ( !Scene.IsValid() )
			return default;

		var tr = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject );

		return tr;
	}

	/// <summary>
	/// Creates the default collision trace and sets the start and end points.
	/// </summary>
	/// <returns> The basis of every collison trace(including a start/end). </returns>
	public SceneTrace Trace( in Vector3 from, in Vector3 to )
	{
		var tFrom = Origin.WithPosition( from );

		return Trace( in tFrom, in to );
	}

	/// <summary>
	/// Creates the default collision trace and sets the end point relative to our starting position.
	/// </summary>
	/// <returns> The basis of every collison trace(including a start/end). </returns>
	public SceneTrace Trace( in Vector3 vDelta )
	{
		var tFrom = Origin;
		var to = tFrom.Position + vDelta;

		return Trace( in tFrom, in to );
	}

	/// <summary>
	/// Creates the default collision trace and sets the start and end transforms.
	/// </summary>
	/// <returns> The basis of every collison trace(including a start/end). </returns>
	public virtual SceneTrace Trace( in Transform tFrom, in Vector3 to )
		=> Trace().FromTo( tFrom, to );

	public virtual bool IsEmpty( in Vector3 pos, out SceneTraceResult trEmpty )
	{
		trEmpty = Trace( pos, pos ).Run();
		return !trEmpty.StartedSolid;
	}
}
